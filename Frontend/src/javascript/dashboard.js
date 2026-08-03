//validador del sidebar
if(typeof updateSidebar === 'function') updateSidebar();
// ============================================================
// dashboard.js - AgroEco
// Diseñado para WPF + WebView2. C# manda datos via postMessage
// y este script solo pinta el DOM. No hay lógica de negocio acá.
// ============================================================

// ---------- Referencias base (cacheadas, no querySelector cada vez) ----------
const el = {
    temp: document.getElementById("Temperatura"),
    notaTemp: document.getElementById("nota-temperatura"),
    humedad: document.getElementById("Humedad"),
    notaHumedad: document.getElementById("nota-humedad"),
    fincaName: document.getElementById("id-finca"),
    resumenDias: document.getElementById("resumen-container") || document.querySelector(".boxesC"),
    plagasMsg: document.getElementById("dashReportPlagas"),
    boxesD: document.querySelector(".boxesD"),
    alertasCont: document.querySelector(".alertas .cards"),
    alertNotif: document.getElementById("alertNotification"),
    tareasCont: document.querySelector(".tareas"), // le agregamos un div interno si no existe
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
    if (temperatura !== undefined && el.temp) el.temp.textContent = `${temperatura}°C`;
    if (notaTemperatura !== undefined && el.notaTemp) el.notaTemp.textContent = notaTemperatura;
    if (humedad !== undefined && el.humedad) el.humedad.textContent = `${humedad}%`;
    if (notaHumedad !== undefined && el.notaHumedad) el.notaHumedad.textContent = notaHumedad; 
}

// ---------- 2. Nombre de finca ----------
/** @param {string} nombre */
function updateFinca(nombre) {
    if (el.fincaName) el.fincaName.textContent = `Finca : ${nombre}`;
}

// ---------- 3. Boxes superiores (sensores/alertas/tareas activas/completadas) ----------
/**
 * @param {Object} data
 * @param {string|number} [data.sensoresActivos]
 * @param {string|number} [data.alertas]
 * @param {string|number} [data.tareasActivas]
 * @param {string|number} [data.tareasCompletadas]
 */
function updateStats({ sensoresActivos, alertas, tareasActivas, tareasCompletadas }) {
    if (!el.boxesD) return;
    const nums = el.boxesD.querySelectorAll(".num");
    // Orden fijo del HTML: sensores, alertas, tareas activas, tareas completadas
    if (sensoresActivos !== undefined && nums[0]) nums[0].textContent = String(sensoresActivos);
    if (alertas !== undefined && nums[1]) nums[1].textContent = String(alertas);
    if (tareasActivas !== undefined && nums[2]) nums[2].textContent = String(tareasActivas);
    if (tareasCompletadas !== undefined && nums[3]) nums[3].textContent = String(tareasCompletadas);
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
 */
function updateEstadoCultivo({ hayPlagas, mensaje }) {
    if (!el.plagasMsg) return;
    el.plagasMsg.textContent = mensaje || (hayPlagas ? "Plagas detectadas" : "Sin plagas reportadas");
    /** @type {HTMLElement} */ (el.plagasMsg).style.color = hayPlagas ? "#BC6661" : "";
}

// ---------- 6. Alertas (cards dinámicas) ----------
// data = [{ titulo:"Alerta 1", tiempo:"5 min", icon:"/images/svg-hackaton/alerta1.svg" }, ...]
/** @param {Array<{titulo: string, tiempo: string, icon?: string}>} data */
function renderAlertas(data) {
    if (!Array.isArray(data) || !el.alertasCont) return;

    el.alertasCont.innerHTML = data.map(a => `
        <div class="card">
            <img src="${a.icon || '/images/svg-hackaton/alerta1.svg'}" alt="alerta">
            <div class="join">
                <h4>${a.titulo}</h4>
                <p>Hace : <span>${a.tiempo}</span></p>
            </div>
        </div>
    `).join("");

    if (el.alertNotif) el.alertNotif.textContent = String(data.length);
}

// ---------- 7. Tareas (cards dinámicas) ----------
// data = [{ titulo:"Regar sector A", estado:"Pendiente", icon:"/images/svg-hackaton/task-checklist 2.svg" }, ...]
/** @param {Array<{titulo: string, estado?: string, icon?: string}>} data */
function renderTareas(data) {
    if (!Array.isArray(data) || !el.tareasCont) return;

    // Si el contenedor de cards de tareas no existe todavía, lo creamos una vez
    let cardsDiv = el.tareasCont.querySelector(".cards");
    if (!cardsDiv) {
        cardsDiv = document.createElement("div");
        cardsDiv.className = "cards";
        el.tareasCont.appendChild(cardsDiv);
    }

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
            default:
                console.warn("Tipo de mensaje no reconocido:", type);
        }
    });

    // Avisamos a C# que el WebView ya cargó y puede empezar a mandar data
    win.chrome.webview.postMessage({ type: "ready" });
}