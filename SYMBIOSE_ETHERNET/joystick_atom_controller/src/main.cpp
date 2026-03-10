// FILE main.cpp
#include <Arduino.h>
#include <Wire.h>

#include <FastLED.h>
CRGB atomPixel;
#define BROCHE_ATOM_PIXEL 27

#include <MicroNetEthernet.h>
const char *nameToResolve = "CM587543"; // Do not append ".local"
MicroNetEthernet myMicroNet(MicroNetEthernet::Configuration::ATOM_POE_WITH_ATOM_LITE);

#include <MicroRemoteWireController.h>
MicroRemoteWireController remote[] = {
    {Wire, 0x42}
};
size_t remoteCount = sizeof(remote) / sizeof(remote[0]);

#include <MicroOscUdp.h>
EthernetUDP udp;
#define MY_PORT 8888
#define CIBLE_PORT 7777
MicroOscUdp<1024> osc(udp);

unsigned long lastMillisSent = 0;

void setup()
{
  Serial.begin(115200);

  delay(100);   // Wait for peripherals to power up
  Wire.begin(); // Begin as controller
  delay(100);   // Short delay to ensure Wire is ready

  // Initialiser FastLED pour contrôler le pixel RGB du M5Atom
  FastLED.addLeds<WS2812, BROCHE_ATOM_PIXEL, GRB>(&atomPixel, 1);
  // animation de démarrage de 3 secondes -------|
  atomPixel = CRGB(255, 0, 255);
  FastLED.show();
  delay(1000); // PAUSE 1 SECONDE
  atomPixel = CRGB(255, 0, 255);
  FastLED.show();
  delay(1000); // PAUSE 1 SECONDE
  atomPixel = CRGB(0, 0, 255);
  FastLED.show();
  delay(1000);

  // Rouge pour indiquer que le programme démarre
  atomPixel = CRGB(255, 0, 0);
  FastLED.show();

  // Créer le nom de l'appareil pour mDNS
  char myName[MICRO_NET_NAME_MAX_LENGTH] = "atom-"; // name prefix
  myMicroNet.appendMacToCString(myName, MICRO_NET_NAME_MAX_LENGTH, 3);

  // Configure Ethernet et démarre le réseau et mDNS
  myMicroNet.begin(myName);

  Serial.print("My IP is: ");
  Serial.println(myMicroNet.getIP());
  Serial.print("My name is: ");
  Serial.println(myName);

  // Jaune pour indiquer que le programme est connecté au réseau
  atomPixel = CRGB(255, 255, 0);
  FastLED.show();

  IPAddress ipCible = myMicroNet.resolveName(nameToResolve);
  Serial.print("Resolved IP for ");
  Serial.print(nameToResolve);
  Serial.print(" is: ");
  Serial.println(ipCible);

  // Vert pour indiquer que l'adresse de la cible a été résolue
  atomPixel = CRGB(0, 255, 0);
  FastLED.show();

  udp.begin(MY_PORT);
  osc.setDestination(ipCible, CIBLE_PORT);

  Serial.print("Configuring ");
  Serial.print(remoteCount);
  Serial.println(" devices");

  for (size_t i = 0; i < remoteCount; i++)
  {
    remote[i].setPinOutput(13);
    remote[i].digitalWriteHigh(13);

    remote[i].setPinOutput(3);
    remote[i].setPinOutput(5);
    remote[i].setPinOutput(6);
    remote[i].setPinOutput(10);

    // Setup inputs
    remote[i].setPinInputPullup(2);
    remote[i].setPinInputPullup(4);
    remote[i].setPinInputPullup(8);
    remote[i].setPinInputPullup(9);
  }

    Serial.println("Starting loop");
}

void processOscMessage(MicroOscMessage &msg)
{
  if (msg.checkOscAddress("/pm"))
  {
    uint8_t i = msg.nextAsInt();
    if (i >= remoteCount)
      return;
    uint8_t pin = msg.nextAsInt();
    uint8_t value = msg.nextAsInt();

    if (value == 0)
    {
      remote[i].setPinInputPullup(pin);
    }
    else if (value == 1)
    {
      remote[i].setPinOutput(pin);
    }
  }
  else if (msg.checkOscAddress("/dw"))
  {
    uint8_t i = msg.nextAsInt();
    if (i >= remoteCount)
      return;
    uint8_t pin = msg.nextAsInt();
    uint8_t value = msg.nextAsInt();
    if (value == 0)
    {
      remote[i].digitalWriteLow(pin);
    }
    else
    {
      remote[i].digitalWriteHigh(pin);
    }
  }
  else if (msg.checkOscAddress("/aw"))
  {
    uint8_t i = msg.nextAsInt();
    if (i >= remoteCount)
      return;
    uint8_t pin = msg.nextAsInt();
    uint8_t value = msg.nextAsInt();
    remote[i].analogWrite(pin, value);
  }
}

void loop()
{
  myMicroNet.update();

  osc.onOscMessageReceived(processOscMessage);

  if (millis() - lastMillisSent > 10)
  {
    lastMillisSent = millis();

    for (size_t i = 0; i < remoteCount; i++)
    {
      uint16_t analogRead0 = remote[i].analogRead(0);
      uint16_t analogRead1 = remote[i].analogRead(1);
      uint16_t analogRead2 = remote[i].analogRead(2);
      uint16_t analogRead3 = remote[i].analogRead(3);

      osc.messageBegin("/a", "iiiii");
      osc.messageAddInt(i);
      osc.messageAddInt(analogRead0);
      osc.messageAddInt(analogRead1);
      osc.messageAddInt(analogRead2);
      osc.messageAddInt(analogRead3);
      osc.messageEnd();

      bool digitalRead2 = remote[i].digitalRead(2);
      bool digitalRead4 = remote[i].digitalRead(4);
      bool digitalRead8 = remote[i].digitalRead(8);
      bool digitalRead9 = remote[i].digitalRead(9);

      osc.messageBegin("/d", "iiiii");
      osc.messageAddInt(i);
      osc.messageAddInt(digitalRead2);
      osc.messageAddInt(digitalRead4);
      osc.messageAddInt(digitalRead8);
      osc.messageAddInt(digitalRead9);
      osc.messageEnd();
    }
  }
}