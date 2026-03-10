#include <Arduino.h>
#include <Wire.h>
#include <MicroRemoteWirePeripheral.h>

constexpr uint8_t PERIPHERAL_I2C_ADDR = 0x42; // important se souvenir


MicroRemoteWirePeripheral peripheral;

// ==========================
// I2C
// ==========================
void onReceive(int numBytes)
{
  peripheral.onReceive(Wire, numBytes);
}

void onRequest()
{
  peripheral.onRequest(Wire);
}

// ==========================
// SETUP
// ==========================
void setup()
{
  Wire.begin(PERIPHERAL_I2C_ADDR);
  Wire.onReceive(onReceive);
  Wire.onRequest(onRequest);

}

void loop()
{


}