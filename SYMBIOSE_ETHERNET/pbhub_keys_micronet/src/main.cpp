#include <Arduino.h>
#include <M5_PbHub.h>
#include <FastLED.h>
#include <MicroNetEthernet.h>
#include <MicroOscUdp.h>

// ===========================================================
// CONFIGURATION ETHERNET (MicroNet)
// ===========================================================
MicroNetEthernet microNet(MicroNetEthernet::Configuration::ATOM_POE_WITH_ATOM_LITE);

// Nom mDNS de l'ordinateur cible (sans .local)
// IMPORTANT : Changer ce nom pour celui de votre ordi récepteur
char nomCible[] = "CM587543";
#define PORT_OSC 7777

// ===========================================================
// PBHUB + KEY UNITS
// ===========================================================
M5_PbHub myPbHub;

// 3 keys qui envoient de l'OSC
#define CANAL_KEY1 0
#define CANAL_KEY2 4
#define CANAL_KEY3 5

// 3 keys pour les LEDs (logique à venir)
#define CANAL_KEY4 1
#define CANAL_KEY5 2
#define CANAL_KEY6 3

// ===========================================================
// OSC VIA UDP ETHERNET
// ===========================================================
EthernetUDP monUdp;
IPAddress ipCible;
MicroOscUdp<1024>* monOsc = nullptr;

// ===========================================================
// CHRONOS
// ===========================================================
unsigned long monChronoOsc = 0;
unsigned long monChronoDebug = 0;

// LED fade
float ledR = 0, ledG = 0, ledB = 0;
float fadeSpeed = 0.65; // Plus petit = plus lent (0.0 à 1.0)

void setup()
{
  Serial.begin(115200);
  delay(500);
  Serial.println("=== Demarrage ===");

  Wire.begin();
  myPbHub.begin();

  // Initialiser le pixel de chaque key unit (1 pixel par canal)
  myPbHub.setPixelCount(CANAL_KEY1, 1);
  myPbHub.setPixelCount(CANAL_KEY2, 1);
  myPbHub.setPixelCount(CANAL_KEY3, 1);
  myPbHub.setPixelCount(CANAL_KEY4, 1);
  myPbHub.setPixelCount(CANAL_KEY5, 1);
  myPbHub.setPixelCount(CANAL_KEY6, 1);

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

  // --- Lecture keys + envoi OSC (toutes les 75 ms) ---
  if (millis() - monChronoOsc >= 75)
  {
    monChronoOsc = millis();

    // Lecture des 3 keys OSC via PbHub
    int key1 = 1 - myPbHub.digitalRead(CANAL_KEY1);
    int key2 = 1 - myPbHub.digitalRead(CANAL_KEY2);
    int key3 = 1 - myPbHub.digitalRead(CANAL_KEY3);

    // Envoi OSC via Ethernet
    if (monOsc)
    {
      monOsc->sendInt("/key1", key1);
      monOsc->sendInt("/key2", key2);
      monOsc->sendInt("/key3", key3);
    }

// Couleur cible selon le key appuyé
    float cibleR = 0, cibleG = 0, cibleB = 0;

    if (key1) {
      cibleR = 255; cibleG = 255; cibleB = 255; // Blanc
    } else if (key2) {
      cibleR = 0; cibleG = 0; cibleB = 255;     // Bleu
    } else if (key3) {
      cibleR = 0; cibleG = 255; cibleB = 0;     // Vert
    }

    // Lerp vers la couleur cible
    ledR += (cibleR - ledR) * fadeSpeed;
    ledG += (cibleG - ledG) * fadeSpeed;
    ledB += (cibleB - ledB) * fadeSpeed;

    int r = (int)ledR;
    int g = (int)ledG;
    int b = (int)ledB;

    myPbHub.setPixelColor(CANAL_KEY1, 0, r, g, b);
    myPbHub.setPixelColor(CANAL_KEY2, 0, r, g, b);
    myPbHub.setPixelColor(CANAL_KEY3, 0, r, g, b);
    myPbHub.setPixelColor(CANAL_KEY4, 0, r, g, b);
    myPbHub.setPixelColor(CANAL_KEY5, 0, r, g, b);
    myPbHub.setPixelColor(CANAL_KEY6, 0, r, g, b);
    // --- Debug série (toutes les 300 ms) ---
    if (millis() - monChronoDebug >= 300)
    {
      monChronoDebug = millis();
      Serial.print("Key1: ");
      Serial.print(key1);
      Serial.print(" | Key2: ");
      Serial.print(key2);
      Serial.print(" | Key3: ");
      Serial.println(key3);
    }
  }
}