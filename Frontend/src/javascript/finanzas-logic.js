document.addEventListener("DOMContentLoaded", () => {
    //aqui te puse eso para que se actualice el side bar bro ya con eso ya lo arregle era solo llamarla xd
    updateSidebar();

    // Atajo para hablar con C# (WebView2)
    const winObj = /** @type {any} */ (window);

    // =====================================================
    //  UTILIDADES
    // =====================================================

    // Convierte 1000 en "C$ 1,000.00". Si llega algo raro, muestra C$ 0.00
    /** @param {any} n */
    function formatearMonto(n) {
        const valor = Number(n);
        const seguro = Number.isFinite(valor) ? valor : 0;
        return "C$ " + seguro.toLocaleString("es-NI", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // Cambia el texto de un elemento por su id (si el elemento no existe, no hace nada)
    /**
     * @param {string} id
     * @param {string} texto
     */
    function pintar(id, texto) {
        const el = document.getElementById(id);
        if (el) el.textContent = texto;
    }

    // Manda un mensaje a C# (si la página se abre fuera de la app, no falla)
    /**
     * @param {string} type
     * @param {any} [payload]
     */
    function enviarACSharp(type, payload) {
        if (!winObj.chrome || !winObj.chrome.webview) {
            console.warn(`[Finanzas] WebView2 no disponible. Se intentó enviar: ${type}`, payload);
            return;
        }
        winObj.chrome.webview.postMessage({ screen: "finanzas", type, payload });
    }

    // =====================================================
    //  RECIBIR DATOS DE C#
    // =====================================================

    if (winObj.chrome && winObj.chrome.webview) {
        // 1) primero ESCUCHAR
        winObj.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const data = event.data;
            if (!data) return;

            const { type, payload } = data;

            switch (type) {
                case "finanzas_cargadas":
                    if (!payload) return;
                    pintar("ingresos", formatearMonto(payload.ingresos));
                    pintar("costos", formatearMonto(payload.costos));
                    pintar("ganancias", formatearMonto(payload.ganancias));
                    pintar("ganancia-neta", (Number(payload.gananciaNeta) || 0) + "%");
                    break;

                case "cargar_registros":
                    renderizarMovimientos(payload);
                    break;

                default:
                    // mensajes de otras pantallas: se ignoran
                    break;
            }
        });

        // 2) luego PEDIR los datos
        enviarACSharp("ready_finanzas");
    }

    // =====================================================
    //  MODAL Y FORMULARIO
    // =====================================================

    // --- Referencias al modal ---
    const btnOpenModal = document.querySelector("#addInfo");
    const modal = /** @type {HTMLDialogElement} */ (document.getElementById("showDialogNewReg"));
    const btnCancelReg = document.getElementById("btnCancelReg");

    // --- Referencias al form ---
    const formNuevoRegistro = /** @type {HTMLFormElement} */ (document.getElementById("formNuevoRegistro"));
    const regMonto = /** @type {HTMLInputElement} */ (document.getElementById("regMonto"));
    const regFecha = /** @type {HTMLInputElement} */ (document.getElementById("regFecha"));
    const regCategoria = /** @type {HTMLSelectElement} */ (document.getElementById("regCategoria"));
    const regDescripcion = /** @type {HTMLInputElement} */ (document.getElementById("regDescripcion"));

    // --- Categorías disponibles solo si el tipo es Costo ---
    const CATEGORIAS_COSTO = [
        { value: "fertilizantes", label: "Fertilizantes" },
        { value: "pesticidas", label: "Pesticidas / Plaguicidas" },
        { value: "mano_obra", label: "Mano de obra" },
        { value: "equipo", label: "Equipo y herramientas" },
        { value: "transporte", label: "Transporte" },
        { value: "combustible", label: "Combustible" },
        { value: "otros", label: "Otros" }
    ];

    /** @param {string} tipo */
    function actualizarCategoria(tipo) {
        if (tipo === "costo") {
            regCategoria.disabled = false;
            regCategoria.innerHTML =
                `<option value="" disabled selected>Selecciona una categoría</option>` +
                CATEGORIAS_COSTO.map(c => `<option value="${c.value}">${c.label}</option>`).join("");
        } else {
            regCategoria.disabled = true;
            regCategoria.innerHTML = `<option value="" disabled selected>Tipo insumo disponible solo si es costo</option>`;
        }
    }

    // --- Abrir modal ---
    btnOpenModal?.addEventListener("click", () => {
        modal?.showModal();
    });

    // --- Cancelar (cierra sin guardar) ---
    btnCancelReg?.addEventListener("click", () => {
        formNuevoRegistro?.reset();
        actualizarCategoria("ingreso");
        modal?.close();
    });

    // --- Validación al enviar + envío a C# ---
    formNuevoRegistro?.addEventListener("submit", (e) => {
        const tipoSeleccionado = /** @type {HTMLInputElement | null} */ (
            formNuevoRegistro.querySelector('input[name="tipoRegistro"]:checked')
        )?.value;
        const cultivoSeleccionado = /** @type {HTMLInputElement | null} */ (
            formNuevoRegistro.querySelector('input[name="cultivo"]:checked')
        )?.value;

        const monto = parseFloat(regMonto.value);

        if (!tipoSeleccionado) {
            e.preventDefault();
            return;
        }

        if (isNaN(monto) || monto <= 0) {
            e.preventDefault();
            regMonto.focus();
            return;
        }

        if (!regFecha.value) {
            e.preventDefault();
            regFecha.focus();
            return;
        }

        if (tipoSeleccionado === "costo" && !regCategoria.value) {
            e.preventDefault();
            regCategoria.focus();
            return;
        }

        const nuevoRegistro = {
            tipo: tipoSeleccionado,
            cultivo: cultivoSeleccionado ?? "general",
            categoria: tipoSeleccionado === "costo" ? regCategoria.value : null,
            monto,
            fecha: regFecha.value,
            descripcion: regDescripcion.value
        };

        // pasar el registro a C# (antes del reset, que borra los campos)
        enviarACSharp("nuevoRegistroFinanciero", nuevoRegistro);

        formNuevoRegistro.reset();
        actualizarCategoria("ingreso");
    });

    // --- Toggle visual para tipo (Ingreso/Costo) ---
    const tipoInputs = document.querySelectorAll('input[name="tipoRegistro"]');
    tipoInputs.forEach(input => {
        input.addEventListener("change", () => {
            document.querySelectorAll(".type-btn").forEach(btn => btn.classList.remove("active"));
            input.closest(".type-btn")?.classList.add("active");
            actualizarCategoria(/** @type {HTMLInputElement} */ (input).value);
        });
    });

    // --- Toggle visual para cultivo (Café/Aguacate/General) ---
    const cultivoInputs = document.querySelectorAll('input[name="cultivo"]');
    cultivoInputs.forEach(input => {
        input.addEventListener("change", () => {
            document.querySelectorAll(".segment").forEach(seg => seg.classList.remove("active"));
            input.closest(".segment")?.classList.add("active");
        });
    });

    // --- Estado inicial de categoría al cargar ---
    actualizarCategoria("ingreso");
});

