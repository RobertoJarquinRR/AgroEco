#include <Arduino.h>
#include "sensorHumedadSuelo.h"

sensorHumedadSuelo::sensorHumedadSuelo(int pin, int min, int max)
{
    pinSensorHumedadSuelo = pin;valorMin = min;valorMax = max;
}
int sensorHumedadSuelo::leerHumedad()
{
    valorActual = analogRead(pinSensorHumedadSuelo);
    return convertidorPorcentaje(valorActual);
}
int sensorHumedadSuelo::convertidorPorcentaje(int valor)
{
    int porcentaje = map(valor,valorMin,valorMax, 100 ,0);
    return porcentaje;
}