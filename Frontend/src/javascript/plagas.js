document.addEventListener("DOMContentLoaded", () => {
    
    updateSidebar();

    /** @type {any[]} */
    let plagas = [];
    let idPlaga = 1;
    const winObj = /** @type {any} */ (window);
    if (winObj.chrome && winObj.chrome.webview) {
        winObj.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const { type, payload } = event.data;

            switch (type) {
                case "cargar_plagas":
                    plagas = payload;
                    cargarPlagas(plagas);
                    break;

                case "cargar_detalles_plaga":
                    cargarPlaga(payload);
                    break;

                case "cargar_cultivos":
                    cargarCultivos(payload);
                    break;

                case "cargar_detalles_cultivo":
                    const card = /** @type {HTMLElement | null} */ (
                        document.querySelector(
                        `.accordion-item[data-id="${idCultivo}"]`
                        )
                    );

                    if (card) {
                        mostrarDetallesCultivo(payload, card);
                    }
                    break;
                default:
                    console.warn("Tipo de mensaje no reconocido en Módulo Plagas:", type);
            }
        });

        winObj.chrome.webview.postMessage({ screen: "plagas", type: "listo_plagas" });
        winObj.chrome.webview.postMessage({ screen: "plagas", type: "obtener_detalle_plaga", payload: 1 });
    }
    
    const MAX_TAREAS = 8;

    const searchInput = document.getElementById("pest-search");
    const resultsContainer = document.getElementById("search-results");
    
    const labelRisk = document.getElementById("pest-risk");
    const labelName = document.getElementById("pest-name");
    const labelScientific = document.getElementById("pest-scientific");
    const labelDesc = document.getElementById("pest-desc");
    const labelFav = document.getElementById("pest-favorece");
    
    
    const tasksContainer = document.getElementById("active-tasks-container");
    const taskCounter = document.getElementById("task-counter");
    const lbSinTareas =  /** @type {HTMLElement} */ (document.getElementById("lbSinTareas"));

    /** @type {number | null} */
    let idCultivo = null;
    
    if (searchInput && resultsContainer) {
        searchInput.addEventListener("input", (e) => {
            const inputTarget = /** @type {HTMLInputElement} */ (e.target);
            if (!inputTarget) return;
            const val = inputTarget.value.toLowerCase().trim();
            resultsContainer.innerHTML = "";
            
            if (val.length === 0) {
                resultsContainer.style.display = "none";
                return;
            }

            const filtradas = plagas.filter(plaga => 
                plaga.nombre.toLowerCase().includes(val) || 
                plaga.cientifico.toLowerCase().includes(val)
            );

            if (filtradas.length > 0) {
                filtradas.forEach(plaga => {
                    const li = document.createElement("li");
                    li.textContent = plaga.nombre;
                    li.addEventListener("click", () => {
                        idPlaga = plaga.id;
winObj.chrome.webview.postMessage({
                        screen: "plagas",
                        type: "obtener_detalle_plaga",
                        payload: idPlaga
                    });
                        if (searchInput instanceof HTMLInputElement) searchInput.value = plaga.nombre;
                        resultsContainer.style.display = "none";
                    });
                    resultsContainer.appendChild(li);
                });
                resultsContainer.style.display = "block";
            } else {
                resultsContainer.style.display = "none";
            }
        });
    }

    /** @param {Array<{ id: number, nombre: string}>} datos */
    function cargarPlagas(datos){

        const contenedorTags = /** @type {HTMLElement | null} */ (
            document.getElementById("quick-tags")
        );
        if (contenedorTags) {
            contenedorTags.innerHTML = "";
            datos.forEach(plaga => {
                const boton = document.createElement("button");
                
                boton.className = "tag-btn";
                boton.textContent = plaga.nombre;

                boton.addEventListener("click", () => {
                    idPlaga = plaga.id;
                    winObj.chrome.webview.postMessage({
                        screen: "plagas",
                        type: "obtener_detalle_plaga",
                        payload: idPlaga
                    });
                });
                
                contenedorTags.appendChild(boton);
            });
        }
    }

    document.addEventListener("click", (e) => {
        if (searchInput && resultsContainer && e.target !== searchInput) {
            resultsContainer.style.display = "none";
        }
    });

    /**
     * @param {any} plaga
     */
    function cargarPlaga(plaga) {
        if (labelRisk) {
            labelRisk.textContent = plaga.riesgo;
            labelRisk.className = ""; 
            if (plaga.riesgo.includes("Alto")) labelRisk.className = "badge-danger";
            else if (plaga.riesgo.includes("Medio")) labelRisk.className = "badge-warning";
            else labelRisk.className = "badge-success";
        }

        if (labelName) labelName.textContent = plaga.nombre;
        if (labelScientific) labelScientific.textContent = plaga.cientifico;
        if (labelDesc) labelDesc.textContent = plaga.desc;
        if (labelFav) labelFav.textContent = plaga.favorece;
    }

    if (tasksContainer) {
        tasksContainer.addEventListener("click", (e) => {
            const clickTarget = /** @type {HTMLElement} */ (e.target);
            if (!clickTarget) return;
            const btn = clickTarget.closest(".btn-complete-task");
            if (!btn) return;

            const item = btn.parentElement;
            if (!item) return;
            item.style.transform = "scale(0.9)";
            item.style.opacity = "0";
            setTimeout(() => {
                item.remove();
                actualizarContador();
            }, 250);
        });
    }
    //funcion para mostrar un mensaje de finalización de tarea
