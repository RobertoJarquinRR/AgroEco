#pragma once

class Isensor{
    public:
    virtual int Read() = 0;
    void Calibrar();
    virtual int GetStatus() = 0; //  0 = desconectado, 1 = conectado,                             
};