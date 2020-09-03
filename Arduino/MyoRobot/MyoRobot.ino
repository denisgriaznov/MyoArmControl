#include <myo.h>
#include <ESP32Servo.h> 
#include <EloquentTinyML.h>
#include "model.h"


Servo servoHorizontal;
Servo servoVertical;

int angleHorizontal = 0;
int angleVertical = 0;

int8_t angleShift;
int8_t emgCounter = 0;
int servoHorizontalPin = 21;
int servoVerticalPin = 22;

const uint8_t sizeEmg = 64;
const uint8_t sizeBuffer = 5;

armband myo;               
uint8_t emg0[16];
uint8_t emg1[16];
uint8_t emg2[16];
uint8_t emg3[16];
uint8_t currentemg[16];
uint16_t measured;
float norm[sizeEmg] = {0};
int8_t sum;
int8_t max_val; 
int8_t emptyGesture = 0;
int8_t gestureData;
int8_t gesture; 
int8_t generalGesture; 
int8_t sameCounter;

int8_t gesturesBuffer[sizeBuffer] = {4}; 
int8_t gesturesCounter[5] = {0}; 


float features[8] = {100.0, 20.0, 34.0, 25.0, 22.0, 48.0, 20.0, 42.0};
float output[4];
float max_ges;
const char* GESTURES[] = {
  "vawe out",
  "vawe in",
  "goat",
  "palm",
  "empty"
};

#define NUMBER_OF_INPUTS 16
#define NUMBER_OF_OUTPUTS 4
#define TENSOR_ARENA_SIZE 8*1024

Eloquent::TinyML::TfLite<NUMBER_OF_INPUTS, NUMBER_OF_OUTPUTS, TENSOR_ARENA_SIZE> ml(model_data);

void updateBuffer(uint8_t newGesture){
  for (int i = 0;i < sizeBuffer - 1;i++){
    gesturesBuffer[i] = gesturesBuffer[i+1];
  }
  gesturesBuffer[sizeBuffer - 1] = newGesture;
}

void updateEMG(uint8_t* emgArray,uint8_t* pData,size_t length){
  for (int i = 0; i < length; i++) {
    emgArray[i] = pData[i];
  }  
}

void emgCallback(BLERemoteCharacteristic* pBLERemoteCharacteristic, uint8_t* pData, size_t length, bool isNotify) {
    if (emgCounter < 3){
      emgCounter = emgCounter + 1;
    } else {
      emgCounter = 0;
    }
    Serial.println (emgCounter);
}


void myoInit(){
  myo.connect();   
  Serial.println (" - Connected");
  delay(100);
  myo.set_myo_mode(myohw_emg_mode_send_emg,         // EMG mode
                   myohw_imu_mode_none,             // IMU mode
                   myohw_classifier_mode_disabled);  // Classifier mode
  myo.emg_notification0(TURN_ON)->registerForNotify(emgCallback);
  myo.emg_notification1(TURN_ON)->registerForNotify(emgCallback);
  myo.emg_notification2(TURN_ON)->registerForNotify(emgCallback);
  myo.emg_notification3(TURN_ON)->registerForNotify(emgCallback); 
}

void printArray(uint8_t* arr){
  for(int i = 0;i<sizeEmg;i++){
    Serial.print(arr[i]);
    Serial.print(",");
  }
  Serial.print(gesture);
  Serial.println();
}
void dataProcessing(){
  measured = 0;
    for (int i = 0; i < 16; i++) {
          sum = 0;
          sum = 127 - currentemg[i];
          sum = abs(sum);
          sum = sum - 127;
          sum = abs(sum);
          norm[i] = sum;
          measured = measured + sum;
    }

    if (measured < 300) {
      if (emptyGesture<32)
      emptyGesture = emptyGesture + 1;
    }
    if (measured > 300) {
      if (emptyGesture>30) {
        for (int i = 0; i < sizeBuffer; i++) {
            gesturesBuffer[i] = 4;
        }
      }
      emptyGesture = 0;
      max_val = 0;
      for (int i = 0; i < 16; i++) {
          if (norm[i] > max_val) max_val = norm[i];
      }
      for (int i = 0; i < 16; i++) {
          norm[i] = (norm[i])/max_val;
      }
      
// BLOCK FOR DATA COLLECTION
//      for (int i = 0; i < 16; i++) {
//        Serial.print(norm[i]);
//        Serial.print(",");
//      }
//        Serial.print(gestureData);
//        Serial.print("\n");
// BLOCK END

      ml.predict(norm,output);
      gesture = 0;
      max_ges = 0;
      for (int i = 0; i < 4; i++) {
        //Serial.print(output[i]);
        //Serial.print("\t");
        if (output[i] > max_ges) {
          max_ges = output[i];
          gesture = i;
        }
      }
      updateBuffer(gesture);
      //Serial.println(" ");

      for (int i = 0; i < 5; i++) {
        gesturesCounter[i] =0;
      }

      for (int i = 0; i < sizeBuffer; i++) {
          gesturesCounter[gesturesBuffer[i]]++;
      }
      for (int i = 0; i < 5; i++) {
        if (gesturesCounter[i] > 2){
          if (generalGesture == i){
              sameCounter++;
           } else {
              sameCounter = 0;
           }
          generalGesture = i;
        }
      } 
      if (sameCounter == 0)angleShift = 0;
      if ((sameCounter > 0)&&(sameCounter < 5)) angleShift = 1;
      if ((sameCounter > 4)&&(sameCounter < 8)) angleShift = 2;
      if ((sameCounter > 7)&&(sameCounter < 15)) angleShift = 3;
      if (sameCounter > 14) angleShift = 4;
      if ((generalGesture == 0)&&(angleHorizontal < 176)) {
        angleHorizontal = angleHorizontal + angleShift;
        servoHorizontal.write(angleHorizontal);
      }
      if ((generalGesture == 1)&&(angleHorizontal > 4)) {
        angleHorizontal = angleHorizontal - angleShift;
        servoHorizontal.write(angleHorizontal);
      }
      if ((generalGesture == 2)&&(angleVertical < 176)) {
        angleVertical = angleVertical + angleShift;
        servoVertical.write(angleVertical);
      }
      if ((generalGesture == 3)&&(angleVertical > 4)) {
        angleVertical = angleVertical - angleShift;
        servoVertical.write(angleVertical);
      }
      Serial.print("\t");
      Serial.print(GESTURES[generalGesture]);  
      Serial.print("\t");
      Serial.print(sameCounter); 
      Serial.print("\t");
      Serial.print("Horizontal ");
      Serial.print(angleHorizontal); 
      Serial.print("\t");
      Serial.print("Vertical ");
      Serial.print(angleVertical); 
      Serial.println();  
    } 
}

void setup()
{
  Serial.begin(115200);                            
  pinMode (2, OUTPUT);
  servoHorizontal.attach(servoHorizontalPin); 
  servoVertical.attach(servoVerticalPin); 

  myoInit();
  
  gestureData = 6;
  gesture = 4;
  generalGesture = 4;
  sameCounter = 0;
  for (int i = 0; i < sizeEmg; i++) {
        Serial.print(i);
        Serial.print(",");
      }
        Serial.print("gesture");
        Serial.print("\n");
}

void loop()
{

  // Detect disconnection
  if (!myo.connected) {
    myoInit();
  }
  Serial.println (millis());
  if (myo.connected) {
      dataProcessing()
  }
   delay (500);
}
