#pragma once
#include "Isensor.h"
#include <DHT.h> //libreria de arduino pa leer los datos del sensor temp

class sensorTempAmbiente : public Isensor {
    public:
    int pinSensorTempAmbiente;
    int tempActual;
    DHT dht;

    //constructor
    sensorTempAmbiente(int pin);

    //funciones
    void Iniciar();
    void Calibrar(); //estara vacio el sensor de temperatura se calibra solo
    int Read();
    int GetStatus(); //  0 = desconectado, 1 = conectado,
    String GetName();

};
