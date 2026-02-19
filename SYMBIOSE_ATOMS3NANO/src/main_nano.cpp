#include <Arduino.h>
#include <MicroOscSlip.h>

MicroOscSlip<256> monOsc(&Serial);

unsigned long monChronoDepart; // DEPART DE MON CHRONOMÈTRE

void setup()
{
  Serial.begin(115200);
  monChronoDepart = millis(); // TEMPS DE DÉPART
}

void loop()
{
  if (millis() - monChronoDepart >= 75)
  {
    monChronoDepart = millis();

    // Lecture du joystick (comme un potentiomètre selon la doc)
    int joystickX = analogRead(A0);
    int joystickY = analogRead(A1);

    // Envoi en OSC
    monOsc.sendInt("/joystick/x", joystickX);
    monOsc.sendInt("/joystick/y", joystickY);
  }
}