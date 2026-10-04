using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgroEco.UI.Mensajeros
{
    public class Mensaje
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("screen")]
        public string? Screen { get; set; }
        [JsonPropertyName("type")] //basicamente la peticion por ejemplo : alturamaximapoda cosas asi
        public string? Type { get; set; }
        [JsonPropertyName("payload")]
        public JsonElement PayLoad { get; set; } //el payload es un json que puede ser cualquier cosa


        //constructor para que el mensaje pueda ser visto bien  aunque no se si aplica
        public Mensaje(int id, string screen, string type , JsonElement payload)
        {
            Id = id;
            Screen = screen;
            Type = type;
            PayLoad = payload;
        }

        private static readonly JsonSerializerOptions _opciones =
        new() { PropertyNameCaseInsensitive = true };
        //el que deserializa el mensaje
        public T? LeerPayload<T>()
        {
          
                if (PayLoad.ValueKind == JsonValueKind.Undefined || PayLoad.ValueKind == JsonValueKind.Null)
                    return default; //osea que si el payload es null o undefined devuelve null
            // Deserialize from the JsonElement's raw text using the options
            return JsonSerializer.Deserialize<T>(PayLoad.GetRawText(), _opciones);

        }

    }
}
