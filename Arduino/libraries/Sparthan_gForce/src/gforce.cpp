#include "gforce.h"

// The remote service we wish to connect to.
static BLEUUID serviceUUID(GFORCE_ID_SERVICE);
static BLEUUID data_UUID(GFORCE_DATA_SERVICE);
static BLERemoteCharacteristic* pRemoteCharacteristic;
static BLEAddress *pServerAddress;

// Create and initialize class variables
boolean armband::connected = false;
boolean armband::detected = false;
boolean armband::debug = false;
armband_gesture armband::gesture;
armband_imu armband::imu;
armband_status armband::status;

/**************************** BLUETOOTH CALLBACKS ****************************/
/*
** Descriptions: Find a BLE device based on the services being advertised
** Input: N/A
** Output: N/A
*/
class ArmbandAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {

    // Called for each advertising BLE server.
    void onResult(BLEAdvertisedDevice advertisedDevice) {

      armband::debug ? Serial.print("BLE Advertised Device found: ") : 0;
      armband::debug ? Serial.println(advertisedDevice.toString().c_str()) : 0;

      // We have found an armband, check if it contains the service ID we are looking for.
      if (advertisedDevice.haveServiceUUID() && advertisedDevice.getServiceUUID().equals(serviceUUID)) {

        armband::debug ? Serial.println(" - gForce device found") : 0;
        // Stop scanning
        advertisedDevice.getScan()->stop();
        // Save the address of the current Myo
        pServerAddress = new BLEAddress(advertisedDevice.getAddress());
        // Update detection status
        armband::detected = true;
      }
    }
};

/*
** Descriptions: Set the connect and disconnect behaviours
** Input: N/A
** Output: N/A
*/
class ArmbandClientCallbacks : public BLEClientCallbacks {

    void onConnect(BLEClient* pClient) {
      // Update connection status
      armband::connected = true;
    }
    void onDisconnect(BLEClient* pClient) {
      // Update connection status
      armband::connected = false;
      // Update detection status
      armband::detected = false;
      // Disconnect the clinet
      pClient->disconnect();
      // Clean the BLE stack for a new connection
      delete pClient;
    }
};
/************************** BLUETOOTH CALLBACKS END **************************/


/******************************** CONNECTION *********************************/
/*
** Descriptions: Helper function to connect to a generic BLE server
** Input: BLE clinet pointer, BLE address of the server
** Output: false flag if the server has not been reached
*/
bool connectToServer(BLEClient*  pClient, BLEAddress pAddress) {
  armband::debug ? Serial.print("Forming a connection to ") : 0;
  armband::debug ? Serial.println(pAddress.toString().c_str()) : 0;

  // Connect to the remove BLE Server.
  pClient->connect(pAddress);

  // Obtain a reference to the service we are after in the remote BLE server.
  BLERemoteService* pRemoteService = pClient->getService(data_UUID);
  if (pRemoteService == nullptr) {
    armband::debug ? Serial.print("Failed to find our service UUID: ") : 0;
    armband::debug ? Serial.println(data_UUID.toString().c_str()) : 0;
    return false;
  }
  armband::debug ? Serial.println(" - Found our service") : 0;
}

/*
** Descriptions: BLE connetion function to the armband
** Input:
** Output:
*/
void armband::connect() {
  if (!armband::connected) {

    // Initialize BLE scan
    BLEDevice::init("");
    BLEScan* pBLEScan = BLEDevice::getScan();
    // Set callbacks for finding devices
    pBLEScan->setAdvertisedDeviceCallbacks(new ArmbandAdvertisedDeviceCallbacks());
    // Active scan uses more power, but get results faster
    pBLEScan->setActiveScan(true);

    // Keep scanning untill a Myo is detected
    while (!armband::detected) {
      pBLEScan->start(10);
    }
    // Create new client and set up callbacks
    armband::pClient = BLEDevice::createClient();
    armband::pClient->setClientCallbacks(new ArmbandClientCallbacks());

    // Attempt Server connection
    connectToServer(armband::pClient, *pServerAddress);
  }
}
/***************************** CONNECTION END *********************************/


