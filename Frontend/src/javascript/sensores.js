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

/**
 * Atajo para buscar elementos del HTML por su id.
 * Devuelve "any" para que TypeScript no se queje de los tipos.
 * @param {string} id
 * @returns {any}
 */
function obtenerElemento(id) {
    return document.getElementById(id);
}

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
    /** @type {HTMLElement | null} */
    const statusButton = document.getElementById("status-button");
    if (!statusButton) return;

    /** @type {HTMLElement | null} */
    const chartTitle = document.getElementById("chart-title");
    /** @type {HTMLElement | null} */
    const chartValDisplay = document.getElementById("chart-val-display");
    /** @type {HTMLElement | null} */
    const chartUnitDisplay = document.getElementById("chart-unit-display");

    /** @type {NodeListOf<HTMLElement>} */
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

    const canvasElement = document.getElementById("historyChart");
    if (!(canvasElement instanceof HTMLCanvasElement)) return;
    /** @type {CanvasRenderingContext2D | null} */
    const ctx = canvasElement.getContext("2d");
    if (!ctx) return;

    /** @type {keyof typeof datasetHistorico} */
    let activeMetric = "temperatura";

    // @ts-ignore
    let historyChart = new Chart(ctx, {
        type: "line",
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
                    grid: { color: "#f0f0f0" },
                    ticks: { color: "#bfc6c4", font: { size: 11 } }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: "#bfc6c4", font: { size: 11 } }
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

            /** @type {HTMLElement | null} */
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

        /** @type {HTMLElement | null} */
        const el = document.getElementById(cfg.uiId);
        if (el) el.textContent = num.toFixed(cfg.dec);

        if (cfg.metrica && cfg.metrica in datasetHistorico) {
            const serie = datasetHistorico[/** @type {keyof typeof datasetHistorico} */ (cfg.metrica)].valores;
            serie[labelsHorarios.length - 1] = num;
            if (cfg.metrica === activeMetric) {
                if (chartValDisplay) chartValDisplay.textContent = num.toFixed(cfg.dec);
                historyChart.update("none");
            }
        }
    };

    // Solo se simula lo que NO tiene hardware (viento)
    setInterval(() => {
        if (statusButton.classList.contains("online")) {
            aplicarLectura("viento", Math.floor(10 + Math.random() * 4));
        }
    }, SIMULACION_VIENTO_MS);

    // Modal Umbral - abrir desde los dos botones "Nuevo Umbral"
    ["btnNuevoUmbral", "btnNuevoUmbral2"].forEach(id => {
        const btn = obtenerElemento(id);
        if (btn) btn.addEventListener("click", () => abrirModalUmbral());
    });

    // Modal Umbral - cerrar con la X y con Cancelar
    ["modalUmbralClose", "btnCancelarUmbral"].forEach(id => {
        const btn = obtenerElemento(id);
        if (btn) btn.addEventListener("click", cerrarModalUmbral);
    });

    // Modal Umbral - guardar (submit del formulario)
    const formUmbral = obtenerElemento("formUmbral");
    if (formUmbral) {
        formUmbral.addEventListener("submit", (/** @type {Event} */ e) => {
            e.preventDefault();
            enviarUmbral();
        });
    }

    // Al elegir un tipo de acción, mostrar u ocultar el campo de configuración JSON
    const accionTipoSelect = obtenerElemento("umbralAccionTipo");
    if (accionTipoSelect) {
        accionTipoSelect.addEventListener("change", () => {
            if (accionTipoSelect.value) mostrarGrupo("grupoAccionConfig");
            else ocultarGrupo("grupoAccionConfig");
        });
    }

    // Modal Eliminar - cerrar con la X y con Cancelar
    ["modalEliminarClose", "btnCancelarEliminar"].forEach(id => {
        const btn = obtenerElemento(id);
        if (btn) btn.addEventListener("click", cerrarModalEliminar);
    });

    // Modal Eliminar - confirmar (aquí SÍ se manda a borrar)
    const btnConfirmarEliminar = obtenerElemento("btnConfirmarEliminar");
    if (btnConfirmarEliminar) {
        btnConfirmarEliminar.addEventListener("click", () => {
            const modal = obtenerElemento("modalEliminar");
            if (modal && modal.dataset.umbralId) {
                eliminarUmbral(parseInt(modal.dataset.umbralId, 10));
            }
        });
    }

    // Cerrar los modales al hacer click en el fondo oscuro
    document.querySelectorAll(".modal-overlay").forEach(overlay => {
        overlay.addEventListener("click", (e) => {
            if (e.target === overlay) overlay.classList.remove("active");
        });
    });
});

