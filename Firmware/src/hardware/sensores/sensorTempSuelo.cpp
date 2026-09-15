#include "sensorTempSuelo.h"

sensorTempSuelo::sensorTempSuelo(int pin) : miCable(pin), sensor(&miCable)//le asigno los valores a mi cable para que sepa el pin y que sensro tenga la direccion de memoria de micable
{
    pinSensor = pin;
}
//iniciar el sensor
void sensorTempSuelo::Iniciar()
{
    sensor.begin();
}
//read ahora
int sensorTempSuelo::Read()
{
    sensor.requestTemperatures(); //pide una lectura nuevo asi funciona este sensor
    float temp = sensor.getTempCByIndex(0);
    tempActual = (int)temp;
    return tempActual;
}
void sensorTempSuelo::Calibrar() {}
//validacion de si funciona o no
int sensorTempSuelo::GetStatus()
{
    sensor.requestTemperatures(); //pide una lectura nuevo asi funciona este sensor
    float temp = sensor.getTempCByIndex(0);
    if(temp == DEVICE_DISCONNECTED_C) return 0;
    else return 1 ;
}

//asignar name
String sensorTempSuelo::GetName()
{
    return "TempSuelo";
}