/****************************** NOTIFICATIONS *********************************/
/*
** Descriptions: Toggle stream of data from the armband
** Input: ON/OFF value
** Output: BLE Characteristic pointer, used to assign a callback function if needed
*/
BLERemoteCharacteristic* armband::data_notification(uint8_t on_off) {
  if (armband::connected) {
    BLEUUID tservice = BLEUUID(GFORCE_DATA_SERVICE);
    BLEUUID tcharacteristic = BLEUUID(GFORCE_DATA_CHARACTERISTIC);
    uint8_t IndicateOn[] = {on_off, 0x00};
    armband::pClient->getService(tservice)->getCharacteristic(tcharacteristic)->getDescriptor(((uint16_t)0x2902))->writeValue((uint8_t*)IndicateOn, sizeof(IndicateOn), true);
    return armband::pClient->getService(BLEUUID(tservice))->getCharacteristic(BLEUUID(tcharacteristic));
  }
}
/**************************** NOTIFICATIONS  END ******************************/


/********************************* COMMANDS ***********************************/
/*
** Descriptions: TODO: implement set_mode command
** Input: 
** Output: 
*/
void armband::set_mode(uint8_t emg_mode, uint8_t imu_mode, uint8_t clf_mode) {
  if (armband::connected) {
    //    BLEUUID tservice = BLEUUID("d5060001-a904-deb9-4748-2c7f4a124842");
    //    BLEUUID tcharacteristic = BLEUUID("d5060401-a904-deb9-4748-2c7f4a124842");
    //    uint8_t writeVal[] = {0x01, 0x03, emg_mode, imu_mode, clf_mode};
    //    armband::pClient->getService(tservice)->getCharacteristic(tcharacteristic)->writeValue(writeVal, sizeof(writeVal));
  }
}

/*
** Descriptions: TODO: implement get_info command
** Input:
** Output:
*/
void armband::get_info() {
  if (armband::connected) {
    //    BLEUUID tservice = BLEUUID("d5060001-a904-deb9-4748-2c7f4a124842");
    //    BLEUUID tcharacteristic = BLEUUID("d5060101-a904-deb9-4748-2c7f4a124842");
    //    std::string stringt;
    //    stringt = armband::pClient->getService(tservice)->getCharacteristic(tcharacteristic)->readValue();
  }
}

/*
** Descriptions: Helper function to string 4 bytes into 32 bit float
** Input: four 8-bit unsigned integers
** Output: 32-bit float
*/
float byte2float (uint8_t b0, uint8_t b1, uint8_t b2, uint8_t b3) {
  float f;
  uint8_t b[] = {b0, b1, b2, b3};
  memcpy(&f, &b, sizeof(f));
  return f;
}

/*
** Descriptions: Update armband data structure from Bluetooth data
** Input: data received from Bluetooth notify callback
** Output: updated data structures in the armband class
*/
void armband::update(uint8_t* pData, size_t length) {
  // detect data type
  int type = pData[0];
  switch (type) {
    case GESTURE:
      // ensure message integrity
      if (length == MSG_LEN_GESTURE) {
        armband::gesture.type = pData[1];
        armband::gesture.updated = true;
      }
      break;
    case QUATERNION_FLOAT:
      // ensure message integrity
      if (length == MSG_LEN_IMU) {
        armband::imu.w = byte2float(pData[1], pData[2], pData[3], pData[4]);
        armband::imu.x = byte2float(pData[5], pData[6], pData[7], pData[8]);
        armband::imu.y = byte2float(pData[9], pData[10], pData[11], pData[12]);
        armband::imu.z = byte2float(pData[13], pData[14], pData[15], pData[16]);
        armband::imu.updated = true;
      }
      break;
    case STATUS_UPDATE:
      // ensure message integrity
      if (length == MSG_LEN_STATUS_UPDATE) {
        armband::status.value = pData[1];
        armband::status.updated = true;
      }
      break;
    default:
      break;
  }
}
/******************************* COMMANDS END *********************************/
