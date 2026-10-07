//validador del sidebar
if(typeof updateSidebar === 'function') updateSidebar();
// ============================================================
// dashboard.js - AgroEco
// Diseñado para WPF + WebView2. C# manda datos via postMessage
// y este script solo pinta el DOM. No hay lógica de negocio acá.
// ============================================================

// ---------- Referencias base (cacheadas, no querySelector cada vez) ----------
const el = {
    // Hero header (original)
    temp: document.getElementById("Temperatura"),
    notaTemp: document.getElementById("nota-temperatura"),
    humedad: document.getElementById("Humedad"),
    notaHumedad: document.getElementById("nota-humedad"),
    fincaName: document.getElementById("id-finca"),
    
    // Dashboard grid - métricas principales
    liveTemp: document.getElementById("live-temp"),
    tempStatus: document.getElementById("temp-status"),
    liveHum: document.getElementById("live-hum"),
    humStatus: document.getElementById("hum-status"),
    liveSensores: document.getElementById("live-sensores"),
    sensStatus: document.getElementById("sens-status"),
    
    // Stats summary (derecha)
    statSensores: document.getElementById("stat-sensores"),
    statAlertas: document.getElementById("stat-alertas"),
    statTareasActivas: document.getElementById("stat-tareas-activas"),
    statTareasCompletadas: document.getElementById("stat-tareas-completadas"),
    statPlagas: document.getElementById("stat-plagas"),
    
    // Header mejorado
    fincaNombre: document.getElementById("finca-nombre-display"),
    tiempoUpdate: document.getElementById("tiempo-update"),
    statusBtn: document.getElementById("status-button"),
    
    // Chart
    chartTitle: document.getElementById("chart-title"),
    chartVal: document.getElementById("chart-val-display"),
    chartUnit: document.getElementById("chart-unit-display"),
    
    // Umbrales
    umbralesContainer: document.getElementById("umbralesContainer"),
    
    // Estado cultivo
    estadoBadge: document.getElementById("estado-cultivo-badge"),
    estadoMsg: document.getElementById("estado-cultivo-msg"),
    
    // Alertas
    alertCount: document.getElementById("alert-count"),
    alertasContainer: document.getElementById("alertasContainer"),
    
    // Tareas
    tareasContainer: document.getElementById("tareasContainer"),
    
    // Resumen semanal (original)
    resumenDias: document.getElementById("resumen-container") || document.querySelector(".boxesC"),
    plagasMsg: document.getElementById("dashReportPlagas"),
    boxesD: document.querySelector(".boxesD"),
    alertasCont: document.querySelector(".alertas .cards"),
    alertNotif: document.getElementById("alertNotification"),
    tareasCont: document.querySelector(".tareas"),
};

// ---------- 1. Temperatura / Humedad ambiente ----------
/**
 * @param {Object} data
 * @param {string|number} [data.temperatura]
 * @param {string} [data.notaTemperatura]
 * @param {string|number} [data.humedad]
 * @param {string} [data.notaHumedad]
 */
function updateAmbiente({ temperatura, notaTemperatura, humedad, notaHumedad }) {
    // Actualiza elementos originales (HeroHeader)
    if (temperatura !== undefined && el.temp) el.temp.textContent = `${temperatura}°C`;
    if (notaTemperatura !== undefined && el.notaTemp) el.notaTemp.textContent = notaTemperatura;
    if (humedad !== undefined && el.humedad) el.humedad.textContent = `${humedad}%`;
    if (notaHumedad !== undefined && el.notaHumedad) el.notaHumedad.textContent = notaHumedad; 
    
    // Actualiza elementos nuevos (metric cards)
    if (temperatura !== undefined && el.liveTemp) el.liveTemp.textContent = String(temperatura);
    if (notaTemperatura !== undefined && el.tempStatus) el.tempStatus.textContent = notaTemperatura;
    if (humedad !== undefined && el.liveHum) el.liveHum.textContent = String(humedad);
    if (notaHumedad !== undefined && el.humStatus) el.humStatus.textContent = notaHumedad;
}

// ---------- 2. Nombre de finca ----------
/** @param {string} nombre */
function updateFinca(nombre) {
    if (el.fincaName) el.fincaName.textContent = `Finca : ${nombre}`;
    if (el.fincaNombre) el.fincaNombre.textContent = nombre;
}

// ---------- 3. Boxes superiores (sensores/alertas/tareas activas/completadas) ----------
/**
 * @param {Object} data
 * @param {string|number} [data.sensoresActivos]
 * @param {string|number} [data.alertas]
 * @param {string|number} [data.tareasActivas]
 * @param {string|number} [data.tareasCompletadas]
 * @param {string|number} [data.plagas]
 */
