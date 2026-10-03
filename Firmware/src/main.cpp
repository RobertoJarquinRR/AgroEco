#include <Arduino.h>
#include <chrono>
#include <queue>
#include "registrer.h"
#include "hardware/sensores/Isensor.h"
#include "hardware/sensores/sensorHumedadSuelo.h"
#include "hardware/sensores/sensorHumedadAmbiente.h"
#include "hardware/sensores/sensorTempAmbiente.h"
#include "hardware/sensores/sensorLuz.h"
#include "hardware/sensores/sensorTempSuelo.h"
#include "hardware/actuadores/actuadorRiego.h"

using namespace std;


//variablesxd
//variable de tiempos 
long ultimaLectura = 0 ;

//textofijo para mandar mensajes
const String jsonTypeRead = "reading";
const String jsonTypeFailed = "fault";
sensorHumedadAmbiente sensorHumedadAmb(4);
sensorHumedadSuelo sensorHSuelo(34,300, 4095);
sensorTempAmbiente sensorTempAmb(4);
sensorLuz senLuz(35,4095, 0);
sensorTempSuelo tempSuelo(32);

//actuadores
actuadorRiego riego(27);

Isensor* sensores[] = {&sensorHSuelo , &sensorHumedadAmb,&sensorTempAmb, &senLuz , &tempSuelo};

int cantidadSensores = 5;


void setup()
{
  Serial.begin(115200);

  //inicio solo estos el de humedad suelo no necesita eso
  sensorHumedadAmb.Iniciar();
  sensorTempAmb.Iniciar();
  tempSuelo.Iniciar();

  //inicio el actuador pero ojo la configuracion no es que va a arrancar 
  riego.IniciarA();//basicamente le hago saber que en el pin 27 va a mandar senal para activarse o no eso se vera en el c#


}

bool conected = false;

queue<string> Stack;

void loop()
{

  while (conected == false)
  {
    Serial.println("canYouconectect?");
    delay(400);
    if (Serial.available() > 0)
    {
      conected = true;
      char answer = Serial.read();

      if (answer == 'y')
      {
        conected = true;
      }
    }
  } 
 
  registrer mi;
  unsigned long ahora = millis() ; 
  //revisalo aver como lo ves roberto
  if(ahora  - ultimaLectura >= 3000)
  {
    
    for (int i = 0; i < cantidadSensores; i++)
    {
    
      //temporral prueba 
      Serial.println("ejecutando sensor:");
      Serial.println(i);
      if(sensores[i]->GetStatus() == 1) //XD que raro para acceder a propiedades eso  -> y no . xD
      {
        int valor = sensores[i]->Read();
        String sensorName = sensores[i]->GetName();

        String json = "{\"type\": \"" + jsonTypeRead + "\" , \"componentId\": \"" + sensorName + "\" , \"value\": " + valor + "}" ;
        
        //lo convierto porque use el String de arduino :'0
        mi.SendContent(string(json.c_str()));

        
      }
      else 
      {
        String sensorName = sensores[i]->GetName();
        String json = "{\"type\": \"" + jsonTypeFailed + "\" , \"componentId\": \"" + sensorName + "\" , \"error\": \"el sensor no se pudo conectar o esta defectuoso\"}";
        mi.SendContent(string(json.c_str()));
      }
    }
    ultimaLectura = ahora;
  }
  //nota elimine un codigo que era de prueba supongo era un hola xd

  while(!mi.DataQueue.empty()){
    string result = mi.DataQueue.front();
    
    Serial.println(result.c_str());
    mi.DataQueue.pop();
  }

  
}
