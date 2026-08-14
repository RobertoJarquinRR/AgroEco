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

    setInterval(() => {
        if (statusButton.classList.contains("online")) {
            const nuevaTemp = (23.8 + Math.random() * 1.8).toFixed(1);
            const liveTempEl = document.getElementById("live-temp");
            if (liveTempEl) liveTempEl.textContent = nuevaTemp;
            
            const nuevaHum = Math.floor(65 + Math.random() * 5).toString();
            const liveHumEl = document.getElementById("live-hum");
            if (liveHumEl) liveHumEl.textContent = nuevaHum;
            
            const nuevoViento = Math.floor(10 + Math.random() * 4).toString();
            const liveVientoEl = document.getElementById("live-viento");
            if (liveVientoEl) liveVientoEl.textContent = nuevoViento;

            if (activeMetric === "temperatura" && chartValDisplay) {
                chartValDisplay.textContent = nuevaTemp;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = parseFloat(nuevaTemp);
            } else if (activeMetric === "humedad" && chartValDisplay) {
                chartValDisplay.textContent = nuevaHum;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = parseInt(nuevaHum, 10);
            } else if (activeMetric === "viento" && chartValDisplay) {
                chartValDisplay.textContent = nuevoViento;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = parseInt(nuevoViento, 10);
            }
            historyChart.update('none'); 
        }
    }, 4000);
});

// Puente con C# (WebView2) envuelto en un bloque local para evitar colisiones con 'win' de otros archivos
{
    const winSensores = /** @type {any} */ (window);
    if (winSensores.chrome && winSensores.chrome.webview) {
        winSensores.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const { type, payload } = event.data;

            switch (type) {
                case "ambiente":    
                    if (payload.temperatura) {
                        const t = document.getElementById("live-temp");
                        if (t) t.textContent = payload.temperatura;
                    }
                    if (payload.humedad) {
                        const h = document.getElementById("live-hum");
                        if (h) h.textContent = payload.humedad;
                    }
                    if (payload.viento) {
                        const v = document.getElementById("live-viento");
                        if (v) v.textContent = payload.viento;
                    }
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

                default:
                    console.warn("Tipo de mensaje no reconocido en Sensores:", type);
            }
        });

        winSensores.chrome.webview.postMessage({ type: "ready_sensores" });
    }
}