#include "sensorHumedadAmbiente.h"

sensorHumedadAmbiente::sensorHumedadAmbiente(int pin) : dht(pin, DHT22) {
    pinSensorHumedadAmbiente = pin;   
}

int sensorHumedadAmbiente::Read()
{
    float humedad = dht.readHumidity();
    humedadActual = humedad;
    return humedadActual;
}

//calibrador no se usa porque el sensor  de humedad se calibra solo 
void sensorHumedadAmbiente::Calibrar(){/*no se usa xD*/}

//ver estado de sensor
int sensorHumedadAmbiente::GetStatus()
{
    float humedad = dht.readHumidity();
    if(isnan(humedad)) //significa que no esta leyendo los datos
    {
        return 0; //danado o no conectado
    }
    else return 1; //conectadoooo y funcional
}


//inicia el sensor de humedad ambiente
void sensorHumedadAmbiente::Iniciar() {
    dht.begin();
}