// ===== LÓGICA DE MODAL UMBRALES =====
/** @type {Array<{typeId: string, displayName: string, fields: Array<any>}>} */
let accionesDisponiblesCache = [];

/** Guarda el id del umbral que el usuario quiere editar (reemplaza a window._umbralAEditar) */
/** @type {number | null} */
let umbralAEditar = null;

/** @param {Array<{typeId: string, displayName: string, fields: Array<any>}>} acciones */
function cacheAccionesDisponibles(acciones) {
    accionesDisponiblesCache = acciones || [];
}

function solicitarUmbrales() {
    /** @type {any} */
    const win = window;
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({ screen: "sensores", type: "obtenerUmbrales" });
    }
}

function solicitarAccionesDisponibles() {
    /** @type {any} */
    const win = window;
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({ screen: "sensores", type: "obtenerAccionesDisponibles" });
    }
}

/**
 * @param {any | null} umbral
 */
function abrirModalUmbral(umbral = null) {
    const modal = obtenerElemento("modalUmbral");
    const form = obtenerElemento("formUmbral");
    const titulo = obtenerElemento("modalUmbralTitulo");
    const btnText = obtenerElemento("btnUmbralGuardarText");

    if (!modal || !form) return;

    form.reset();
    const cooldownInput = obtenerElemento("umbralCooldown");
    if (cooldownInput) cooldownInput.value = "30";
    const genTareaCheck = obtenerElemento("umbralGenerarTarea");
    if (genTareaCheck) genTareaCheck.checked = true;
    ocultarGrupo("grupoAccionConfig");
    ocultarGrupo("grupoInsumo");
    ocultarGrupo("grupoCantidadCosto");

    if (umbral) {
        if (titulo) titulo.textContent = "Editar Umbral";
        if (btnText) btnText.textContent = "Actualizar Umbral";
        form.dataset.umbralId = String(umbral.Id);

        const sensorTipoSel = obtenerElemento("umbralSensorTipo");
        if (sensorTipoSel) sensorTipoSel.value = umbral.SensorTipo || "";
        const fincaSel = obtenerElemento("umbralFinca");
        if (fincaSel) fincaSel.value = umbral.FincaId != null ? String(umbral.FincaId) : "";
        const minInput = obtenerElemento("umbralMinimo");
        if (minInput) minInput.value = umbral.Minimo != null ? String(umbral.Minimo) : "";
        const maxInput = obtenerElemento("umbralMaximo");
        if (maxInput) maxInput.value = umbral.Maximo != null ? String(umbral.Maximo) : "";
        const sevMinSel = obtenerElemento("umbralSeveridadMin");
        if (sevMinSel) sevMinSel.value = umbral.SeveridadMinima || "media";
        const sevMaxSel = obtenerElemento("umbralSeveridadMax");
        if (sevMaxSel) sevMaxSel.value = umbral.SeveridadMaxima || "media";
        const accionText = obtenerElemento("umbralAccion");
        if (accionText) accionText.value = umbral.AccionSugerida || "";
        const genTarea = obtenerElemento("umbralGenerarTarea");
        if (genTarea) genTarea.checked = umbral.GenerarTareaAuto ?? true;
        const accionTipoSel = obtenerElemento("umbralAccionTipo");
        if (accionTipoSel) accionTipoSel.value = umbral.AccionTipo || "";
        const accionConfigText = obtenerElemento("umbralAccionConfig");
        if (accionConfigText) accionConfigText.value = umbral.AccionConfigJson || "";
        const cooldownInput2 = obtenerElemento("umbralCooldown");
        if (cooldownInput2) cooldownInput2.value = String(umbral.CooldownMinutos ?? 30);

        if (umbral.AccionTipo) {
            mostrarGrupo("grupoAccionConfig");
        }
        if (umbral.GenerarTareaAuto && umbral.InsumoSugeridoId) {
            mostrarGrupo("grupoInsumo");
            const insumoSel = obtenerElemento("umbralInsumo");
            if (insumoSel) insumoSel.value = String(umbral.InsumoSugeridoId);
            if (umbral.CantidadInsumoSugerida != null || umbral.CostoUnitarioSugerido != null) {
                mostrarGrupo("grupoCantidadCosto");
                const cantInput = obtenerElemento("umbralCantidad");
                if (cantInput) cantInput.value = umbral.CantidadInsumoSugerida != null ? String(umbral.CantidadInsumoSugerida) : "1";
                const costoInput = obtenerElemento("umbralCosto");
                if (costoInput) costoInput.value = umbral.CostoUnitarioSugerido != null ? String(umbral.CostoUnitarioSugerido) : "0";
            }
        }
    } else {
        if (titulo) titulo.textContent = "Nuevo Umbral";
        if (btnText) btnText.textContent = "Crear Umbral";
        delete form.dataset.umbralId;
    }

    modal.classList.add("active");
    solicitarAccionesDisponibles();
}

