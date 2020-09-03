
#ifndef sparthan_h
#define sparthan_h

#include "Arduino.h"

#define LED           2U
#define BOOT          18U
#define RST           19U

#define MAXPOS                (float)     0.01
#define MINPOS                (float)     -0.07
#define STEP                  (float)     0.0002

// Sparthan controller class
class sparthan
{
  public:
    void begin();
    void set_position(float f0, float f1, float f2, float f3, float f4);

    // Static status variables that can be used in callbacks
    static HardwareSerial uart;
    static float position [5];

  private:
};

#endif
