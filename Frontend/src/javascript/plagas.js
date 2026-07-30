document.addEventListener("DOMContentLoaded", () => {
    
    const DB_PLAGAS = [
        {
            nombre: "Roya del Café",
            cientifico: "Hemileia vastatrix",
            riesgo: "Alto",
            desc: "Hongo que afecta las hojas provocando manchas amarillas y caída prematura.",
            tratamiento: "Aplicación preventiva de fungicidas cúpricos.",
            comoIdentificar: "Busca manchas amarillas o naranjas con un polvillo en el envés de las hojas.",
            pasosIdentificacion: [
                "Revisar el envés de las hojas maduras.",
                "Identificar lesiones cloróticas circulares.",
                "Confirmar la presencia de polvo uredospórico anaranjado."
            ],
            formulaTratamiento: "Fungicida cúprico al 50% PM",
            dosisPor20Litros: "60 gramos",
            frecuenciaTratamiento: "Cada 30 días en época lluviosa."
        },
        {
            nombre: "Broca del Café",
            cientifico: "Hypothenemus hampei",
            riesgo: "Alto",
            desc: "Pequeño escarabajo que perfora los frutos, destruyendo el grano y afectando la calidad.",
            tratamiento: "Uso de trampas con alcohol y recolección de granos infestados.",
            comoIdentificar: "Inspeccionar los granos verdes o maduros en busca de un pequeño orificio redondo en la parte apical.",
            pasosIdentificacion: [
                "Buscar perforaciones circulares precisas en la corona del fruto.",
                "Cortar granos sospechosos para verificar la presencia de la larva.",
                "Monitorear los niveles de infestación por árbol."
            ],
            formulaTratamiento: "Beauveria bassiana (Control biológico)",
            dosisPor20Litros: "50 gramos de esporas viables",
            frecuenciaTratamiento: "Cada 15 a 21 días durante la fructificación."
        },
        {
            nombre: "Pulgón del Café",
            cientifico: "Toxoptera aurantii",
            riesgo: "Medio",
            desc: "Insectos chupadores que se concentran en brotes tiernos y producen mielecilla.",
            tratamiento: "Aplicación de jabón potásico o extracto de Neem.",
            comoIdentificar: "Observar acumulaciones de insectos pequeños de color oscuro en los brotes nuevos y hojas deformadas.",
            pasosIdentificacion: [
                "Revisar los brotes apicales más tiernos.",
                "Inspeccionar la presencia de fumagina (hongo negro) sobre la mielecilla.",
                "Detectar la alta concurrencia de hormigas asociadas."
            ],
            formulaTratamiento: "Jabón potásico líquido",
            dosisPor20Litros: "100 mililitros",
            frecuenciaTratamiento: "Cada 7 a 10 días hasta controlar el brote."
        },
        {
            nombre: "Minador de la Hoja del Café",
            cientifico: "Leucoptera coffeella",
            riesgo: "Medio",
            desc: "Larvas de polilla que minan el parénquima de las hojas, creando manchas marrones secas.",
            tratamiento: "Control biológico con crisopas y liberación de parásitos.",
            comoIdentificar: "Presencia de manchas transparentes o abullonadas que luego se vuelven cafés y secas en el haz de la hoja.",
            pasosIdentificacion: [
                "Inspeccionar el haz de las hojas por zonas con parches secos.",
                "Examinar contrarreferencias de galerías internas.",
                "Evaluar el porcentaje de defoliación en el tercio medio."
            ],
            formulaTratamiento: "Extracto concentrado de Neem",
            dosisPor20Litros: "40 mililitros",
            frecuenciaTratamiento: "Cada 14 días en periodos secos."
        },
        {
            nombre: "Arañita Roja del Aguacate",
            cientifico: "Oligonychus perseae",
            riesgo: "Medio",
            desc: "Ácaro que teje finas telas en el envés de las hojas, succionando la savia y provocando necrosis.",
            tratamiento: "Aplicación de azufre mojable o acaricidas específicos.",
            comoIdentificar: "Manchas bronceadas o amarillentas a lo largo de las nervaduras principales en el envés de la hoja.",
            pasosIdentificacion: [
                "Revisar el envés de las hojas adultas con lupa de mano.",
                "Identificar puntos rojizos diminutos rodeados de telaraña fina.",
                "Evaluar la necrosis progresiva desde la nervadura central."
            ],
            formulaTratamiento: "Azufre micronizado mojable",
            dosisPor20Litros: "50 gramos",
            frecuenciaTratamiento: "Cada 15 días en temporadas cálidas."
        },
        {
            nombre: "Barrenador del Hueso del Aguacate",
            cientifico: "Conotrachelus perseae",
            riesgo: "Alto",
            desc: "Insecto cuyas larvas penetran el fruto y destruyen la semilla interna por completo.",
            tratamiento: "Eliminación de frutos caídos y aplicación de cebos.",
            comoIdentificar: "Frutos con pequeñas depresiones, exudaciones de goma cristalizada y perforaciones oscuras.",
            pasosIdentificacion: [
                "Buscar marcas de picadura o manchas de resina seca en frutos.",
                "Partir frutos caídos para detectar galerías con larvas.",
                "Monitorear la caída prematura de frutos."
            ],
            formulaTratamiento: "Insecticida a base de Spinosad",
            dosisPor20Litros: "30 mililitros",
            frecuenciaTratamiento: "Cada 20 días durante el desarrollo del fruto."
        },
        {
            nombre: "Trips del Aguacate",
            cientifico: "Scirtothrips perseae",
            riesgo: "Medio",
            desc: "Insectos alargados que se alimentan de la epidermis de frutos pequeños y hojas nuevas, causando cicatrices acorchadas.",
            tratamiento: "Uso de trampas cromáticas azules y extractos botánicos.",
            comoIdentificar: "Frutos jóvenes con anillos o manchas color café con textura áspera y acorchada.",
            pasosIdentificacion: [
                "Inspeccionar brotes tiernos y frutos recién cuajados.",
                "Sacudir ramas sobre una superficie blanca para ver insectos alargados.",
                "Detectar plateado superficial en hojas jóvenes."
            ],
            formulaTratamiento: "Extracto de ajo y ají picante",
            dosisPor20Litros: "60 mililitros",
            frecuenciaTratamiento: "Cada 7 días en infestaciones activas."
        },
        {
            nombre: "Cochinilla Harinosa",
            cientifico: "Pseudococcus comstocki",
            riesgo: "Medio",
            desc: "Insecto cubierto por una secreción cérea algodonosa que debilita brotes y frutos al succionar savia.",
            tratamiento: "Lavado a presión y aplicación de aceites agrícolas.",
            comoIdentificar: "Presencia de masas algodonosas blancas en las uniones de los pecíolos, ramas y frutos.",
            pasosIdentificacion: [
                "Revisar la base de los racimos de frutos y axilas de las hojas.",
                "Identificar colonias con aspecto de algodón blanco.",
                "Verificar la presencia de fumagina pegajosa."
            ],
            formulaTratamiento: "Aceite agrícola emulsionable",
            dosisPor20Litros: "150 mililitros",
            frecuenciaTratamiento: "Aplicación única mensual preventiva o cada 14 días en ataque fuerte."
        }
    ];

    const MAX_TAREAS = 10;

    const searchInput = document.getElementById("pest-search");
    const resultsContainer = document.getElementById("search-results");
    const quickBtns = document.querySelectorAll(".tag-btn");
    
    const labelRisk = document.getElementById("pest-risk");
    const labelName = document.getElementById("pest-name");
    const labelScientific = document.getElementById("pest-scientific");
    const labelDesc = document.getElementById("pest-desc");
    const labelTreat = document.getElementById("pest-treat");
    
    const btnAction = document.getElementById("btn-action");
    const tasksContainer = document.getElementById("active-tasks-container");
    const taskCounter = document.getElementById("task-counter");

    const labelHowToId = document.getElementById("pest-how-to-id");
    const labelStepsId = document.getElementById("pest-steps-id");
    const labelTreatmentSteps = document.getElementById("pest-treatment-steps");

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

            const filtradas = DB_PLAGAS.filter(plaga => 
                plaga.nombre.toLowerCase().includes(val) || 
                plaga.cientifico.toLowerCase().includes(val)
            );

            if (filtradas.length > 0) {
                filtradas.forEach(plaga => {
                    const li = document.createElement("li");
                    li.textContent = plaga.nombre;
                    li.addEventListener("click", () => {
                        cargarPlaga(plaga);
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

    document.addEventListener("click", (e) => {
        if (searchInput && resultsContainer && e.target !== searchInput) {
            resultsContainer.style.display = "none";
        }
    });

    quickBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            const nombrePlaga = btn.getAttribute("data-name");
            const plagaMatch = DB_PLAGAS.find(p => p.nombre === nombrePlaga);
            if (plagaMatch) {
                cargarPlaga(plagaMatch);
                if (searchInput instanceof HTMLInputElement) searchInput.value = plagaMatch.nombre;
            }
        });
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
        if (labelTreat) labelTreat.textContent = plaga.tratamiento;

        if (labelHowToId) labelHowToId.textContent = plaga.comoIdentificar;

        if (labelStepsId) {
            labelStepsId.innerHTML = "";
            plaga.pasosIdentificacion.forEach(/** @type {(paso: string) => void} */ (paso) => {
                const li = document.createElement("li");
                li.textContent = paso;
                labelStepsId.appendChild(li);
            });
        }

        if (labelTreatmentSteps) {
            labelTreatmentSteps.innerHTML = `
                <p><strong>Fórmula:</strong> ${plaga.formulaTratamiento}</p>
                <p><strong>Dosis por bomba de 20 litros:</strong> ${plaga.dosisPor20Litros}</p>
                <p><strong>Frecuencia de aplicación:</strong> ${plaga.frecuenciaTratamiento}</p>
            `;
        }
    }

    if (btnAction) {
        btnAction.addEventListener("click", () => {
            const tareasActuales = tasksContainer ? tasksContainer.querySelectorAll(".task-item").length : 0;
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
            const lote = "Lote General";
            const accionPredeterminada = "Aplicación de Tratamiento";

            const nuevaTarea = document.createElement("div");
            nuevaTarea.className = "task-item";
            nuevaTarea.innerHTML = `
                <div class="task-status-dot pending"></div>
                <div class="task-details">
                    <h4>${accionPredeterminada}</h4>
                    <p class="task-sub">${nombrePlaga} · <strong>${lote}</strong></p>
                    <span class="task-date">Iniciada hace unos instantes</span>
                </div>
                <button class="btn-complete-task"><i class="fa-solid fa-check"></i></button>
            `;

            if (tasksContainer) {
                tasksContainer.prepend(nuevaTarea);
                actualizarContador();
            }

            const win = /** @type {any} */ (window);
            if (win.chrome && win.chrome.webview) {
                win.chrome.webview.postMessage({
                    type: "nueva_tarea_plaga",
                    payload: { plaga: nombrePlaga, lote: lote, accion: accionPredeterminada }
                });
            }

            const originalText = btnAction.innerHTML;
            btnAction.style.backgroundColor = "#3b9d85";
            btnAction.innerHTML = `<i class="fa-solid fa-circle-check"></i> ¡Tarea Asignada!`;
            setTimeout(() => {
                btnAction.style.backgroundColor = "";
                btnAction.innerHTML = originalText;
            }, 1800);
        });
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

    function actualizarContador() {
        const total = tasksContainer ? tasksContainer.querySelectorAll(".task-item").length : 0;
        if (taskCounter) {
            taskCounter.textContent = `${total} Activas`;
        }
    }

    const winObj = /** @type {any} */ (window);
    if (winObj.chrome && winObj.chrome.webview) {
        winObj.chrome.webview.addEventListener("message", (/** @type {any} */ event) => {
            const { type, payload } = event.data;

            switch (type) {
                case "cargar_plaga_externa":
                    const plagaEncontrada = DB_PLAGAS.find(p => p.nombre.toLowerCase().includes(payload.nombre.toLowerCase()));
                    if (plagaEncontrada) {
                        cargarPlaga(plagaEncontrada);
                    }
                    break;
                default:
                    console.warn("Tipo de mensaje no reconocido en Módulo Plagas:", type);
            }
        });

        winObj.chrome.webview.postMessage({ type: "ready_plagas" });
    }

});