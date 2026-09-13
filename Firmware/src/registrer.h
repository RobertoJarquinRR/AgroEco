#pragma once
#include<queue>
#include<string>


class registrer{
    public:
        static std::queue<std::string> DataQueue;
        std::string hola;

    registrer();

    void SendContent(std::string content);

};