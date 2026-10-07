// ============================================
// AGROECO - MÓDULO SENSORES
// Flujo: Firmware (serial) → C# SerialHostedService → SensorReadingHandler → ambiente message → UI
// Sensores REALES (firmware): temperatura_suelo, temperatura_ambiente, humedad_suelo, humedad_ambiente
// Sensores SIMULADOS (sin hardware): viento, luz
// ============================================

// ============================================
// AGROECO - MÓDULO SENSORES
// Flujo: Firmware (serial) → C# SerialHostedService → SensorReadingHandler → ambiente message → UI
// Sensores REALES (firmware): temperatura_suelo, temperatura_ambiente, humedad_suelo, humedad_ambiente
// Sensores SIMULADOS (sin hardware): viento, luz
// ============================================

// ===== CONFIGURACION DE SENSORES: REALES vs SIMULADOS =====
const SENSORES_CONFIG = {
    // SENSORES REALES (vienen del firmware via serial/C#)
    temperatura_suelo: { 
        real: true, 
        fuente: "firmware", 
        sensorName: "temperatura_suelo", 
        uiId: "live-temp", 
        unidad: "°C",
        historicoKey: "temperatura"
    },
    temperatura_ambiente: { 
        real: true, 
        fuente: "firmware", 
        sensorName: "temperatura_ambiente", 
        uiId: "live-temp", 
        unidad: "°C",
        historicoKey: "temperatura"
    },
    humedad_suelo: { 
        real: true, 
        fuente: "firmware", 
        sensorName: "humedad_suelo", 
        uiId: "live-hum", 
        unidad: "%",
        historicoKey: "humedad"
    },
    humedad_ambiente: { 
        real: true, 
        fuente: "firmware", 
        sensorName: "humedad_ambiente", 
        uiId: "live-hum", 
        unidad: "%",
        historicoKey: "humedad"
    },

    // SENSORES SIMULADOS (no existen en firmware aún)
    viento: { 
        real: false, 
        fuente: "simulado", 
        sensorName: "viento", 
        uiId: "live-viento", 
        unidad: "km/h",
        historicoKey: "viento",
        rango: { min: 10, max: 14 } // km/h
    },
    luz: { 
        real: false, 
        fuente: "simulado", 
        sensorName: "luz", 
        uiId: "live-luz", 
        unidad: "lux",
        historicoKey: "luz",
        rango: { min: 0, max: 100000 }
    }
};

// Función para obtener info de un sensor
function getSensorConfig(sensor) {
    return SENSORES_CONFIG[sensor] || { real: false, fuente: "desconocido" };
}

// Verificar si un sensor es real
function esSensorReal(sensor) {
    const config = getSensorConfig(sensor);
    return config.real === true;
}

// Obtener lista de sensores reales
function getSensoresReales() {
    return Object.entries(SENSORES_CONFIG)
        .filter(([_, config]) => config.real)
        .map(([key, config]) => ({ key, ...config }));
}

// Obtener lista de sensores simulados
function getSensoresSimulados() {
    return Object.entries(SENSORES_CONFIG)
        .filter(([_, config]) => !config.real)
        .map(([key, config]) => ({ key, ...config }));
}

// Exportar para debug/consola
window.SENSORES_CONFIG = SENSORES_CONFIG;
window.getSensorConfig = getSensorConfig;
window.esSensorReal = esSensorReal;
window.getSensoresReales = getSensoresReales;
window.getSensoresSimulados = getSensoresSimulados;

// ===== HISTÓRICO DE LECTURAS (para gráficos) =====
const HISTORICO_MAX_PUNTOS = 50;
const historicoData = {
    temperatura: [],
    humedad: [],
    viento: []
};

// Agregar punto al histórico
function agregarAlHistorico(tipo, valor) {
    const key = tipo === "temperatura_suelo" || tipo === "temperatura_ambiente" ? "temperatura" :
                tipo === "humedad_suelo" || tipo === "humedad_ambiente" ? "humedad" :
                tipo === "viento" ? "viento" : null;
    
    if (!key || !historicoData[key]) return;
    
    const punto = { 
        valor: parseFloat(valor), 
        timestamp: Date.now() 
    };
    
    historicoData[key].push(punto);
    if (historicoData[key].length > HISTORICO_MAX_PUNTOS) {
        historicoData[key].shift();
    }
    
    // Actualizar gráfico si está activo
    actualizarGraficoHistorico(tipo);
}

