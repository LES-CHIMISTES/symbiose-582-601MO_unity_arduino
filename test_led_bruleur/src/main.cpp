#include <Arduino.h>
#include <FastLED.h>

// --- Angle Unit ---
#define BROCHE_ANGLE 32

// --- Ruban LED ---
#define BROCHE_LEDS 26
#define NOMBRE_PIXELS 200  // Nombres de pixels

CRGB mesPixels[NOMBRE_PIXELS];

void setup() {
  Serial.begin(115200);
  delay(500);
  Serial.println("=== Test Angle + LEDs ===");

  // Initialiser le ruban LED
  FastLED.addLeds<WS2812B, BROCHE_LEDS, RGB>(mesPixels, NOMBRE_PIXELS);
  FastLED.setBrightness(50); // Brightness basse pour le test

  // Tout éteindre au départ
  fill_solid(mesPixels, NOMBRE_PIXELS, CRGB::Black);
  FastLED.show();
}

void loop() {
  // Lire l'angle
  int lectureAngle = analogRead(BROCHE_ANGLE);
  int angleMapped = map(lectureAngle, 0, 4095, 0, 255);

  // Afficher dans le serial monitor
  Serial.print("Angle brut: ");
  Serial.print(lectureAngle);
  Serial.print(" | Mappé (0-255): ");
  Serial.println(angleMapped);

  // Allumer les 200 pixels en rouge selon l'angle
  for (int i = 0; i < NOMBRE_PIXELS; i++) {
    // Flicker aléatoire par pixel (entre 0 et 80)
    int flicker = random(0, 80);
    int r = max(0, angleMapped - flicker);
    int g = max(0, (angleMapped * 40 / 255) - flicker / 2);
    mesPixels[i] = CRGB(r, g, 0);
  }
  FastLED.show();

  delay(30); // Un peu plus rapide pour l'effet feu
  }