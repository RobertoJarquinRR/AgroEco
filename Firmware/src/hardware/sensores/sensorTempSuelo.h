#pragma once
#include "Isensor.h"
#include <OneWire.h>
#include <DallasTemperature.h>

class sensorTempSuelo : public Isensor {
    public:
    int pinSensor;
    int tempActual;
    OneWire miCable;
    DallasTemperature sensor;

    sensorTempSuelo(int pin);

    void Iniciar();
    int Read();
    void Calibrar();
    int GetStatus();
    String GetName();
};