function cerrarModalUmbral() {
    const modal = obtenerElemento("modalUmbral");
    const form = obtenerElemento("formUmbral");
    if (modal) modal.classList.remove("active");
    if (form) {
        form.reset();
        delete form.dataset.umbralId;
    }
    limpiarErroresFormulario();
}

/** @param {string} id */
function mostrarGrupo(id) {
    const el = obtenerElemento(id);
    if (el) el.style.display = "block";
}

/** @param {string} id */
function ocultarGrupo(id) {
    const el = obtenerElemento(id);
    if (el) el.style.display = "none";
}

function limpiarErroresFormulario() {
    /** @type {NodeListOf<HTMLElement>} */
    const errors = document.querySelectorAll("#formUmbral .error-message");
    errors.forEach(el => {
        el.textContent = "";
    });
    /** @type {NodeListOf<HTMLElement>} */
    const inputs = document.querySelectorAll("#formUmbral input, #formUmbral select, #formUmbral textarea");
    inputs.forEach(el => {
        el.style.borderColor = "";
    });
}

/** @param {string} mensaje */
function mostrarErrorUmbral(mensaje) {
    alert("Error: " + mensaje); // Temporal - mejorar con toast
}

function validarFormularioUmbral() {
    limpiarErroresFormulario();
    let valido = true;

    const sensorTipo = obtenerElemento("umbralSensorTipo");
    if (sensorTipo && !sensorTipo.value) {
        const err = obtenerElemento("errUmbralSensorTipo");
        if (err) err.textContent = "Selecciona un tipo de sensor";
        sensorTipo.style.borderColor = "#DC2626";
        valido = false;
    }

    const minimo = obtenerElemento("umbralMinimo");
    const maximo = obtenerElemento("umbralMaximo");
    if (minimo && maximo && !minimo.value && !maximo.value) {
        const errMin = obtenerElemento("errUmbralMinimo");
        if (errMin) errMin.textContent = "Debe configurar al menos un umbral (mínimo o máximo)";
        const errMax = obtenerElemento("errUmbralMaximo");
        if (errMax) errMax.textContent = "Debe configurar al menos un umbral (mínimo o máximo)";
        minimo.style.borderColor = "#DC2626";
        maximo.style.borderColor = "#DC2626";
        valido = false;
    }

    const accionTipo = obtenerElemento("umbralAccionTipo");
    const accionConfig = obtenerElemento("umbralAccionConfig");
    if (accionTipo && accionConfig && accionTipo.value && accionConfig.value.trim()) {
        try {
            JSON.parse(accionConfig.value);
        } catch {
            const err = obtenerElemento("errUmbralAccionConfig");
            if (err) err.textContent = "JSON inválido";
            accionConfig.style.borderColor = "#DC2626";
            valido = false;
        }
    }

    const cooldown = obtenerElemento("umbralCooldown");
    if (cooldown && cooldown.value !== "") {
        const cooldownVal = parseInt(cooldown.value, 10);
        if (isNaN(cooldownVal) || cooldownVal < 0) {
            const err = obtenerElemento("errUmbralCooldown");
            if (err) err.textContent = "El cooldown debe ser ≥ 0";
            cooldown.style.borderColor = "#DC2626";
            valido = false;
        }
    }

    return valido;
}

