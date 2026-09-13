#include <Arduino.h>
#include "sensorHumedadSuelo.h"

sensorHumedadSuelo::sensorHumedadSuelo(int pin, int min, int max)
{
    pinSensorHumedadSuelo = pin;valorMin = min;valorMax = max;
}

//leer
int sensorHumedadSuelo::Read()
{
    valorActual = analogRead(pinSensorHumedadSuelo);
    return convertidorPorcentaje(valorActual);
}

//convertir a %
int sensorHumedadSuelo::convertidorPorcentaje(int valor)
{
    int porcentaje = map(valor,valorMin,valorMax, 100 ,0);
    return porcentaje;
}
//calibrador uso no implementado aun
void sensorHumedadSuelo::Calibrar()
{
    valorMin = analogRead(pinSensorHumedadSuelo);
}

// ver estado de sensor
int sensorHumedadSuelo::GetStatus()
{
    int valorCrudo = analogRead(pinSensorHumedadSuelo);
    if(valorCrudo == 0 || valorCrudo == 4095) //aqui es eso porque usamos un ADC de 12 bits y si esta en 0 o 4095 es que no esta leyendo correctamente
    return 0 ; //esta o danado o no leyendo corretamente
    else return 1;
}