// =====================================================
//  RENDERIZAR MOVIMIENTOS (HISTORIAL)
// =====================================================

/**
 * @param {Array<{id: number, descripcion: string, monto: number, fecha: string, tipo: string}>} registros
 */
function renderizarMovimientos(registros) {
    const container = document.getElementById("movimientosContainer");
    if (!container) return;

    if (!registros || registros.length === 0) {
        container.innerHTML = '<p class="sin-movimientos">No hay registros financieros</p>';
        return;
    }

    // Ordenar por fecha descendente (más reciente primero)
    const ordenados = [...registros].sort((a, b) => {
        const fa = a.fecha.split("/").reverse().join("-");
        const fb = b.fecha.split("/").reverse().join("-");
        return fb.localeCompare(fa);
    });

    container.innerHTML = ordenados.map(r => {
        const esIngreso = r.tipo === "ingreso";
        const signo = esIngreso ? "+" : "-";
        const clase = esIngreso ? "ingreso" : "costo";
        const icono = esIngreso
            ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 6l-9.5 9.5-5-5L1 18"/><path d="M17 6h6v6"/></svg>'
            : '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 18l-9.5-9.5-5 5L1 6"/><path d="M17 18h6v-6"/></svg>';

        return `
            <div class="movimiento-item ${clase}" data-id="${r.id}">
                <div class="mov-info">
                    <span class="mov-icon ${clase}">${icono}</span>
                    <div class="mov-detalle">
                        <span class="mov-descripcion">${r.descripcion || "Sin descripcion"}</span>
                        <span class="mov-fecha">${r.fecha}</span>
                    </div>
                </div>
                <span class="mov-monto ${clase}">${signo} C$ ${Number(r.monto).toLocaleString("es-NI", {minimumFractionDigits: 2, maximumFractionDigits: 2})}</span>
            </div>
        `;
    }).join("");
}