document.addEventListener("DOMContentLoaded", () => {
    
  
    const statusButton = document.getElementById("status-button");
    const statusText = statusButton.querySelector(".status-text");
    const tiempoUpdate = document.getElementById("tiempo-update");
    const leftColumn = document.querySelector(".left-column");
    const rightColumn = document.querySelector(".card-devices");
    
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

    const ctx = document.getElementById('historyChart').getContext('2d');
    let activeMetric = "temperatura";

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

            activeMetric = card.getAttribute("data-metric");
            const dataConfig = datasetHistorico[activeMetric];

            chartTitle.textContent = dataConfig.titulo;
            chartUnitDisplay.textContent = dataConfig.unit || dataConfig.unidad;
            
            const valorActual = card.querySelector(".metric-value span:first-child").textContent;
            chartValDisplay.textContent = valorActual;

            historyChart.data.datasets[0].data = dataConfig.valores;
            historyChart.data.datasets[0].borderColor = dataConfig.colorLinea;
            historyChart.data.datasets[0].backgroundColor = dataConfig.colorArea;
            historyChart.data.datasets[0].pointBackgroundColor = dataConfig.colorLinea;
            historyChart.update();
        });
    });

    statusButton.addEventListener("click", () => {
        if (statusButton.classList.contains("online")) {
            statusButton.classList.replace("online", "offline");
            statusText.textContent = "Fuera de línea";
            

            leftColumn.classList.add("offline-mode");
            rightColumn.classList.add("offline-mode");
        } else {
            statusButton.classList.replace("offline", "online");
            statusText.textContent = "En línea";
            
            leftColumn.classList.remove("offline-mode");
            rightColumn.classList.remove("offline-mode");
            
            tiempoUpdate.textContent = "0";
        }
    });

    setInterval(() => {
        if (statusButton.classList.contains("online")) {
            const nuevaTemp = (23.8 + Math.random() * 1.8).toFixed(1);
            document.getElementById("live-temp").textContent = nuevaTemp;
            
            const nuevaHum = Math.floor(65 + Math.random() * 5);
            document.getElementById("live-hum").textContent = nuevaHum;
            
            const nuevoViento = Math.floor(10 + Math.random() * 4);
            document.getElementById("live-viento").textContent = nuevoViento;

            if (activeMetric === "temperatura") {
                chartValDisplay.textContent = nuevaTemp;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = nuevaTemp;
            } else if (activeMetric === "humedad") {
                chartValDisplay.textContent = nuevaHum;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = nuevaHum;
            } else if (activeMetric === "viento") {
                chartValDisplay.textContent = nuevoViento;
                historyChart.data.datasets[0].data[labelsHorarios.length - 1] = nuevoViento;
            }
            historyChart.update('none'); 
        }
    }, 4000);
});