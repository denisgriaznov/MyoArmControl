
#ifndef crc_h
#define crc_h

#include "Arduino.h"

#define START_OF_FRAME        (uint8_t) 0xA5

#define OFFSET_SOF            (uint8_t) 0
#define OFFSET_DATA_LENGTH    (uint8_t) 1
#define OFFSET_FRAME_SEQ      (uint8_t) 3
#define OFFSET_CRC8           (uint8_t) 4
#define OFFSET_DATA_TYPE      (uint8_t) 5

#define SIZE_FRAMEHEAD        (uint8_t) 5
#define SIZE_FRAMEID          (uint8_t) 2
#define SIZE_FRAMETAIL        (uint8_t) 2

#define FRAMEID_CMD_CAL       (uint16_t)  0x0001
#define FRAMEID_CMD_PWR       (uint16_t)  0x0002
#define FRAMEID_CMD_POS       (uint16_t)  0x0003
#define FRAMEID_CMD_VEL       (uint16_t)  0x0004

#define FRAMEID_FB_CAL        (uint16_t)  0x1001
#define FRAMEID_FB_PWR        (uint16_t)  0x1002
#define FRAMEID_FB_POS        (uint16_t)  0x1003
#define FRAMEID_FB_VEL        (uint16_t)

unsigned char Get_CRC8_Check_Sum(unsigned char *pchMessage, unsigned int dwLength, unsigned char ucCRC8);

unsigned int Verify_CRC8_Check_Sum(unsigned char *pchMessage, unsigned int dwLength);

void Append_CRC8_Check_Sum(unsigned char *pchMessage, unsigned int dwLength);

uint16_t Get_CRC16_Check_Sum(uint8_t *pchMessage, uint32_t dwLength, uint16_t wCRC);

uint32_t Verify_CRC16_Check_Sum(uint8_t  *pchMessage, uint32_t  dwLength);

void Append_CRC16_Check_Sum(uint8_t * pchMessage, uint32_t dwLength);

void uartSendBuffer(HardwareSerial s, const void* data, const uint16_t dataSize, const uint16_t dataType);

void* uartReceiveBuffer(uint8_t* recBuffer);

#endif