// Obtener datos para gráfico (últimos N puntos)
function obtenerHistoricoParaGrafico(tipo, maxPuntos = 20) {
    const key = tipo === "temperatura_suelo" || tipo === "temperatura_ambiente" ? "temperatura" :
                tipo === "humedad_suelo" || tipo === "humedad_ambiente" ? "humedad" :
                tipo === "viento" ? "viento" : null;
    
    if (!key || !historicoData[key]) return { labels: [], valores: [] };
    
    const datos = historicoData[key].slice(-maxPuntos);
    return {
        labels: datos.map(d => new Date(d.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
        valores: datos.map(d => d.valor)
    };
}

// Actualizar gráfico histórico
function actualizarGraficoHistorico(tipo) {
    if (activeMetric !== tipo) return;
    
    const datos = obtenerHistoricoParaGrafico(tipo, labelsHorarios.length);
    if (historyChart && datos.valores.length > 0) {
        historyChart.data.labels = datos.labels;
        historyChart.data.datasets[0].data = datos.valores;
        historyChart.update('none');
    }
}

function obtenerHistoricoParaGrafico(tipo, maxPuntos = 20) {
    const key = tipo === "temperatura_suelo" || tipo === "temperatura_ambiente" ? "temperatura" :
                tipo === "humedad_suelo" || tipo === "humedad_ambiente" ? "humedad" :
                tipo === "viento" ? "viento" : null;
    
    if (!key || !historicoData[key]) return { labels: [], valores: [] };
    
    const datos = historicoData[key].slice(-maxPuntos);
    return {
        labels: datos.map(d => new Date(d.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
        valores: datos.map(d => d.valor)
    };
}

function actualizarGraficoHistorico(tipo) {
    if (activeMetric !== tipo) return;
    
    const datos = obtenerHistoricoParaGrafico(tipo, labelsHorarios.length);
    if (historyChart && datos.valores.length > 0) {
        historyChart.data.labels = datos.labels;
        historyChart.data.datasets[0].data = datos.valores;
        historyChart.update('none');
    }
}

// Exportar funciones para debug
window.historicoData = historicoData;
window.agregarAlHistorico = agregarAlHistorico;
window.obtenerHistoricoParaGrafico = obtenerHistoricoParaGrafico;

// ===== CONFIGURACIÓN DE SIMULACIÓN (solo para sensores sin hardware) =====
const SIMULACION_CONFIG = {
    viento: {
        activo: true,
        intervalo: 4000, // ms
        generar: () => Math.floor(10 + Math.random() * 4).toString() // 10-14 km/h
    },
    luz: {
        activo: false, // desactivado por ahora
        intervalo: 10000,
        generar: () => Math.floor(Math.random() * 100000).toString()
    }
};

// Variable para controlar simulación de viento
let simulacionVientoInterval = null;

// Iniciar/parar simulación de sensores sin hardware
function iniciarSimulacionSensores() {
    // Solo simular viento (no hay hardware para viento)
    if (SIMULACION_CONFIG.viento.activo && !simulacionVientoInterval) {
        simulacionVientoInterval = setInterval(() => {
            const valor = SIMULACION_CONFIG.viento.generar();
            actualizarLecturaEnUI("viento", valor);
            agregarAlHistorico("viento", valor);
        }, SIMULACION_CONFIG.viento.intervalo);
    }
}

function detenerSimulacionSensores() {
    if (simulacionVientoInterval) {
        clearInterval(simulacionVientoInterval);
        simulacionVientoInterval = null;
    }
}

// Iniciar simulación al cargar (solo viento)
document.addEventListener("DOMContentLoaded", () => {
    iniciarSimulacionSensores();
    
    // Solicitar configuración inicial al backend
    winSensores.chrome.webview.postMessage({ type: "ready_sensores" });
});

// ===== FUNCIONES EXISTENTES (mantenidas) =====

function actualizarLecturaEnUI(sensor, valor) {
    const config = getSensorConfig(sensor);
    if (!config || !config.uiId) return;
    
    const el = document.getElementById(config.uiId);
    if (el) {
        el.textContent = typeof valor === 'number' ? valor.toFixed(1) : valor;
        
        // También actualizar el histórico si es sensor real
        if (config.real) {
            agregarAlHistorico(config.historicoKey, valor);
        }
    }
}

function agregarAlHistorico(tipo, valor) {
    const key = tipo === "temperatura_suelo" || tipo === "temperatura_ambiente" ? "temperatura" :
                tipo === "humedad_suelo" || tipo === "humedad_ambiente" ? "humedad" :
                tipo === "viento" ? "viento" : null;
    
    if (!key || !historicoData[key]) return;
    
    const punto = { 
        valor: parseFloat(valor), 
        timestamp: Date.now() 
    };
    
    historicoData[key].push(punto);
    if (historicoData[key].length > HISTORICO_MAX_PUNTOS) {
        historicoData[key].shift();
    }
    
    // Actualizar gráfico si está activo
    actualizarGraficoHistorico(tipo);
}

function actualizarGraficoHistorico(tipo) {
    if (activeMetric !== tipo) return;
    
    const datos = obtenerHistoricoParaGrafico(tipo, labelsHorarios.length);
    if (historyChart && datos.valores.length > 0) {
        historyChart.data.labels = datos.labels;
        historyChart.data.datasets[0].data = datos.valores;
        historyChart.update('none');
    }
}

function obtenerHistoricoParaGrafico(tipo, maxPuntos = 20) {
    const key = tipo === "temperatura_suelo" || tipo === "temperatura_ambiente" ? "temperatura" :
                tipo === "humedad_suelo" || tipo === "humedad_ambiente" ? "humedad" :
                tipo === "viento" ? "viento" : null;
    
    if (!key || !historicoData[key]) return { labels: [], valores: [] };
    
    const datos = historicoData[key].slice(-maxPuntos);
    return {
        labels: datos.map(d => new Date(d.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
        valores: datos.map(d => d.valor)
    };
}

function actualizarGraficoHistorico(tipo) {
    if (activeMetric !== tipo) return;
    
    const datos = obtenerHistoricoParaGrafico(tipo, labelsHorarios.length);
    if (historyChart && datos.valores.length > 0) {
        historyChart.data.labels = datos.labels;
        historyChart.data.datasets[0].data = datos.valores;
        historyChart.update('none');
    }
}

// Puente con C# (WebView2) envuelto en un bloque local para evitar colisiones con 'win' de otros archivos
{
    const winSensores = /** @type {any} */ (window);
    if (winSensores.chrome && winSensores.chrome.webview) {
        winSensores.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const { type, payload } = event.data;

            switch (type) {
                case "ambiente":
                    if (payload.temperatura !== undefined) {
                        const t = document.getElementById("live-temp");
                        if (t) t.textContent = payload.temperatura;
                        agregarAlHistorico("temperatura", payload.temperatura);
                    }
                    if (payload.humedad !== undefined) {
                        const h = document.getElementById("live-hum");
                        if (h) h.textContent = payload.humedad;
                        agregarAlHistorico("humedad", payload.humedad);
                    }
                    if (payload.viento !== undefined) {
                        const v = document.getElementById("live-viento");
                        if (v) v.textContent = payload.viento;
                        agregarAlHistorico("viento", payload.viento);
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

                case "lecturaProcesada":
                    if (payload.sensor && payload.valor !== undefined) {
                        actualizarLecturaEnUI(payload.sensor, payload.valor);
                    }
                    break;

                case "lecturas":
                    if (payload.lecturas && Array.isArray(payload.lecturas)) {
                        payload.lecturas.forEach(l => actualizarLecturaEnUI(l.sensor, l.valor));
                    }
                    break;

                default:
                    console.warn("Tipo de mensaje no reconocido en Sensores:", type);
            }
        });

        // Solicitar configuración inicial al backend
        winSensores.chrome.webview.postMessage({ type: "ready_sensores" });
    }
}