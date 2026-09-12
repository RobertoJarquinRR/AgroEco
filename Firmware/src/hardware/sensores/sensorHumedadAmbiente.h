#pragma once
#include "Isensor.h"
#include <DHT.h>  //libreria para leer los datos y conectar con el sensor de HUMEDAD Y TEMP

class sensorHumedadAmbiente : public Isensor {
    public:
    int pinSensorHumedadAmbiente;
    int humedadActual;
    DHT dht;

    //constructor
    sensorHumedadAmbiente(int pin);

    void Iniciar();
    int Read(); //lee la humedad del ambiente (aire)
    void Calibrar();
    int GetStatus(); //  0 = desconectado, 1 = conectado,

    

};