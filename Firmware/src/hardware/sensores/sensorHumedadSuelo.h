#pragma once 

class sensorHumedadSuelo{
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
   int leerHumedad();
   int convertidorPorcentaje(int valor);
};