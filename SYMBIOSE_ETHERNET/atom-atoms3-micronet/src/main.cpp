#include <Arduino.h>
#include <MicroNetEthernet.h>
#include <MicroOscUdp.h>

// ===========================================================
// PROTOCOLE BINAIRE : même struct que sur l'AtomS3
// ===========================================================
struct ImuPacket {
  uint8_t header;
  float accelX, accelY, accelZ;
  float gyroX, gyroY, gyroZ;
  uint8_t footer;
};

// ===========================================================
// CONFIGURATION ETHERNET (MicroNet)
// ===========================================================
MicroNetEthernet microNet(MicroNetEthernet::Configuration::ATOM_POE_WITH_ATOM_LITE);

char nomCible[] = "CM587543";
#define PORT_OSC 7777

// ===========================================================
// SERIAL GROVE (depuis AtomS3)
// ===========================================================
// Atom Lite Grove : G26 (yellow) = RX, G32 (white) = TX
// Avec un câble Grove droit :
//   Atom Lite G26 (RX) --yellow-- G2 (TX) AtomS3
//   Atom Lite G32 (TX) --white--- G1 (RX) AtomS3
#include <HardwareSerial.h>
HardwareSerial GroveSerial(1); // UART1
#define GROVE_RX 26
#define GROVE_TX 32

// ===========================================================
// OSC VIA UDP ETHERNET
// ===========================================================
EthernetUDP monUdp;
IPAddress ipCible;
MicroOscUdp<1024>* monOsc = nullptr;

// ===========================================================
// BUFFER DE RÉCEPTION
// ===========================================================
uint8_t rxBuffer[sizeof(ImuPacket)];
int rxIndex = 0;

// ===========================================================
// CHRONO DEBUG
// ===========================================================
unsigned long monChronoDebug = 0;

void setup()
{
  Serial.begin(115200);
  delay(1000);
  Serial.println("=== Atom Lite PoE - Demarrage ===");

  // --- Serial1 via Grove (depuis AtomS3) ---
  GroveSerial.begin(115200, SERIAL_8N1, GROVE_RX, GROVE_TX);

  // --- MicroNet : Ethernet + DHCP + mDNS ---
  char myName[MICRO_NET_NAME_MAX_LENGTH] = "atom-";
  microNet.appendMacToCString(myName, MICRO_NET_NAME_MAX_LENGTH, 3);
  Serial.print("MicroNet nom : ");
  Serial.println(myName);

  microNet.begin(myName);
  Serial.print("IP obtenue : ");
  Serial.println(microNet.getIP().toString());

  // --- Résolution mDNS de la cible ---
  Serial.print("Resolution mDNS : ");
  Serial.println(nomCible);
  ipCible = microNet.resolveName(nomCible);
  Serial.print("IP cible : ");
  Serial.println(ipCible.toString());

  // --- Initialiser OSC UDP via Ethernet ---
  monUdp.begin(PORT_OSC);
  monOsc = new MicroOscUdp<1024>(&monUdp, ipCible, PORT_OSC);

  Serial.println("=== Pret ===");
}

void loop()
{
  microNet.update();

  // --- Réception des paquets binaires du AtomS3 ---
  while (GroveSerial.available())
  {
    uint8_t b = GroveSerial.read();

    // Chercher le header 0xAA
    if (rxIndex == 0 && b != 0xAA)
    {
      continue; // Ignorer les octets jusqu'au header
    }

    rxBuffer[rxIndex] = b;
    rxIndex++;

    // Paquet complet reçu
    if (rxIndex >= (int)sizeof(ImuPacket))
    {
      ImuPacket* packet = (ImuPacket*)rxBuffer;

      // Vérifier le footer
      if (packet->footer == 0x55)
      {
        // Envoi OSC via Ethernet
        if (monOsc)
        {
          monOsc->sendFloat("/accel/x", packet->accelX);
          monOsc->sendFloat("/accel/y", packet->accelY);
          monOsc->sendFloat("/accel/z", packet->accelZ);
          monOsc->sendFloat("/gyro/x",  packet->gyroX);
          monOsc->sendFloat("/gyro/y",  packet->gyroY);
          monOsc->sendFloat("/gyro/z",  packet->gyroZ);
        }

        // DEBUG (toutes les 300 ms)
        if (millis() - monChronoDebug >= 300)
        {
          monChronoDebug = millis();
          Serial.print("Accel X: ");
          Serial.print(packet->accelX);
          Serial.print(" Y: ");
          Serial.print(packet->accelY);
          Serial.print(" Z: ");
          Serial.println(packet->accelZ);
          Serial.print("Gyro  X: ");
          Serial.print(packet->gyroX);
          Serial.print(" Y: ");
          Serial.print(packet->gyroY);
          Serial.print(" Z: ");
          Serial.println(packet->gyroZ);
        }
      }

      rxIndex = 0; // Reset pour le prochain paquet
    }
  }
}