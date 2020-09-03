#include <gforce.h>

/**********************************************************************
 * GFORCE EMG ARMBAND EXAMPLE
 * Author: Davide Asnaghi
 * Date: April 3rd 2019
 * Requirements: 
 *    - ESP32 arduino (https://github.com/espressif/arduino-esp32.git)
 *    - ESP32 gForce library (https://github)
 **********************************************************************/

#define PRINT_IMU     1
#define PRINT_GESTURE 1
#define PRINT_STATUS  1

// Array for data visualization
String gesture2string[] = {"RELAX",
                           "FIST",
                           "SPREAD FINGERS",
                           "WAVE_TOWARD_PALM",
                           "WAVE_BACKWARD_PALM",
                           "TUCK_FINGERS",
                           "SHOOT"
                          };
String status2string[] = {"NONE",
                          "BUTTTON_PRESSED"
                         };

// BLE Armband: gForce
armband gForce;


//BLE AUTOMATIC CALLBACK WHEN RECEIVING DATA
void dataCallback(BLERemoteCharacteristic* pBLERemoteCharacteristic, uint8_t* pData, size_t length, bool isNotify) {
  gForce.update(pData, length);
}

void setup()
{
  Serial.begin(115200);                      // Serial interface for debugging

  gForce.debug = false;                      // Enable/Disable verbose debugging

  Serial.println ("Connecting...");
  gForce.connect();                          // Connect to the armband
  Serial.println (" - Connected");

  gForce.data_notification(TURN_ON)->registerForNotify(dataCallback);
}

void loop()
{
  // Routine for updated IMU data
  if (gForce.imu.updated) {
    if (PRINT_IMU) {
      Serial.print("IMU: [ ");
      Serial.print(gForce.imu.w);
      Serial.print("\t");
      Serial.print(gForce.imu.x);
      Serial.print("\t");
      Serial.print(gForce.imu.y);
      Serial.print("\t");
      Serial.print(gForce.imu.z);
      Serial.println(" ]");
    }
    /* USER IMU CODE BEGIN */

    /* USER IMU CODE END */
    gForce.imu.updated = false;
  }

  // Routine for updated gesture data
  if (gForce.gesture.updated) {
    if (PRINT_GESTURE) {
      Serial.print("GESTURE: [ ");
      Serial.print(gesture2string[gForce.gesture.type]);
      Serial.println(" ]");
    }
    /* USER GESTURE CODE BEGIN */
    
    /* USER GESTURE CODE END */
    gForce.gesture.updated = false;
  }

  // Routine for updated status data
  if (gForce.status.updated) {
    if (PRINT_STATUS) {
      Serial.print("STATUS: [ ");
      Serial.print(status2string[gForce.status.value]);
      Serial.println(" ]");
    }
    /* USER STATUS CODE BEGIN */

    /* USER STATUS CODE END */
    gForce.status.updated = false;
  }

  // Detect disconnection and reconnect automatically
  if (!gForce.connected) {
    Serial.println ("Device disconnected: reconnecting...");
    gForce.connect();
    Serial.println (" - Connected");
    gForce.data_notification(TURN_ON)->registerForNotify(dataCallback);
  }

  // Delay of 10ms for stability
  delay (10);
}
