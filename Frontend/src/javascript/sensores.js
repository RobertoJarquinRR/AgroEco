// Sensores reales (firmware) vs simulados (sin hardware)
// uiId = elemento del dashboard donde se muestra; metrica = tarjeta/grafico asociado
/** @type {Record<string, {real: boolean, uiId: string | null, metrica: string | null, dec: number}>} */
const SENSORES_CONFIG = {
    temperatura_ambiente: { real: true,  uiId: "live-temp",   metrica: "temperatura", dec: 1 },
    humedad_suelo:        { real: true,  uiId: "live-hum",    metrica: "humedad",     dec: 0 },
    temperatura_suelo:    { real: true,  uiId: null,          metrica: null,          dec: 1 },
    humedad_ambiente:     { real: true,  uiId: null,          metrica: null,          dec: 0 },
    temperatura:          { real: true,  uiId: "live-temp",   metrica: "temperatura", dec: 1 },
    humedad:              { real: true,  uiId: "live-hum",    metrica: "humedad",     dec: 0 },
    viento:               { real: false, uiId: "live-viento", metrica: "viento",      dec: 0 },
    luz:                  { real: false, uiId: "live-luz",    metrica: null,          dec: 0 }
};
const SIMULACION_VIENTO_MS = 4000;

/** Se asigna dentro de DOMContentLoaded (ahi viven el grafico y activeMetric) */
/** @type {(sensor: string, valor: number | string) => void} */
let aplicarLectura = () => {};

// TODO: implementar la UI de alertas (estas funciones no existian en main)
/** @param {any} alerta */
function mostrarAlerta(alerta) { console.info("Alerta recibida:", alerta); }
/** @param {any} id */
function ocultarAlerta(id) { console.info("Alerta resuelta:", id); }
/** @param {any[]} alertas */
function cargarAlertasActivas(alertas) { (alertas || []).forEach(mostrarAlerta); }

