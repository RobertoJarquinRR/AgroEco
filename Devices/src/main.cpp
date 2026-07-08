#include <DHT.h>

#define DHTPIN 4
#define DHTTYPE DHT22
#define SOIL_PIN 34

DHT dht(DHTPIN, DHTTYPE);

void setup()
{
  // Inicializamos el puerto serial a 115200 baudios
  Serial.begin(115200);

  // Mensaje de control para confirmar que el puerto serial responde
  Serial.println("--- Sistema Iniciado Correctamente ---");

  dht.begin();

  Serial.begin(115200);
  delay(1000);
  Serial.println("BOOT_OK");
}

void loop()
{
  float temp = dht.readTemperature();
  float hum = dht.readHumidity();
  int soilRaw = analogRead(SOIL_PIN);

  if (isnan(temp) || isnan(hum))
  {
    Serial.println("ERROR,sensor_read_failed");
  }
  else
  {
    Serial.print("DATA,");
    Serial.print(temp);
    Serial.print(",");
    Serial.print(hum);
    Serial.print(",");
    Serial.println(soilRaw);
  }

  delay(2000);
}