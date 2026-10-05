using System;
using System.Collections.Generic;
using System.Linq;
using AgroEco.UI.Clases;
using AgroEco.UI.Mensajeros;

namespace AgroEco.UI.Handlers
{
    public class FinanzasHandler
    {
        private readonly Action<string, object> _enviar;
        private readonly List<RegistroFinanciero> _registros = new();
        private long _siguienteId = 1;

        public FinanzasHandler(Action<string, object> enviar)
        {
            _enviar = enviar;
        }

        public void ManejarMensaje(Mensaje msg)
        {
            switch (msg.Type)
            {
                case "ready_finanzas":
                    EnviarResumen();
                    EnviarRegistros();
                    break;

                case "nuevoRegistroFinanciero":
                    GuardarRegistro(msg);
                    break;
            }
        }

        private void GuardarRegistro(Mensaje msg)
        {
            var registro = msg.LeerPayload<RegistroFinanciero>();
            if (registro is null) return;

            // C# también valida, no solo el JS
            if (registro.Monto <= 0) return;
            if (registro.Tipo != "ingreso" && registro.Tipo != "costo") return;

            registro.Id = _siguienteId++;
            _registros.Add(registro);

            EnviarResumen();
            EnviarRegistros();
        }

        private void EnviarResumen()
        {
            decimal ingresos = 0;
            decimal costos = 0;

            foreach (var r in _registros)
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

        private void EnviarRegistros()
        {
            var lista = _registros.Select(r => new
            {
                id = r.Id,
                descripcion = r.Descripcion,
                monto = r.Monto,
                fecha = r.Fecha.ToString("dd/MM/yyyy"),
                tipo = r.Tipo
            }).ToList();

            _enviar("cargar_registros", lista);
        }
    }
}