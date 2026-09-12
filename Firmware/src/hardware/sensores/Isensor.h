#pragma once

class Isensor{
    public:
    virtual void Iniciar() = 0;
    virtual int Read() = 0;
    virtual void Calibrar() = 0;
    virtual int GetStatus() = 0; //  0 = desconectado, 1 = conectado,                             
};