#include <Arduino.h>
#include <M5Unified.h>
#include <MicroOscSlip.h>

MicroOscSlip<1024> monOsc(&Serial);

unsigned long monChronoDepart; // DEPART DE MON CHRONOMÈTRE
unsigned long monChronoDebug;  // CHRONO POUR DEBUG

void setup()
{
  monChronoDepart = millis(); // TEMPS DE DÉPART
  monChronoDebug = millis();

  // Configuration M5Unified pour AtomS3
  auto cfg = M5.config();       // Assign a structure for initializing M5Stack
  cfg.serial_baudrate = 115200; // Enable Serial with 115200 baud
  M5.begin(cfg);                // Initialize M5Stack with the specified configuration
}

void loop()
{
  M5.update();

  if (millis() - monChronoDepart >= 75)
  {
    monChronoDepart = millis();

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
      if (millis() - monChronoDebug >= 300)
      {
        monChronoDebug = millis();

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
      }
    }
  }
}