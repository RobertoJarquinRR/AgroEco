using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs.Actions.Configuration;

namespace AgroEco.Core.Jobs.Actions;

public static class ActionConfigurationParser
{
    public static ActionConfiguration Parse(string? actionTypeId, JsonElement config)
    {
        return actionTypeId switch
        {
            "sendAlert" => ParseSendAlert(config),
            "noop" => ParseNoOp(config),
            _ => ParseExecuteTask(config),
        };
    }

    private static SendAlertActionConfiguration ParseSendAlert(JsonElement config)
    {
        return new SendAlertActionConfiguration
        {
            Title = ReadString(config, "title"),
            Message = ReadString(config, "message"),
            Level = ReadLevel(config),
            EnableChannels = ReadStringList(config, "enableChannels"),
            DisableChannels = ReadStringList(config, "disableChannels"),
            Options = ReadOptions(config)
        };
    }

    private static ExecuteTaskActionConfiguration ParseExecuteTask(JsonElement config)
    {
        int insumoId = 0;
        decimal cantidadDescontar = 0;
        decimal costoUnitario = 0;
        string? descripcion = null;
        string? cultivo = null;
        string? categoriaInsumo = null;

        if (TryGetProperty(config, "insumoId", out var insumoIdEl) && insumoIdEl.ValueKind == JsonValueKind.Number)
        {
            insumoId = insumoIdEl.GetInt32();
        }
        if (TryGetProperty(config, "cantidadDescontar", out var cantidadEl) && cantidadEl.ValueKind == JsonValueKind.Number)
        {
            cantidadDescontar = cantidadEl.GetDecimal();
        }
        if (TryGetProperty(config, "costoUnitario", out var costoEl) && costoEl.ValueKind == JsonValueKind.Number)
        {
            costoUnitario = costoEl.GetDecimal();
        }
        if (TryGetProperty(config, "descripcion", out var descEl) && descEl.ValueKind == JsonValueKind.String)
        {
            descripcion = descEl.GetString();
        }
        if (TryGetProperty(config, "cultivo", out var cultivoEl) && cultivoEl.ValueKind == JsonValueKind.String)
        {
            cultivo = cultivoEl.GetString();
        }
        if (TryGetProperty(config, "categoriaInsumo", out var catEl) && catEl.ValueKind == JsonValueKind.String)
        {
            categoriaInsumo = catEl.GetString();
        }

        return new ExecuteTaskActionConfiguration
        {
            InsumoId = insumoId,
            CantidadDescontar = cantidadDescontar,
            CostoUnitario = costoUnitario,
            Descripcion = descripcion,
            Cultivo = cultivo,
            CategoriaInsumo = categoriaInsumo
        };
    }

    private static NoOpActionConfiguration ParseNoOp(JsonElement config)
    {
        long insumoId = 0;
        decimal cantidadDescontar = 0;
        decimal costoUnitario = 0;
        string? descripcion = null;

        if (TryGetProperty(config, "insumoId", out var insumoIdEl) && insumoIdEl.ValueKind == JsonValueKind.Number)
        {
            insumoId = insumoIdEl.GetInt64();
        }
        if (TryGetProperty(config, "cantidadDescontar", out var cantidadEl) && cantidadEl.ValueKind == JsonValueKind.Number)
        {
            cantidadDescontar = cantidadEl.GetDecimal();
        }
        if (TryGetProperty(config, "costoUnitario", out var costoEl) && costoEl.ValueKind == JsonValueKind.Number)
        {
            costoUnitario = costoEl.GetDecimal();
        }
        if (TryGetProperty(config, "descripcion", out var descEl) && descEl.ValueKind == JsonValueKind.String)
        {
            descripcion = descEl.GetString();
        }

        return new NoOpActionConfiguration
        {
            InsumoId = insumoId,
            CantidadDescontar = cantidadDescontar,
            CostoUnitario = costoUnitario,
            Descripcion = descripcion
        };
    }

    private static bool TryGetProperty(JsonElement config, string name, out JsonElement value)
    {
        value = default;
        return config.ValueKind == JsonValueKind.Object && config.TryGetProperty(name, out value);
    }

    private static string ReadString(JsonElement config, string name)
    {
        if (TryGetProperty(config, name, out var element) && element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static AlertLevel ReadLevel(JsonElement config)
    {
        if (!TryGetProperty(config, "level", out var element))
        {
            return AlertLevel.Info;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int numeric))
        {
            return Enum.IsDefined(typeof(AlertLevel), numeric)
                ? (AlertLevel)numeric
                : AlertLevel.Info;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            string raw = element.GetString() ?? string.Empty;
            if (int.TryParse(raw, out int numericString) && Enum.IsDefined(typeof(AlertLevel), numericString))
            {
                return (AlertLevel)numericString;
            }

            if (Enum.TryParse(raw, ignoreCase: true, out AlertLevel parsed))
            {
                return parsed;
            }
        }

        return AlertLevel.Info;
    }

    private static List<string> ReadStringList(JsonElement config, string name)
    {
        if (!TryGetProperty(config, name, out var element))
        {
            return new List<string>();
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!.Trim())
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            return (element.GetString() ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new List<string>();
    }

    private static Dictionary<string, object> ReadOptions(JsonElement config)
    {
        if (!TryGetProperty(config, "options", out var element))
        {
            return new Dictionary<string, object>();
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            return ToOptionDictionary(element);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            string raw = element.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Dictionary<string, object>();
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(raw);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    return ToOptionDictionary(document.RootElement);
                }
            }
            catch (JsonException)
            {
                return new Dictionary<string, object>();
            }
        }

        return new Dictionary<string, object>();
    }

    private static Dictionary<string, object> ToOptionDictionary(JsonElement element)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => property.Value.TryGetInt64(out long integer)
                    ? integer
                    : property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => property.Value.GetRawText()
            };
        }

        return result;
    }
}
