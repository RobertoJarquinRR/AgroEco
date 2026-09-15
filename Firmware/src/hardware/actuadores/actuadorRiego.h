#pragma once

#include <string>
#include <Arduino.h>

using namespace std;

class actuadorRiego{
    public:
    int pinRele;
    bool estaActivo;
    //constructor
    actuadorRiego(int pin);

    void IniciarA(); //lo inicia declara en que pin va a mandar senal
    void Activar(); //lo que va osea el digitalwrite 
    void Desactivar();    
    std::string VerName();
    bool VerStatus();
};