#include <Arduino.h>
#include <M5Unified.h>
#include <M5_PbHub.h>
#include <MicroOscSlip.h>

M5_PbHub myPbHub;
MicroOscSlip<1024> monOsc(&Serial);

#define CANAL_ANGLE 0
#define CANAL_FADER_X 1
#define CANAL_FADER_Y 2
#define CANAL_KEY1 3
#define CANAL_KEY2 4
#define CANAL_KEY3 5

unsigned long monChronoDepart; // DEPART DE MON CHRONOMÈTRE
// unsigned long monChronoDebug;  // CHRONO POUR DEBUG

void setup()
{
  monChronoDepart = millis(); // TEMPS DE DÉPART
  // monChronoDebug = millis();

  // Configuration M5Unified pour AtomS3
  auto cfg = M5.config();       // Assign a structure for initializing M5Stack
  cfg.serial_baudrate = 115200; // Enable Serial with 115200 baud
  M5.begin(cfg);                // Initialize M5Stack with the specified configuration

  Wire.begin();
  myPbHub.begin();
}

void loop()
{
  M5.update();

  if (millis() - monChronoDepart >= 75)
  {
    monChronoDepart = millis();

    // Lecture des units PbHub
    int angle = myPbHub.analogRead(CANAL_ANGLE);
    int faderX = myPbHub.analogRead(CANAL_FADER_X);
    int faderY = myPbHub.analogRead(CANAL_FADER_Y);
    int key1 = 1 - myPbHub.digitalRead(CANAL_KEY1);
    int key2 = 1 - myPbHub.digitalRead(CANAL_KEY2);
    int key3 = 1 - myPbHub.digitalRead(CANAL_KEY3);

    monOsc.sendInt("/angle", angle);
    monOsc.sendInt("/faderX", faderX);
    monOsc.sendInt("/faderY", faderY);
    monOsc.sendInt("/key1", key1);
    monOsc.sendInt("/key2", key2);
    monOsc.sendInt("/key3", key3);

    // Lecture et envoi des données IMU
    if (M5.Imu.update())
    {
      auto data = M5.Imu.getImuData();

      // Envoyer via OSC (pour Pure Data)
      monOsc.sendFloat("/accel/x", data.accel.x);
      monOsc.sendFloat("/accel/y", data.accel.y);
      monOsc.sendFloat("/accel/z", data.accel.z);

      monOsc.sendFloat("/gyro/x", data.gyro.x);
      monOsc.sendFloat("/gyro/y", data.gyro.y);
      monOsc.sendFloat("/gyro/z", data.gyro.z);

      // DEBUG
      /*if (millis() - monChronoDebug >= 300)
      {
        monChronoDebug = millis();

        Serial.print("Angle: ");
        Serial.println(angle);
        Serial.print("FaderX: ");
        Serial.println(faderX);
        Serial.print("FaderY: ");
        Serial.println(faderY);
        Serial.print("Key1: ");
        Serial.println(key1);
        Serial.print("Key2: ");
        Serial.println(key2);
        Serial.print("Key3: ");
        Serial.println(key3);
        Serial.print("Accel X: ");
        Serial.println(data.accel.x);
        Serial.print("Accel Y: ");
        Serial.println(data.accel.y);
        Serial.print("Accel Z: ");
        Serial.println(data.accel.z);
        Serial.print("Gyro X: ");
        Serial.println(data.gyro.x);
        Serial.print("Gyro Y: ");
        Serial.println(data.gyro.y);
        Serial.print("Gyro Z: ");
        Serial.println(data.gyro.z);
      }*/
    }
  }
}