/** @returns {Record<string, any>} */
function obtenerDatosFormularioUmbral() {
    const fincaSel = obtenerElemento("umbralFinca");
    const minimoInput = obtenerElemento("umbralMinimo");
    const maximoInput = obtenerElemento("umbralMaximo");
    const insumoSel = obtenerElemento("umbralInsumo");
    const cantInput = obtenerElemento("umbralCantidad");
    const costoInput = obtenerElemento("umbralCosto");
    const cooldownInput = obtenerElemento("umbralCooldown");

    return {
        SensorTipo: obtenerElemento("umbralSensorTipo").value,
        FincaId: fincaSel && fincaSel.value ? parseInt(fincaSel.value, 10) : null,
        FincaNombre: fincaSel && fincaSel.selectedOptions[0] ? fincaSel.selectedOptions[0].text : "",
        Minimo: minimoInput && minimoInput.value ? parseFloat(minimoInput.value) : null,
        Maximo: maximoInput && maximoInput.value ? parseFloat(maximoInput.value) : null,
        SeveridadMinima: obtenerElemento("umbralSeveridadMin").value,
        SeveridadMaxima: obtenerElemento("umbralSeveridadMax").value,
        GenerarTareaAuto: obtenerElemento("umbralGenerarTarea").checked,
        AccionSugerida: obtenerElemento("umbralAccion").value,
        InsumoSugeridoId: insumoSel && insumoSel.value ? parseInt(insumoSel.value, 10) : null,
        CantidadInsumoSugerida: cantInput && cantInput.value ? parseFloat(cantInput.value) : null,
        CostoUnitarioSugerido: costoInput && costoInput.value ? parseFloat(costoInput.value) : null,
        Activo: true,
        AccionTipo: obtenerElemento("umbralAccionTipo").value || null,
        AccionConfigJson: obtenerElemento("umbralAccionConfig").value.trim() || null,
        CooldownMinutos: cooldownInput && cooldownInput.value ? parseInt(cooldownInput.value, 10) : 30
    };
}

function enviarUmbral() {
    if (!validarFormularioUmbral()) return;

    const form = obtenerElemento("formUmbral");
    if (!form) return;
    const esEdicion = form.dataset.umbralId;
    const datos = obtenerDatosFormularioUmbral();
    const tipo = esEdicion ? "actualizarUmbral" : "crearUmbral";

    if (esEdicion) {
        datos.Id = parseInt(esEdicion, 10);
    }

    /** @type {any} */
    const win = window;
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({ screen: "sensores", type: tipo, payload: datos });
    }
}

/** @param {Array<{Id: number, SensorTipo: string, FincaNombre: string | null, Minimo: number | null, Maximo: number | null, SeveridadMinima: string, SeveridadMaxima: string, AccionTipo: string | null, CooldownMinutos: number, Activo: boolean}>} umbrales */
function renderizarUmbrales(umbrales) {
    const container = obtenerElemento("umbralesContainer");
    if (!container) return;

    if (!umbrales || umbrales.length === 0) {
        container.innerHTML = "<p style='color: #666; text-align: center; padding: 20px;'>No hay umbrales configurados</p>";
        return;
    }

    container.innerHTML = umbrales.map(u => `
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
                ${u.CooldownMinutos ? `<span class="umbral-cooldown">Cooldown: ${u.CooldownMinutos} min</span>` : ""}
            </div>
            <div class="umbral-actions">
                <button class="btn-sm btn-editar" data-id="${u.Id}" data-action="editar">Editar</button>
                <button class="btn-sm btn-eliminar" data-id="${u.Id}" data-action="eliminar">Eliminar</button>
            </div>
        </div>
    `).join("");

    /** @type {NodeListOf<HTMLButtonElement>} */
    const btnEditar = container.querySelectorAll(".btn-editar");
    btnEditar.forEach(btn => {
        btn.addEventListener("click", () => editarUmbral(parseInt(btn.dataset.id || "0", 10)));
    });
    /** @type {NodeListOf<HTMLButtonElement>} */
    const btnEliminar = container.querySelectorAll(".btn-eliminar");
    btnEliminar.forEach(btn => {
        btn.addEventListener("click", () => confirmarEliminarUmbral(parseInt(btn.dataset.id || "0", 10)));
    });
}

