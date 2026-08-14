document.addEventListener("DOMContentLoaded", () => {
    //aqui te puse eso para que se actualice el side bar bro ya con eso ya lo arregle era solo llamarla xd
    updateSidebar();
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

    // --- Validación al enviar ---
    formNuevoRegistro?.addEventListener("submit", (e) => {
        const tipoSeleccionado = /** @type {HTMLInputElement | null} */ (
            formNuevoRegistro.querySelector('input[name="tipoRegistro"]:checked')
        )?.value;
        const cultivoSeleccionado = /** @type {HTMLInputElement | null} */ (
            formNuevoRegistro.querySelector('input[name="cultivo"]:checked')
        )?.value;

        const monto = parseFloat(regMonto.value);

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
            cultivo: cultivoSeleccionado,
            categoria: tipoSeleccionado === "costo" ? regCategoria.value : null,
            monto,
            fecha: regFecha.value,
            descripcion: regDescripcion.value
        };

        // aca ira el codigo para pasar los datos al c#
        // window.chrome.webview.postMessage({ type: "nuevoRegistroFinanciero", payload: nuevoRegistro });

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