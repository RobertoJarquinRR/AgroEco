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
        public JsonElement Payload { get; set; } //el payload es un json que puede ser cualquier cosa


        
        private static readonly JsonSerializerOptions _opciones =
        new() { PropertyNameCaseInsensitive = true };
        //el que deserializa el mensaje
        public T? LeerPayload<T>()
        {
          
                if (Payload.ValueKind == JsonValueKind.Undefined || Payload.ValueKind == JsonValueKind.Null)
                    return default; //osea que si el payload es null o undefined devuelve null
            // Deserialize from the JsonElement's raw text using the options
            return JsonSerializer.Deserialize<T>(Payload.GetRawText(), _opciones);

        }

    }
}
