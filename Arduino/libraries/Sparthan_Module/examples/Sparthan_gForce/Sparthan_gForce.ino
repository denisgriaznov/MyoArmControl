#include <sparthan.h>
#include <gforce.h>

/**********************************************************************
 * SPARTHAN GFORCE EMG ARMBAND EXAMPLE
 * Author: Davide Asnaghi
 * Date: April 3rd 2019
 * Requirements: 
 *    - ESP32 arduino (https://github.com/espressif/arduino-esp32.git)
 *    - Sparthan ESP32 gForce library (https://github)
 *    - Sparthan motion library (https://github)
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

// Sparthan module
sparthan module;


//BLE AUTOMATIC CALLBACK WHEN RECEIVING DATA
void dataCallback(BLERemoteCharacteristic* pBLERemoteCharacteristic, uint8_t* pData, size_t length, bool isNotify) {
  gForce.update(pData, length);
}

void setup()
{
  module.begin();                            // Initialize the module

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
    switch (gForce.gesture.type) {
      case GESTURE_WAVE_TOWARD_PALM:
        module.set_position(0.1, 0.1, 0.1, 0.08, 0.08);
        break;
      case GESTURE_WAVE_BACKWARD_PALM:
        module.set_position(0, 0, 0, 0, 0);
        break;
      default:
        break;
    }
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