/**
 * 
 * @param {any} mensaje
 */

    function mostrarMensajeFinalizacion(mensaje) {
        const toast = document.createElement("div");
        toast.textContent = mensaje;
        toast.style.cssText= `
        position: fixed;
            bottom: 20px; 
            right: 20px;
            background-color: #2ec4b6;
            color: #fff;
            padding: 12px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.15);
            font-weight: bold;
            z-index: 1000;
            transition: opacity 0.3s ease;

        `;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = "0";
            setTimeout(() => {
                toast.remove();
            }, 300);        
        },2500);
    }
    if (tasksContainer) {
        tasksContainer.addEventListener("click", (e)=> {
            const clickTarget = /** @type {HTMLElement} */ (e.target);
            if (!clickTarget) return;
            const btn = clickTarget.closest(".btn-complete-task");
            if (!btn) return;

            const item = btn.parentElement;
            if (!item) return;

            const tituloTarea = item.querySelector("h4")?.textContent || "Tarea";

            item.style.transform = "scale(0.9)";
            item.style.opacity = "0";

            setTimeout(() => {
                item.remove();
                actualizarContador();
                //y finalmente se muestra el mensaje de finalización de tarea
                mostrarMensajeFinalizacion(`¡${tituloTarea} completada!`);
            }, 250);
        });
    }

    function actualizarContador() {
        const total = tasksContainer ? tasksContainer.querySelectorAll(".task-item").length : 0;
        if (taskCounter) {
            taskCounter.textContent = `${total} Activas`;
        }
        if (total == 0) {
            lbSinTareas.style.display = "block";
        }
    }
    
     /** @param {Array<{ idCultivo: number, nombreCultivo: string}>} datos */
     function cargarCultivos(datos){
    const contenedor = /** @type {HTMLElement} */  (document.getElementById("cultivos-accordion"));
        contenedor.innerHTML = "";
        datos.forEach(cultivo => {
            const card = document.createElement("div");

            card.className = `accordion-item`;
            card.dataset.id = String(cultivo.idCultivo);
            card.innerHTML = `
                        <div class="accordion-header">
                                <span class="bioinsumo-title">${cultivo.nombreCultivo}</span>
                                <img src="/images/svg-hackaton/angle-small-right.svg" alt="Colapsar" class="icon-svg accordion-arrow">
                        </div>
                        <div class="accordion-content">
                        </div>
            `;
            const header = /** @type {HTMLElement} */ (card.querySelector(".accordion-header"));
            header.addEventListener("click", () => {
                const cards = contenedor.querySelectorAll(".accordion-item");

                cards.forEach(otraCard => {
                    if (otraCard !== card) {
                        otraCard.classList.remove("open");
                    }
                });

                card.classList.toggle("open");

                if (card.classList.contains("open")) {
                    idCultivo = cultivo.idCultivo;
                    winObj.chrome.webview.postMessage({
                        screen: "plagas",
                        type: "obtener_detalle_cultivo",
                        payload: {
                            idPlaga: idPlaga,
                            idCultivo: cultivo.idCultivo
                        }
                    });
                }
            });
            contenedor.appendChild(card);

    });
}
/**
 * @param {{
 *     idCultivo: number,
 *     nombreCultivo: string,
 *     comoIdentificar: string,
 *     pasosIdentificacion: string[],
 *     prevencion: string[],
 *     formulaTratamiento: string,
 *     dosisPor20Litros: string,
 *     frecuenciaTratamiento: string
 * }} cultivo
 * @param {HTMLElement} card
 */
