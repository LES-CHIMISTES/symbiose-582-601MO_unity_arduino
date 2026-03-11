#include <Arduino.h>
#include <M5Unified.h>

// ===========================================================
// PROTOCOLE BINAIRE : struct envoyée via Serial au Atom Lite
// ===========================================================
struct ImuPacket {
  uint8_t header = 0xAA;
  float accelX, accelY, accelZ;
  float gyroX, gyroY, gyroZ;
  uint8_t footer = 0x55;
};

// ===========================================================
// SERIAL GROVE (vers Atom Lite sur PoE)
// ===========================================================
// AtomS3 Grove : G2 (yellow) = TX, G1 (white) = RX
// Avec un câble Grove droit :
//   AtomS3 G2 (TX) --yellow-- G26 (RX) Atom Lite
//   AtomS3 G1 (RX) --white--- G32 (TX) Atom Lite
#include <HardwareSerial.h>
HardwareSerial GroveSerial(1); // UART1
#define GROVE_RX 1
#define GROVE_TX 2

// ===========================================================
// CHRONOS
// ===========================================================
unsigned long monChronoDepart = 0;
unsigned long monChronoDebug = 0;

void setup()
{
  Serial.begin(115200);
  delay(3000);
  Serial.println("=== AtomS3 IMU - Demarrage ===");

  // --- M5Unified (pour l'IMU) ---
  auto cfg = M5.config();
  M5.begin(cfg);

  // --- Serial1 via Grove (vers Atom Lite) ---
  GroveSerial.begin(115200, SERIAL_8N1, GROVE_RX, GROVE_TX);

  Serial.println("=== Pret ===");
}

void loop()
{
  M5.update();

  // --- Lecture IMU + envoi via Grove (toutes les 75 ms) ---
  if (millis() - monChronoDepart >= 75)
  {
    monChronoDepart = millis();

    if (M5.Imu.update())
    {
      auto data = M5.Imu.getImuData();

      // Construire le paquet binaire
      ImuPacket packet;
      packet.accelX = data.accel.x;
      packet.accelY = data.accel.y;
      packet.accelZ = data.accel.z;
      packet.gyroX  = data.gyro.x;
      packet.gyroY  = data.gyro.y;
      packet.gyroZ  = data.gyro.z;

      // Envoyer via Grove Serial
      GroveSerial.write((uint8_t*)&packet, sizeof(packet));

      // DEBUG (toutes les 300 ms)
      if (millis() - monChronoDebug >= 300)
      {
        monChronoDebug = millis();
        Serial.print("Accel X: ");
        Serial.print(data.accel.x);
        Serial.print(" Y: ");
        Serial.print(data.accel.y);
        Serial.print(" Z: ");
        Serial.println(data.accel.z);
        Serial.print("Gyro  X: ");
        Serial.print(data.gyro.x);
        Serial.print(" Y: ");
        Serial.print(data.gyro.y);
        Serial.print(" Z: ");
        Serial.println(data.gyro.z);
      }
    }
  }
}