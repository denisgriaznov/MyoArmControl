#include "sparthan.h"
#include "crc/crc.h"

// Communication on ESP UART2 GPIO(16, 17)
HardwareSerial sparthan::uart(2);
// Initialize position
float sparthan::position[5] = {0.0, 0.0, 0.0, 0.0, 0.0};


/*
** Descriptions: Initialize the LED and STM32 via GPIOs connection
** Input: N/A
** Output: N/A
*/
void sparthan::begin() {
  pinMode(BOOT, OUTPUT);                              // Initialize STM32 BOOT pin
  pinMode(RST, OUTPUT);                               // Initialize STM32 RST pin
  digitalWrite(BOOT, LOW);                            // Pull STM32 BOOT to ground
  digitalWrite(RST, LOW);                             // Pull STM32 RST to ground

  delay(100);
  pinMode(RST, INPUT);                                // Finish STM32 reboot
  delay(100);

  pinMode (LED, OUTPUT);                              // Notification LED
  digitalWrite (LED, HIGH);                           // Turn ON

  delay(100);
  sparthan::uart.begin(500000, SERIAL_8N1, 16, 17);   // STM32 UART communication
  delay(100);
}

/*
** Descriptions: Send position array over UART to STM32
** Input: five float values, one for each motor
** Output: N/A
*/
void sparthan::set_position(float f0, float f1, float f2, float f3, float f4) {
  float p[5] = {f0, f1, f2, f3, f4};
  memcpy(sparthan::position, &p, sizeof(p));
  uartSendBuffer(sparthan::uart, sparthan::position, sizeof(sparthan::position), FRAMEID_CMD_POS);
}