/** @param {number} id */
function editarUmbral(id) {
    /** @type {any} */
    const win = window;
    if (win.chrome && win.chrome.webview) {
        // Solicitar umbrales y luego buscar el que coincide
        win.chrome.webview.postMessage({ screen: "sensores", type: "obtenerUmbrales" });
        // Guardar el ID para cuando llegue la respuesta
        umbralAEditar = id;
    }
}

/** Paso 1: solo muestra el modal de confirmación @param {number} id */
function confirmarEliminarUmbral(id) {
    const modal = obtenerElemento("modalEliminar");
    const nombreEl = obtenerElemento("eliminarUmbralNombre");
    if (modal && nombreEl) {
        modal.dataset.umbralId = String(id);
        nombreEl.textContent = `ID ${id}`;
        modal.classList.add("active");
    }
}

/** Paso 2: el usuario confirmó, ahora sí se envía a C# @param {number} id */
function eliminarUmbral(id) {
    /** @type {any} */
    const win = window;
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({ screen: "sensores", type: "eliminarUmbral", payload: { Id: id } });
    }
    cerrarModalEliminar();
}

function cerrarModalEliminar() {
    const modal = obtenerElemento("modalEliminar");
    if (modal) {
        modal.classList.remove("active");
        delete modal.dataset.umbralId;
    }
}

// Puente con C# (WebView2) envuelto en un bloque local para evitar colisiones con 'win' de otros archivos
{
    /** @type {any} */
    const winSensores = window;
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

                case "estado_conexion": {
                    const statusButton = obtenerElemento("status-button");
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
                }

                case "alerta":
                    mostrarAlerta(payload);
                    break;

                case "alertaResuelta":
                    ocultarAlerta(payload.id);
                    break;

                case "alertasActivas":
                    cargarAlertasActivas(payload.alertas);
                    break;

                case "umbrales": {
                    /** @type {Array<any>} */
                    const umbrales = payload.umbrales || [];
                    renderizarUmbrales(umbrales);
                    // Si hay un umbral pendiente de editar, buscarlo y abrir modal
                    if (umbralAEditar !== null) {
                        const umbral = umbrales.find(u => u.Id === umbralAEditar);
                        if (umbral) {
                            abrirModalUmbral(umbral);
                        }
                        umbralAEditar = null;
                    }
                    break;
                }

                case "umbralCreado":
                case "umbralActualizado":
                    cerrarModalUmbral();
                    if (payload.umbral) {
                        solicitarUmbrales();
                    }
                    break;

                case "umbralEliminado":
                    solicitarUmbrales();
                    break;

                case "accionesDisponibles": {
                    /** @type {Array<{typeId: string, displayName: string, fields: Array<any>}>} */
                    const acciones = payload.acciones || [];
                    cacheAccionesDisponibles(acciones);
                    // Poblar el select de tipo de acción
                    poblarSelectAcciones(acciones);
                    break;
                }

                case "umbralError":
                    mostrarErrorUmbral(payload.mensaje);
                    break;

                default:
                    console.warn("Tipo de mensaje no reconocido en Sensores:", type);
            }
        });

        // Solicitar alertas activas al iniciar
        winSensores.chrome.webview.postMessage({ screen: "sensores", type: "obtenerAlertasActivas" });

        winSensores.chrome.webview.postMessage({ screen: "sensores", type: "ready_sensores" });

        // Cargar la lista de umbrales al abrir la pantalla
        solicitarUmbrales();
    }
}

/** @param {Array<{typeId: string, displayName: string, fields: Array<any>}>} acciones */
function poblarSelectAcciones(acciones) {
    const select = obtenerElemento("umbralAccionTipo");
    if (!select) return;

    // Guardar la opción "Ninguna"
    const primeraOpcion = select.querySelector('option[value=""]');
    select.innerHTML = "";
    if (primeraOpcion) select.appendChild(primeraOpcion);

    (acciones || []).forEach(accion => {
        /** @type {HTMLOptionElement} */
        const option = document.createElement("option");
        option.value = accion.typeId;
        option.textContent = accion.displayName;
        select.appendChild(option);
    });
}