function updateStats({ sensoresActivos, alertas, tareasActivas, tareasCompletadas, plagas }) {
    // Original: boxesD .num
    if (!el.boxesD) return;
    const nums = el.boxesD.querySelectorAll(".num");
    if (sensoresActivos !== undefined && nums[0]) nums[0].textContent = String(sensoresActivos);
    if (alertas !== undefined && nums[1]) nums[1].textContent = String(alertas);
    if (tareasActivas !== undefined && nums[2]) nums[2].textContent = String(tareasActivas);
    if (tareasCompletadas !== undefined && nums[3]) nums[3].textContent = String(tareasCompletadas);
    
    // Nuevos: stat-*
    if (sensoresActivos !== undefined) {
        if (el.statSensores) el.statSensores.textContent = String(sensoresActivos);
        if (el.liveSensores) el.liveSensores.textContent = String(sensoresActivos);
    }
    if (alertas !== undefined) {
        if (el.statAlertas) el.statAlertas.textContent = String(alertas);
        if (el.alertCount) el.alertCount.textContent = String(alertas);
    }
    if (tareasActivas !== undefined && el.statTareasActivas) el.statTareasActivas.textContent = String(tareasActivas);
    if (tareasCompletadas !== undefined && el.statTareasCompletadas) el.statTareasCompletadas.textContent = String(tareasCompletadas);
    if (plagas !== undefined && el.statPlagas) el.statPlagas.textContent = String(plagas);
}

// ---------- 4. Resumen semanal (Temp / Humedad) ----------
// data = [{ dia:"Lun", temp:25, humedad:60, icon:"/images/svg-hackaton/Vector (6).svg" }, ...]
/** @param {Array<{dia: string, temp: number|string, humedad: number|string, icon?: string}>} data */
function updateResumenSemanal(data) {
    if (!Array.isArray(data) || data.length === 0 || !el.resumenDias) return;

    el.resumenDias.innerHTML = data.map(d => `
        <div class="boxCalendar">
            <span>${d.dia}</span>
            <img src="${d.icon || '/images/svg-hackaton/Vector (6).svg'}" alt="clima" width="25px" height="25px">
            <span>${d.temp}°C</span> <!--
            <span>${d.humedad}%</span>
        </div>
    `).join("");
}

// ---------- 5. Estado del cultivo / plagas ----------
/**
 * @param {Object} data
 * @param {boolean} [data.hayPlagas]
 * @param {string} [data.mensaje]
 * @param {string} [data.salud]
 */
function updateEstadoCultivo({ hayPlagas, mensaje, salud }) {
    // Original
    if (el.plagasMsg) {
        el.plagasMsg.textContent = mensaje || (hayPlagas ? "Plagas detectadas" : "Sin plagas reportadas");
        el.plagasMsg.style.color = hayPlagas ? "#BC6661" : "";
    }
    // Nuevo
    if (el.estadoBadge && el.estadoMsg) {
        if (hayPlagas) {
            el.estadoBadge.textContent = "Plagas detectadas";
            el.estadoBadge.className = "estado-badge critico";
        } else if (salud === "atencion") {
            el.estadoBadge.textContent = "Requiere atención";
            el.estadoBadge.className = "estado-badge atencion";
        } else {
            el.estadoBadge.textContent = "Saludable";
            el.estadoBadge.className = "estado-badge saludable";
        }
        el.estadoMsg.textContent = mensaje || (hayPlagas ? "Se detectaron plagas en el cultivo" : "Sin plagas reportadas · condiciones óptimas");
    }
}

// ---------- 6. Alertas (cards dinámicas) ----------
/** @param {Array<{titulo: string, tiempo: string, nivel?: string}>} data */
function renderAlertas(data) {
    // Original container
    if (el.alertasCont) {
        if (!Array.isArray(data) || data.length === 0) {
            el.alertasCont.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay alertas recientes</p>";
        } else {
            el.alertasCont.innerHTML = data.map(a => `
                <div class="card">
                    <img src="${a.icon || '/images/svg-hackaton/alerta1.svg'}" alt="alerta">
                    <div class="join">
                        <h4>${a.titulo}</h4>
                        <p>Hace : <span>${a.tiempo}</span></p>
                    </div>
                </div>
            `).join("");
        }
    }
    // New container
    if (el.alertasContainer) {
        if (!Array.isArray(data) || data.length === 0) {
            el.alertasContainer.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay alertas recientes</p>";
        } else {
            el.alertasContainer.innerHTML = data.slice(0, 5).map(a => `
                <div class="alerta-card">
                    <div class="alerta-icon ${a.nivel === "critica" ? "critical" : a.nivel === "alta" ? "warning" : "info"}">
                        <svg class="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            ${a.nivel === "critica" ? '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>' : a.nivel === "alta" ? '<path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>' : '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>'}
                        </svg>
                    </div>
                    <div class="alerta-content">
                        <h4 class="alerta-title">${a.titulo}</h4>
                        <p class="alerta-time">Hace ${a.tiempo}</p>
                    </div>
                </div>
            `).join("");
        }
    }
    // Update notification badges
    const count = Array.isArray(data) ? data.length : 0;
    if (el.alertNotif) el.alertNotif.textContent = String(count);
    if (el.alertCount) el.alertCount.textContent = String(count);
}

