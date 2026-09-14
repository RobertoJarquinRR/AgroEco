#include "sensorTempAmbiente.h"

//constructor 
sensorTempAmbiente::sensorTempAmbiente(int pin): dht(pin,DHT22)
{
    pinSensorTempAmbiente = pin;
}
//inicio el sensorrr
void sensorTempAmbiente::Iniciar()
{
    dht.begin();
}
//leectura del sensor 
int sensorTempAmbiente::Read()
{
    float valor = dht.readTemperature();
    return tempActual = valor;//retorna el valor que el sensor calibra
}
//calibrar otra ves no sirve porque el propio sensor lo hace solo 
void sensorTempAmbiente::Calibrar() {}

//verificador del estado del sensor
int sensorTempAmbiente::GetStatus()
{
    float lectura = dht.readTemperature();
    //validador
    if(isnan(lectura)) return 0;
    else return 1;
}