#include "registrer.h"
#include <string>
std::queue<std::string> registrer::DataQueue;


registrer::registrer(){

};
void registrer::SendContent(std::string content){
    DataQueue.push("@{"+content+"}*");  
};