// ---------- 7. Tareas (cards dinámicas) ----------
/** @param {Array<{titulo: string, estado?: string, icon?: string, cultivo?: string, lote?: string}>} data */
function renderTareas(data) {
    // Original container
    if (el.tareasCont) {
        let cardsDiv = el.tareasCont.querySelector(".cards");
        if (!cardsDiv) {
            cardsDiv = document.createElement("div");
            cardsDiv.className = "cards";
            el.tareasCont.appendChild(cardsDiv);
        }
        if (!Array.isArray(data) || data.length === 0) {
            cardsDiv.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay tareas pendientes</p>";
        } else {
            cardsDiv.innerHTML = data.map(t => `
                <div class="card">
                    <img src="${t.icon || '/images/svg-hackaton/task-checklist 2.svg'}" alt="tarea">
                    <div class="join">
                        <h4>${t.titulo}</h4>
                        <p>${t.estado || ''}</p>
                    </div>
                </div>
            `).join("");
        }
    }
    // New container
    if (el.tareasContainer) {
        if (!Array.isArray(data) || data.length === 0) {
            el.tareasContainer.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay tareas pendientes</p>";
        } else {
            el.tareasContainer.innerHTML = data.slice(0, 5).map(t => `
                <div class="tarea-card">
                    <div class="tarea-icon">
                        <svg class="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M9 11l3 3L22 4"/>
                            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
                        </svg>
                    </div>
                    <div class="tarea-content">
                        <h4 class="tarea-title">${t.titulo}</h4>
                        <p class="tarea-meta">
                            ${t.cultivo ? `<span>${t.cultivo}</span>` : ""}
                            ${t.lote ? `<span>${t.lote}</span>` : ""}
                            ${t.estado ? `<span>${t.estado}</span>` : ""}
                        </p>
                    </div>
                </div>
            `).join("");
        }
    }
}

function renderUmbrales(umbrales) {
    const container = document.getElementById("umbralesContainer") || document.querySelector(".umbrales-list");
    if (!container) return;

    if (!umbrales || umbrales.length === 0) {
        container.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay umbrales configurados</p>";
        return;
    }

    const recientes = umbrales.slice(0, 3);
    container.innerHTML = recientes.map(u => `
        <div class="umbral-card" data-id="${u.Id}">
            <div class="umbral-header">
                <span class="umbral-sensor">${u.SensorTipo}</span>
                <span class="umbral-finca">${u.FincaNombre || "Todas las fincas"}</span>
                <span class="umbral-status ${u.Activo ? "activo" : "inactivo"}">${u.Activo ? "Activo" : "Inactivo"}</span>
            </div>
            <div class="umbral-details">
                ${u.Minimo !== null && u.Minimo !== undefined ? `<span>Mín: ${u.Minimo} (${u.SeveridadMinima})</span>` : ""}
                ${u.Maximo !== null && u.Maximo !== undefined ? `<span>Máx: ${u.Maximo} (${u.SeveridadMaxima})</span>` : ""}
                ${u.AccionTipo ? `<span class="umbral-accion">Acción: ${u.AccionTipo}</span>` : ""}
            </div>
        </div>
    `).join("");
}

// ============================================================
// Puente con C# (WebView2)
// Desde C#: webView.CoreWebView2.PostWebMessageAsJson(jsonString)
// jsonString con forma: { type: "ambiente" | "stats" | "resumen" | "plagas" | "alertas" | "tareas" | "finca", payload: {...} }
// ============================================================
const win = /** @type {any} */ (window);
if (win.chrome && win.chrome.webview) {
    win.chrome.webview.addEventListener("message", (/** @type {MessageEvent} */ event) => {
        const { type, payload } = event.data;

        switch (type) {
            case "ambiente":     updateAmbiente(payload); break;
            case "stats":        updateStats(payload); break;
            case "resumen":      updateResumenSemanal(payload); break;
            case "plagas":       updateEstadoCultivo(payload); break;
            case "alertas":      renderAlertas(payload); break;
            case "tareas":       renderTareas(payload); break;
            case "finca":        updateFinca(payload); break;
            case "umbrales":     renderUmbrales(payload.umbrales || []); break;
            default:
                console.warn("Tipo de mensaje no reconocido:", type);
        }
    });

    // Avisamos a C# que el WebView ya cargó y puede empezar a mandar data
    win.chrome.webview.postMessage({ type: "ready_dashboard" });
}