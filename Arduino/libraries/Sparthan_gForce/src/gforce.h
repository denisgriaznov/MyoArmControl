
#ifndef gforce_h
#define gforce_h

#include "Arduino.h"
#include <BLEDevice.h>

// Types of data
#define QUATERNION_FLOAT                (byte)    0x5
#define GESTURE                         (byte)    0x7
#define STATUS_UPDATE                   (byte)    0xB

// Length of messages for integrity check
#define MSG_LEN_IMU                     (byte)    0x11
#define MSG_LEN_GESTURE                 (byte)    0x2
#define MSG_LEN_STATUS_UPDATE           (byte)    0x2

// Types of gesture data
#define GESTURE_RELAX                   (byte)    0x0
#define GESTURE_FIST                    (byte)    0x1
#define GESTURE_SPREAD_FINGERS          (byte)    0x2
#define GESTURE_WAVE_TOWARD_PALM        (byte)    0x3
#define GESTURE_WAVE_BACKWARD_PALM      (byte)    0x4
#define GESTURE_TUCK_FINGERS            (byte)    0x5
#define GESTURE_SHOOT                   (byte)    0x6
#define GESTURE_MAX                     GESTURE_SHOOT
#define GESTURE_UNKNOWN                 -1

// BLE Addresses and notification flags
#define GFORCE_ID_SERVICE               (const char*)  "0000ffd0-0000-1000-8000-00805f9b34fb"
#define GFORCE_DATA_SERVICE             (const char*)  "f000ffd0-0451-4000-b000-000000000000"
#define GFORCE_DATA_CHARACTERISTIC      (const char*)  "f000ffe2-0451-4000-b000-000000000000"
#define TURN_OFF                        (byte)    0x00
#define TURN_ON                         (byte)    0x01

// IMU structure
typedef struct Armband_IMU {
  float w;
  float x;
  float y;
  float z;
  bool updated = false;
} armband_imu;

// gesture structure
typedef struct Armband_Gesture {
  uint8_t type;
  bool updated = false;
} armband_gesture;

// status structure
typedef struct Armband_Status {
  uint8_t value;
  bool updated = false;
} armband_status;

// BLE armband class
class armband
{
  public:
    void connect();
    void get_info();
    void update(uint8_t* pData, size_t length);

    void set_mode(uint8_t, uint8_t, uint8_t);

    BLERemoteCharacteristic* data_notification(uint8_t);

    BLEClient* pClient;

    // Static status variables that can be used in callbacks
    static bool connected;
    static bool detected;
    static bool debug;

    // Data structures
    static armband_imu imu;
    static armband_gesture gesture;
    static armband_status status;

  private:
};

#endif