document.addEventListener("DOMContentLoaded", () => {
    const statusButton = document.getElementById("status-button");
    if (!statusButton) return;
    
    const chartTitle = document.getElementById("chart-title");
    const chartValDisplay = document.getElementById("chart-val-display");
    const chartUnitDisplay = document.getElementById("chart-unit-display");
    
    const metricCards = document.querySelectorAll(".card-metric");

    const datasetHistorico = {
        temperatura: {
            titulo: "Temperatura",
            unidad: "°C",
            valores: [20, 21.5, 23, 26, 27, 25.5, 24, 22.5, 24.5],
            colorArea: "rgba(250, 165, 51, 0.15)", 
            colorLinea: "#faa533"
        },
        humedad: {
            titulo: "Humedad Suelo",
            unidad: "%",
            valores: [75, 74, 72, 65, 60, 62, 66, 67, 68],
            colorArea: "rgba(47, 160, 132, 0.15)", 
            colorLinea: "#2fa084"
        },
        viento: {
            titulo: "Viento",
            unidad: "km/h",
            valores: [8, 10, 14, 18, 15, 11, 9, 13, 12],
            colorArea: "rgba(59, 157, 133, 0.15)", 
            colorLinea: "#3b9d85"
        }
    };

    const labelsHorarios = ["06:00", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "Ahora"];

    const canvasElement = document.getElementById('historyChart');
    if (!(canvasElement instanceof HTMLCanvasElement)) return;
    const ctx = canvasElement.getContext('2d');
    if (!ctx) return;

    /** @type {keyof typeof datasetHistorico} */
    let activeMetric = "temperatura";

    // @ts-ignore
    let historyChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labelsHorarios,
            datasets: [{
                data: datasetHistorico[activeMetric].valores,
                borderColor: datasetHistorico[activeMetric].colorLinea,
                backgroundColor: datasetHistorico[activeMetric].colorArea,
                borderWidth: 3,
                fill: true,
                tension: 0.4, 
                pointBackgroundColor: datasetHistorico[activeMetric].colorLinea,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    border: { display: false },
                    grid: { color: '#f0f0f0' },
                    ticks: { color: '#bfc6c4', font: { size: 11 } }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#bfc6c4', font: { size: 11 } }
                }
            }
        }
    });

    metricCards.forEach(card => {
        card.addEventListener("click", () => {
            metricCards.forEach(c => c.classList.remove("active"));
            card.classList.add("active");

            const metricAttr = card.getAttribute("data-metric");
            if (!metricAttr || !(metricAttr in datasetHistorico)) return;
            activeMetric = /** @type {keyof typeof datasetHistorico} */ (metricAttr);
            
            const dataConfig = datasetHistorico[activeMetric];

            if (chartTitle) chartTitle.textContent = dataConfig.titulo;
            if (chartUnitDisplay) chartUnitDisplay.textContent = dataConfig.unidad;
            
            const valorSpan = card.querySelector(".metric-value span:first-child");
            if (valorSpan && chartValDisplay) {
                chartValDisplay.textContent = valorSpan.textContent || "";
            }

            historyChart.data.datasets[0].data = dataConfig.valores;
            historyChart.data.datasets[0].borderColor = dataConfig.colorLinea;
            historyChart.data.datasets[0].backgroundColor = dataConfig.colorArea;
            historyChart.data.datasets[0].pointBackgroundColor = dataConfig.colorLinea;
            historyChart.update();
        });
    });

    // Lectura real o simulada: actualiza el numero en pantalla y el grafico activo
    aplicarLectura = (sensor, valor) => {
        const cfg = SENSORES_CONFIG[sensor];
        const num = parseFloat(String(valor));
        if (!cfg || !cfg.uiId || Number.isNaN(num)) return;

        const el = document.getElementById(cfg.uiId);
        if (el) el.textContent = num.toFixed(cfg.dec);

        if (cfg.metrica && cfg.metrica in datasetHistorico) {
            const serie = datasetHistorico[/** @type {keyof typeof datasetHistorico} */ (cfg.metrica)].valores;
            serie[labelsHorarios.length - 1] = num;
            if (cfg.metrica === activeMetric) {
                if (chartValDisplay) chartValDisplay.textContent = num.toFixed(cfg.dec);
                historyChart.update('none');
            }
        }
    };

    // Solo se simula lo que NO tiene hardware (viento)
    setInterval(() => {
        if (statusButton.classList.contains("online")) {
            aplicarLectura("viento", Math.floor(10 + Math.random() * 4));
        }
    }, SIMULACION_VIENTO_MS);
});

// Puente con C# (WebView2) envuelto en un bloque local para evitar colisiones con 'win' de otros archivos
{
    const winSensores = /** @type {any} */ (window);
    if (winSensores.chrome && winSensores.chrome.webview) {
        winSensores.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const { type, payload } = event.data;

            switch (type) {
                case "ambiente":
                    Object.entries(payload || {}).forEach(([sensor, valor]) => aplicarLectura(sensor, /** @type {any} */ (valor)));
                    break;

                case "lecturaProcesada":
                    if (payload && payload.exito !== false && payload.sensor !== undefined) aplicarLectura(payload.sensor, payload.valor);
                    break;
                    
                case "estado_conexion":
                    const statusButton = document.getElementById("status-button");
                    if (statusButton) {
                        const statusText = statusButton.querySelector(".status-text");
                        if (payload.online) {
                            statusButton.className = "status-badge online";
                            if (statusText) statusText.textContent = "En línea";
                        } else {
                            statusButton.className = "status-badge offline";
                            if (statusText) statusText.textContent = "Fuera de línea";
                        }
                    }
                    break;

                case "alerta":
                    mostrarAlerta(payload);
                    break;

                case "alertaResuelta":
                    ocultarAlerta(payload.id);
                    break;

                case "alertasActivas":
                    cargarAlertasActivas(payload.alertas);
                    break;

                default:
                    console.warn("Tipo de mensaje no reconocido en Sensores:", type);
            }
        });
 
        // Solicitar alertas activas al iniciar
        winSensores.chrome.webview.postMessage({ type: "obtenerAlertasActivas" });
 
        winSensores.chrome.webview.postMessage({ type: "ready_sensores" });
    }
}