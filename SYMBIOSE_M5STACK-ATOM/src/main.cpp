#include <Arduino.h>
#include <FastLED.h>
#include <M5_PbHub.h>
#include <MicroOscSlip.h>

M5_PbHub myPbHub;
CRGB atomPixel;
MicroOscSlip<128> monOsc(&Serial);

#define CANAL_ANGLE 0
#define CANAL_FADER_X 1
#define CANAL_FADER_Y 2
#define CANAL_KEY1 3
#define CANAL_KEY2 4
#define CANAL_KEY3 5

unsigned long monChronoDepart ; // DEPART DE MON CHRONOMÈTRE

void setup() {
  monChronoDepart = millis(); // TEMPS DE DÉPART
  Serial.begin(115200);
  Wire.begin();
  myPbHub.begin();



  FastLED.addLeds< WS2812, 27 , GRB >(&atomPixel, 1); // setup le pixel du btn atom
  atomPixel = CRGB(255,0,0); // ROUGE
  FastLED.show();
  delay(1000); // PAUSE 1 SECONDE
  atomPixel = CRGB(255,255,0); // JAUNE
  FastLED.show();
  delay(1000); // PAUSE 1 SECONDE
  atomPixel = CRGB(0,255,0); // VERT
  FastLED.show();
  delay(1000); // PAUSE 1 SECONDE
  atomPixel = CRGB(0,0,0);
  FastLED.show(); // PAUSE 1 SECOND
}

void loop() {

  if ( millis() - monChronoDepart >= 75 ) { 
    monChronoDepart = millis();   

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
  }
}