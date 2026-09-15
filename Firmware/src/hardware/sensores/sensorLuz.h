#pragma once
#include "Isensor.h"

class sensorLuz : public Isensor {
    public:
    int pinSensorLuz;
    int valorDeLuz;
    int valorMax;
    int valorMin;
    //constructor
    sensorLuz(int pin, int vMax, int vMin);

    //funciones
    void Iniciar();
    void Calibrar();
    int GetStatus();
    int Read();
    String GetName();

    //funcion extra para sacar %
    int Convertidor(int valor);
};