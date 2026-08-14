#include <Arduino.h>
#include <DHT.h>
#include <chrono>
#include <queue>
#include "registrer.h"

DHT sensor(4, DHT22);

void setup()
{
  Serial.begin(115200);
  sensor.begin();
}

bool conected = false;

std::queue<std::string> Stack;

void loop()
{

  while (conected == true)
  {
    Serial.println("canYouconectect?");
    delay(400);
    if (Serial.available() > 0)
    {
      conected = true;
      char answer = Serial.read();

      if (answer == 'y')
      {
        conected = true;
      }
    }
  }
  delay(800);
  float temperatura = sensor.readTemperature();
  float humedad = sensor.readHumidity();

  registrer mi;
  Serial.printf(mi.hola.c_str());
  mi.SendContent("hola");

  while(!mi.DataQueue.empty()){
    std::string result = mi.DataQueue.front();

    Serial.println(result.c_str());
  }
}
