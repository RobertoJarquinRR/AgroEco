#include "registrer.h"
#include <string>

using namespace std;
queue<string> registrer::DataQueue;


registrer::registrer(){

};
void registrer::SendContent(string content){
    DataQueue.push("@{"+content+"}*");  
};