function mostrarDetallesCultivo(cultivo, card) {

    const contenedor = /** @type {HTMLElement} */ (
        card.querySelector(".accordion-content")
    );

    const pasos = Array.isArray(cultivo.pasosIdentificacion)
        ? cultivo.pasosIdentificacion
        : [];

    const pasosHTML = pasos
        .filter(paso => paso)
        .map(paso => `<li>${paso}</li>`)
        .join("");

    const prevencion = Array.isArray(cultivo.prevencion)
        ? cultivo.prevencion
        : [];

    const prevencionHTML = prevencion
        .filter(prevencion => prevencion)
        .map(prevencion => `<li>${prevencion}</li>`)
        .join("");

    const identificacionHTML = cultivo.comoIdentificar || pasosHTML ? `
        <div class="pest-identification">
            <h3 class="section-sub-title">
                Cómo identificar la plaga
            </h3>

            ${cultivo.comoIdentificar ? `<p> ${cultivo.comoIdentificar } </p> ` : ""}
        </div>

        ${pasosHTML ? `
        <div class="pest-steps">

            <h4>Pasos de diagnóstico</h4>

            <ul class="steps-list-styled">
                ${pasosHTML}
            </ul>

        </div>
        ` 
        : ""}
    ` : "";

    const tratamientoHTML = cultivo.formulaTratamiento ? `

        ${prevencionHTML ? `
            <div class="pest-steps">

                <h4>Prevención sin químicos</h4>

                <ul class="steps-list-styled">
                    ${prevencionHTML}
                </ul>

            </div>
            `
    : ""}
        <div class="pest-treatment">

            <h4>
                Tratamiento sugerido y formulación (20 Litros)
            </h4>
            <div class= "advertencia">
                <img src="/images/svg-hackaton/Vector (8).svg" alt="advertencia" width="15px" height="15px">
                <p class="lbAdvertencia">
                    Consultar a un técnico agrícola antes de realizar cualquier tratamiento
                </p>
            </div>

            <div class="pest-treatment-steps">

                <p>
                    <strong>Fórmula:</strong>
                    ${cultivo.formulaTratamiento}
                </p>

                 ${cultivo.dosisPor20Litros ? ` 
                    <p>
                        <strong>Dosis por bomba de 20 litros:</strong>
                        ${cultivo.dosisPor20Litros}
                    </p>
                ` : ""}

                ${cultivo.frecuenciaTratamiento ? `
                    <p>
                        <strong>Frecuencia de aplicación:</strong>
                        ${cultivo.frecuenciaTratamiento}
                    </p>
                ` : ""}
            </div>

        </div>
        ` : "";

    const tareaHTML = `

        <div class="task-action-box">

            <button class="btn-start-task">
                <i class="fa-solid fa-circle-play"></i>
                Iniciar Tarea de Mitigación
            </button>

        </div>
       `;

    contenedor.innerHTML = `
        ${identificacionHTML}
        ${tratamientoHTML}
        ${tareaHTML}
    `;

    const btnAction =  /** @type {HTMLButtonElement} */ (contenedor.querySelector(".btn-start-task"));
    if (btnAction) {
        btnAction.addEventListener("click", () => {
            const tareasActuales = tasksContainer ? tasksContainer.querySelectorAll(".task-item").length : 0;
            lbSinTareas.style.display = "none";

            if (tareasActuales >= MAX_TAREAS) {
                const originalText = btnAction.innerHTML;
                btnAction.style.backgroundColor = "#e74c3c";
                btnAction.innerHTML = `<i class="fa-solid fa-triangle-exclamation"></i> Límite (${MAX_TAREAS}) alcanzado`;

                setTimeout(() => {
                    btnAction.style.backgroundColor = "";
                    btnAction.innerHTML = originalText;
                }, 2000);
                return;
            }

            const nombrePlaga = labelName ? labelName.textContent : "Plaga";
            const nombreCultivo = cultivo.nombreCultivo;
            const lote = "Lote General";
            const accionPredeterminada = "Aplicación de Tratamiento";

            const nuevaTarea = document.createElement("div");
            nuevaTarea.className = "task-item";
            nuevaTarea.innerHTML = `
            <div class="task-status-dot pending"></div>
            <div class="task-details">
                <h4>${accionPredeterminada}</h4>
                <p class="task-sub">${nombrePlaga} · ${nombreCultivo} · <strong>${lote}</strong></p>
                <span class="task-date">Iniciada hace unos instantes</span>
            </div>
            <button class="btn-complete-task"><i class="fa-solid fa-check"></i></button>
        `;

            if (tasksContainer) {
                tasksContainer.prepend(nuevaTarea);
                actualizarContador();
            }

            winObj.chrome.webview.postMessage({
                screen: "plagas",
                type: "nueva_tarea_plaga",
                payload: { plaga: nombrePlaga, lote: lote, cultivo: nombreCultivo, accion: accionPredeterminada }
            });

            const originalText = btnAction.innerHTML;
            btnAction.style.backgroundColor = "#3b9d85";
            btnAction.innerHTML = `<i class="fa-solid fa-circle-check"></i> ¡Tarea Asignada!`;
            setTimeout(() => {
                btnAction.style.backgroundColor = "";
                btnAction.innerHTML = originalText;
            }, 1800);
        });
    }
}
});