#pragma once 
#include "Isensor.h"

class sensorHumedadSuelo : public Isensor {
    public:
    int pinSensorHumedadSuelo;
    int valorMin;
    int valorMax;
    int valorActual;

    //constructor
    sensorHumedadSuelo(int pin, int min, int max);
    /*
    aqui lo que hare sera un funcion llama a la otra una lee 
    la otra lo convierte a porcentaje
    */
   int Read();
   void Calibrar();
   int GetStatus(); //  0 = desconectado, 1 = conectado,

   int convertidorPorcentaje(int valor);
};