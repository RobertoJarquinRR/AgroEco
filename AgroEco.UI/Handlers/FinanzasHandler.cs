using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Finanzas.Persistence;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class FinanzasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly ILogger<FinanzasHandler> _logger;
        private readonly GetAllRegistroFinanciero _getAllRegistros;
        private readonly CreateRegistroFinanciero _createRegistro;

        public FinanzasHandler(
            Action<string, object> enviar,
            GetAllRegistroFinanciero getAllRegistros,
            CreateRegistroFinanciero createRegistro,
            ILogger<FinanzasHandler> logger)
        {
            _enviar = enviar;
            _getAllRegistros = getAllRegistros;
            _createRegistro = createRegistro;
            _logger = logger;
        }

        public async void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_finanzas":
                    await EnviarResumenAsync();
                    await EnviarRegistrosAsync();
                    break;

                case "nuevoRegistroFinanciero":
                    await GuardarRegistroAsync(msg);
                    break;
            }
        }

        private async Task GuardarRegistroAsync(Mensaje msg)
        {
            try
            {
                var registro = msg.LeerPayload<RegistroFinancieroDto>();
                if (registro is null) return;

                if (registro.Monto <= 0) return;
                if (registro.Tipo != "ingreso" && registro.Tipo != "costo") return;

                var result = await _createRegistro.HandleAsync(
                    registro.Tipo,
                    registro.Cultivo,
                    registro.Categoria,
                    registro.Monto,
                    registro.Fecha,
                    registro.Descripcion,
                    registro.TaskId);

                if (result.Success)
                {
                    _logger.LogInformation("Registro financiero creado: {Tipo} {Monto} - {Descripcion}", 
                        result.Value.Tipo, result.Value.Monto, result.Value.Descripcion);
                    await EnviarResumenAsync();
                    await EnviarRegistrosAsync();
                }
                else
                {
                    _enviar("finanzasError", new { mensaje = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando registro financiero");
                _enviar("finanzasError", new { mensaje = ex.Message });
            }
        }

        private async Task EnviarResumenAsync()
        {
            try
            {
                var result = await _getAllRegistros.HandleAsync();
                if (!result.Success || result.Value == null)
                {
                    _enviar("finanzas_cargadas", new { ingresos = 0m, costos = 0m, ganancias = 0m, gananciaNeta = 0m });
                    return;
                }

                decimal ingresos = 0;
                decimal costos = 0;

                foreach (var r in result.Value)
                {
                    if (r.Tipo == "ingreso") ingresos += r.Monto;
                    else if (r.Tipo == "costo") costos += r.Monto;
                }

                decimal ganancias = ingresos - costos;
                decimal gananciaNeta = ingresos == 0
                    ? 0
                    : Math.Round(ganancias / ingresos * 100, 1);

                _enviar("finanzas_cargadas", new { ingresos, costos, ganancias, gananciaNeta });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando resumen financiero");
                _enviar("finanzas_cargadas", new { ingresos = 0m, costos = 0m, ganancias = 0m, gananciaNeta = 0m });
            }
        }

        private async Task EnviarRegistrosAsync()
        {
            try
            {
                var result = await _getAllRegistros.HandleAsync();
                if (!result.Success || result.Value == null)
                {
                    _enviar("cargar_registros", new List<object>());
                    return;
                }

                var lista = result.Value.Select(r => new
                {
                    id = r.Id,
                    descripcion = r.Descripcion,
                    monto = r.Monto,
                    fecha = r.Fecha.ToString("dd/MM/yyyy"),
                    tipo = r.Tipo
                }).ToList();

                _enviar("cargar_registros", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando registros financieros");
                _enviar("cargar_registros", new List<object>());
            }
        }

        private record RegistroFinancieroDto(
            string Tipo,
            string? Cultivo,
            string Categoria,
            decimal Monto,
            DateOnly Fecha,
            string Descripcion,
            int? TaskId = null);
    }
}