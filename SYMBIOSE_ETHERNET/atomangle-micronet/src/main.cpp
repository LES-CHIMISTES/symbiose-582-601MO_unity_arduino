#include <Arduino.h>
#include <M5Unified.h>
#include <FastLED.h>
#include <MicroNetEthernet.h>
#include <MicroOscUdp.h>

// ===========================================================
// CONFIGURATION ETHERNET (MicroNet)
// ===========================================================
// Configuration prédéfinie pour Atom Lite sur base PoE
MicroNetEthernet microNet(MicroNetEthernet::Configuration::ATOM_POE_WITH_ATOM_LITE);

// Nom mDNS de l'ordinateur cible (sans .local)
// IMPORTANT : Changer ce nom pour celui de votre ordi récepteur
char nomCible[] = "CM587543";
#define PORT_OSC 7777

// ===========================================================
// ANGLE UNIT (lecture analogique directe via Grove Hub)
// ===========================================================
// Le Grove Hub split le signal du port Grove de l'Atom.
// Pin 32 = lecture analogique de l'Angle Unit
#define BROCHE_ANGLE 32

// ===========================================================
// RUBAN LED (FastLED via Grove Hub)
// ===========================================================
// Pin 26 = signal data du ruban LED
#define BROCHE_LEDS 26
#define NOMBRE_PIXELS 200
CRGB mesPixels[NOMBRE_PIXELS];

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

void setup()
{
  Serial.begin(115200);
  delay(500);
  Serial.println("=== Demarrage ===");

  // --- M5Unified ---
  auto cfg = M5.config();
  M5.begin(cfg);

  // --- FastLED ---
  FastLED.addLeds<WS2812B, BROCHE_LEDS, RGB>(mesPixels, NOMBRE_PIXELS);
  FastLED.setBrightness(50);
  fill_solid(mesPixels, NOMBRE_PIXELS, CRGB::Black);
  FastLED.show();

  // --- MicroNet : Ethernet + DHCP + mDNS ---
  // Génère un nom unique avec le MAC en suffixe (ex: atom-932AE4)
  char myName[MICRO_NET_NAME_MAX_LENGTH] = "atom-";
  microNet.appendMacToCString(myName, MICRO_NET_NAME_MAX_LENGTH, 3);
  Serial.print("MicroNet nom : ");
  Serial.println(myName);

  // Démarrer Ethernet (DHCP + mDNS)
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
  M5.update();
  // Maintenir la connexion réseau (mDNS, DHCP, etc.)
  microNet.update();

  // --- Lecture angle + envoi OSC + contrôle LEDs (toutes les 75 ms) ---
  if (millis() - monChronoOsc >= 75)
  {
    monChronoOsc = millis();

    // Lecture analogique directe de l'Angle Unit (via Grove Hub)
    int angle = analogRead(BROCHE_ANGLE);
    int angleMapped = map(angle, 0, 4095, 0, 255);

    // Envoi OSC de la valeur brute via Ethernet
    if (monOsc)
    {
      monOsc->sendInt("/angle", angle);
    }

    // Effet feu sur le ruban LED
    for (int i = 0; i < NOMBRE_PIXELS; i++)
    {
      int flicker = random(0, 80);
      int r = max(0, angleMapped - flicker);
      int g = max(0, (angleMapped * 40 / 255) - flicker / 2);
      mesPixels[i] = CRGB(r, g, 0);
    }
    FastLED.show();
  }

  // --- Debug série (toutes les 300 ms) ---
  if (millis() - monChronoDebug >= 300)
  {
    monChronoDebug = millis();
    int angle = analogRead(BROCHE_ANGLE);
    Serial.print("Angle : ");
    Serial.println(angle);
  }
}