#include "actuadorRiego.h"

actuadorRiego::actuadorRiego(int pin)
{
    pinRele = pin;
}

void actuadorRiego::IniciarA()
{
    pinMode(pinRele,OUTPUT);
}

void actuadorRiego::Activar()
{
    digitalWrite(pinRele,HIGH); //LO ENCIENDE
    estaActivo = true;
}
void actuadorRiego::Desactivar()
{
    digitalWrite(pinRele, LOW); //LO APAGA
    estaActivo = false;
}

//ahora veo el estado
bool actuadorRiego::VerStatus()
{
    return estaActivo; 
}

//OBTENER EL NAME DEL ACTUADOR
string actuadorRiego::VerName()
{
    return "REGADOR XD"; //MAN NO SE QUE PONER SI RELE O RELE QUE ACTIVA LA MAGUERITA XDDD 
}
