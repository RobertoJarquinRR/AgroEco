#include "sensorLuz.h"

//constructor para que este solo para recibir los parametros
sensorLuz::sensorLuz(int pin , int vMax,int vMin)
{
    pinSensorLuz = pin ; valorMax = vMax; valorMin = vMin;
}
int sensorLuz::Read()
{
    valorDeLuz = analogRead(pinSensorLuz);
    return Convertidor(valorDeLuz);
}
//no lo uso porque nah xd
void sensorLuz::Calibrar(){  }

//conversor a %
int sensorLuz::Convertidor(int valor)
{
    int porcentaje = map(valor,valorMin,valorMax,0,100);
    return porcentaje;
}
int sensorLuz::GetStatus()
{
    int valorCrudo = analogRead(pinSensorLuz);

    if (valorCrudo == 0 || valorCrudo== 4095)  return 0;
    else return 1;
}
String sensorLuz::GetName()
{
    return "Luz";
}