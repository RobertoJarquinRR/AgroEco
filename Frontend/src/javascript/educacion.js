document.addEventListener('DOMContentLoaded', () => {
    //datos de prueba
    // const datosEtapaPrueba = {
    // nombre: "Floración",
    // duracion: "15 días",
    // descripcion: "Etapa en la que la planta desarrolla sus flores y requiere un manejo adecuado de nutrientes y riego.",
    // icono: "floracion",
    // insumos: [
    //     {
    //         nombre: "Fertilizante NPK 15-15-15",
    //         dosis: "2 kg/ha",
    //         dia: 0
    //     },
    //     {
    //         nombre: "Fertilizante foliar",
    //         dosis: "1 L/ha",
    //         dia: 7
    //     },
    //     {
    //         nombre: "Fertilizante potásico",
    //         dosis: "1.5 kg/ha",
    //         dia: 15
    //     }
    // ]};

    // const datosPodaPrueba = {
    //     id:1,
    // nombre: "Poda de formación",
    // edad: "30 - 45 días",
    // objetivo: "Orientar el crecimiento de la planta y establecer una estructura adecuada para su desarrollo.",
    // procedimiento: [
    //     "Identificar el tallo principal y las ramas que se conservarán.",
    //     "Eliminar los brotes que crecen hacia el interior de la planta.",
    //     "Retirar ramas débiles o mal ubicadas.",
    //     "Realizar cortes limpios con una herramienta desinfectada."
    // ],
    // justificacion: "La poda de formación permite distribuir mejor el crecimiento de la planta, facilitar el manejo del cultivo y mejorar la exposición de las hojas a la luz."
    // };

    // const alertaPrueba ={
    //     mostrar: true,
    //     titulo: "Humedad alta",
    //     mensaje: "pruebaaaaa",
    // }

//     const datosBioinsumosPrueba = [
//     {
//         id: 1,
//         nombre: "Biol de estiércol",
//         categoria: "Biofertilizante",
//         descripcion: "Fertilizante líquido elaborado mediante la fermentación de materia orgánica, utilizado para aportar nutrientes y favorecer el desarrollo de los cultivos."
//     },
//     {
//         id: 2,
//         nombre: "Compost líquido",
//         categoria: "Biofertilizante",
//         descripcion: "Preparado orgánico líquido obtenido a partir de compost maduro, utilizado para mejorar la disponibilidad de nutrientes para las plantas."
//     },
//     {
//         id: 3,
//         nombre: "Extracto de ajo",
//         categoria: "Bioplaguicida",
//         descripcion: "Preparado vegetal utilizado como apoyo para el manejo de determinadas plagas mediante compuestos naturales presentes en el ajo."
//     },
//     {
//         id: 4,
//         nombre: "Extracto de neem",
//         categoria: "Bioplaguicida",
//         descripcion: "Bioinsumo de origen vegetal utilizado para el manejo de insectos que pueden afectar diferentes cultivos."
//     },
//     {
//         id: 5,
//         nombre: "Trichoderma",
//         categoria: "Control biológico",
//         descripcion: "Microorganismo utilizado como agente de control biológico y como apoyo para mantener un ambiente favorable en la zona radicular."
//     },
//     {
//         id: 6,
//         nombre: "Bacillus subtilis",
//         categoria: "Control biológico",
//         descripcion: "Microorganismo utilizado para apoyar el manejo biológico de algunos organismos que pueden afectar la salud de las plantas."
//     },
//     {
//         id: 7,
//         nombre: "Humus líquido",
//         categoria: "Biofertilizante",
//         descripcion: "Extracto líquido de materia orgánica procesada que puede utilizarse como complemento para mejorar la nutrición y el desarrollo del cultivo."
//     },
//     {
//         id: 8,
//         nombre: "Micorrizas",
//         categoria: "Bioestimulante",
//         descripcion: "Hongos benéficos que establecen una asociación con las raíces y pueden favorecer la absorción de agua y nutrientes."
//     }
// ];
// const detalleBioinsumoPrueba = {
//     ingredientes: [
//         {
//             ingrediente: "Estiércol bovino",
//             cantidad: "5 kg"
//         },
//         {
//             ingrediente: "Agua",
//             cantidad: "20 L"
//         },
//         {
//             ingrediente: "Melaza",
//             cantidad: "1 L"
//         },
//         {
//             ingrediente: "Levadura",
//             cantidad: "100 g"
//         }
//     ],

//     procedimiento: [
//         "Mezclar el estiércol con el agua en un recipiente limpio.",
//         "Agregar la melaza y la levadura.",
//         "Mezclar bien todos los ingredientes.",
//         "Tapar el recipiente dejando una pequeña salida para los gases.",
//         "Dejar fermentar durante 15 días.",
//         "Filtrar el preparado antes de utilizarlo."
//     ],

//     tiempo: "15 días",

//     dosis: "1 L de biol por cada 10 L de agua"
// };
    // --obtener datos--
const Win = /** @type {any} */ (window);
    if (Win.chrome && Win.chrome.webview) {
        // Entorno real de C# (WPF WebView2)
        Win.chrome.webview.addEventListener("message", (/** @type {MessageEvent} */ event) => {
            const { type, payload } = event.data;
            switch (type) {
                case "educacion_contenidos":
                    inicializarModuloEducacion(payload);
                    break;
                case "cargar_etapas":  // Coincide con lo que envía C# (EducacionHandler.cs línea 270)
                    mostrarDatosEtapa(payload);
                    break;
                case "alturaMaxima":
                    mostrarAlturaMaxima(payload);
                    break;
                case "infoPoda":
                    mostrarInfoPoda(payload);
                    break;
                case "listaBioinsumos":
                    cargarBioinsumos(payload);
                    break;
                case "cargar_detalle_bioinsumo":
                    mostrarDetalleBioinsumo(payload, document.querySelector('.accordion-item.open'));
                    break;
                case "alertaClimatica":
                    mostrarAlertaClimatica(payload);
                    break;
            }
        });
        // Usar el mismo tipo que define TiposMensaje.ListoEducacion = "listo_educacion"
        Win.chrome.webview.postMessage({ screen: "educacion", type: "listo_educacion" });
    } else {
        // Entorno de prueba en Navegador (Live Server / Vite)
        console.warn("Ejecutando en navegador web local. Cargando JSON localmente...");
        // La carpeta 'public' se sirve en la ra�z '/' por Vite
        fetch('/data/educacion.json')
            .then(res => {
                if (!res.ok) throw new Error("No se pudo cargar el archivo educacion.json");
                return res.json();
            })
            .then(data => {
                console.log("JSON cargado con éxito en navegador:", data);
                inicializarModuloEducacion(data);
            })
            .catch(err => console.error("Error al cargar el JSON:", err));
    }
    let datosGlobales = null; // Guardamos el JSON aquí para usarlo en los filtros
    const isBrowserMode = !(Win.chrome && Win.chrome.webview); // true si NO hay WebView2

    /**@param {*} data  */
    function inicializarModuloEducacion(data) {
        datosGlobales = data;
        
        // 1. Renderizar botones de cultivos dinámicamente si tienes un contenedor para ellos
        renderizarFiltroCultivos(data.cultivos);
        
        // 2. Cargar bioinsumos generales
        if (data.bioinsumos) {
            cargarBioinsumos(data.bioinsumos);
        }

        // 3. Seleccionar por defecto el primer cultivo si existe
        if (data.cultivos && data.cultivos.length > 0) {
            seleccionarCultivo(data.cultivos[0].id);
        }
    }
    // ========== MODO NAVEGADOR (sin C#) ==========
    // Estas funciones leen directo de datosGlobales en vez de pedir a C#
    
    /** @param {string | number} cultivo */
    function seleccionarCultivoBrowser(cultivo) {
        idCultivo = Number(cultivo);
        // Renderizar stepper de etapas para este cultivo
        renderizarStepperEtapas(idCultivo);
        // Seleccionar primera etapa por defecto
        const etapas = datosGlobales.etapas?.filter(e => e.cultivoIds.includes(idCultivo)) || [];
        if (etapas.length > 0) {
            seleccionarEtapaBrowser(etapas[0].id);
        }
    }
    
    /** @param {string | number} etapa */
    function seleccionarEtapaBrowser(etapa) {
        idEtapa = Number(etapa);
        const etapaData = datosGlobales.etapas?.find(e => e.id === idEtapa && e.cultivoIds.includes(idCultivo));
        if (etapaData) {
            mostrarDatosEtapa(etapaData);
        }
        // Actualizar UI stepper
        document.querySelectorAll('.step').forEach(s => s.classList.toggle('active', s.dataset.step == idEtapa));
        document.querySelectorAll('.etapa-panel').forEach(p => p.classList.toggle('active', p.dataset.step == idEtapa));
    }
    
    function renderizarStepperEtapas(cultivoId) {
        const stepper = document.querySelector('.stepper-horizontal');
        if (!stepper) return;
        const etapas = datosGlobales.etapas?.filter(e => e.cultivoIds.includes(cultivoId)) || [];
        if (etapas.length === 0) return;
        
        stepper.innerHTML = '';
        etapas.forEach((etapa, index) => {
            const stepDiv = document.createElement('div');
            stepDiv.className = 'step' + (index === 0 ? ' active' : '');
            stepDiv.dataset.step = etapa.id;
            stepDiv.innerHTML = `\n                <div class="step-icon">\n                    <svg class="icon-svg" viewBox="0 0 24 24"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm-1-13h2v6h-2zm0 8h2v2h-2z"/></svg>\n                </div>\n                <span class="step-label">${etapa.nombre}</span>\n            `;
            stepDiv.addEventListener('click', () => {
                document.querySelectorAll('.step').forEach(s => s.classList.remove('active'));
                stepDiv.classList.add('active');
                document.querySelectorAll('.etapa-panel').forEach(p => p.classList.toggle('active', p.dataset.step == etapa.id));
                seleccionarEtapaBrowser(etapa.id);
            });
            stepper.appendChild(stepDiv);
            if (index < etapas.length - 1) {
                const line = document.createElement('div');
                line.className = 'step-line';
                stepper.appendChild(line);
            }
        });
    }
    
    /** @param {string | number} poda */
    function seleccionarTipoPodaBrowser(poda) {
        idPoda = Number(poda);
        // Filtrar podas del cultivo actual y tipo seleccionado
        const podaData = datosGlobales.podas?.find(p => 
            p.tipoId === idPoda && p.cultivoIds.includes(idCultivo)
        );
        if (podaData) {
            mostrarInfoPoda(podaData);
        }
        // Actualizar filtros visuales
        document.querySelectorAll('#filter-poda .filter-btn').forEach(b => b.classList.toggle('active', b.dataset.filter == idPoda));
        document.querySelectorAll('.icon-filter').forEach(f => f.classList.toggle('active', f.dataset.filter == idPoda));
    }
    
    /** @param {string | number} categoria */
    function seleccionarCategoriaBIBrowser(categoria) {
        idCategoriaBioInsumo = Number(categoria);
        let bioinsumos = datosGlobales.bioinsumos || [];
        if (idCategoriaBioInsumo !== 1) { // 1 = "Todos"
            const catMap = {2: 'JADAM', 3: 'FPJ', 4: 'Extractos', 5: 'Cobertura'};
            const catNombre = catMap[idCategoriaBioInsumo];
            if (catNombre) bioinsumos = bioinsumos.filter(b => b.categoria === catNombre);
        }
        cargarBioinsumos(bioinsumos);
    }
    
    /** @param {string | number} insumo */
    function seleccionarBioInsumoBrowser(insumo) {
        idBioInsumo = Number(insumo);
        const bio = datosGlobales.bioinsumos?.find(b => b.id === idBioInsumo);
        if (bio) {
            const index = datosGlobales.bioinsumos?.findIndex(b => b.id === idBioInsumo) ?? 0;
            const card = document.querySelector(`.accordion-item:nth-child(${index + 1})`);
            if (card && card.classList.contains('open')) {
                mostrarDetalleBioinsumo(bio, card);
            }
        }
    }
    
    /** @param {string | number} dist */
    function seleccionarDistanciaBrowser(dist) {
        distancia = Number(dist);
        const altura = Math.round(distancia * 0.7 * 10) / 10; // 70% con 1 decimal
        mostrarAlturaMaxima(altura);
    }

    /**@param {Array<{id: number, nombre: string, icono: string}>} cultivos  */

    function renderizarFiltroCultivos(cultivos) {
        const contenedorFiltro = document.getElementById("filter-cultivos");
        if (!contenedorFiltro) return;

        contenedorFiltro.innerHTML = "";
        cultivos.forEach((cultivo, index) => {
            const btn = document.createElement("button");
            btn.className = `filter-btn ${index === 0 ? 'active' : ''}`;
            btn.setAttribute("data-filter", String(cultivo.id));
            btn.textContent = cultivo.nombre;
            
            btn.addEventListener('click', () => {
                contenedorFiltro.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                seleccionarCultivo(cultivo.id);
            });

            contenedorFiltro.appendChild(btn);
        });
    }

    let idCultivo = 1;
    let idEtapa = 1;
    let distancia = 5;
    let idPoda = 1;
    let idCategoriaBioInsumo = null;
    /** @type {number | null} */
    let idBioInsumo = null;
    
    //--comunicacion con c#--
    /**
    * @param {string} type
    * @param {*} payload
    */
    function enviarMensaje(type, payload) {   
        if (!Win.chrome || !Win.chrome.webview) {
            console.warn("WebView2 no está disponible.");
            return;
        }
        
        Win.chrome.webview.postMessage({
            screen: "educacion",
            type: type,
            payload: payload
        });
    }
    //Ciclo del cultivo
    /**
    * @param {string | number} cultivo
    */
    function seleccionarCultivo(cultivo) {
        if (isBrowserMode) { seleccionarCultivoBrowser(cultivo); return; }
        idCultivo = Number(cultivo);
        idEtapa = 1; // Reset a primera etapa del nuevo cultivo
        renderizarStepperEtapas(idCultivo); // Reconstruir stepper para este cultivo
        solicitarDatosEtapa();
    }
    /**
     * @param {string | number} etapa
    */
    function seleccionarEtapa(etapa) {
        if (isBrowserMode) { seleccionarEtapaBrowser(etapa); return; }
        idEtapa = Number(etapa);
        solicitarDatosEtapa();
    }

    function solicitarDatosEtapa() {
        if (!idCultivo || !idEtapa) {
            return;
        }

        // Coincide con TiposMensaje.ObtenerDatosEtapa = "obtener_datos_etapa"
        enviarMensaje("obtener_datos_etapa", {
            idCultivoSelec: idCultivo,
            idEtapaSelec: idEtapa
        });
    }
    
    //Calculadora de altura maxima
    /**
     * @param {string | number} dist
    */
    function seleccionarDistancia(dist) {
        if (isBrowserMode) { seleccionarDistanciaBrowser(dist); return; }
        distancia = Number(dist);
        enviarMensaje("obtenerAlturaMaxima", distancia);
    }

    //Guias de poda
    /**
     * @param {string | number} poda
    */
    function seleccionarTipoPoda(poda){
        if (isBrowserMode) { seleccionarTipoPodaBrowser(poda); return; }
        idPoda = Number(poda)
        enviarMensaje("obtenerDatosPoda", idPoda);
    }
    
    //Bioinsumos
    /**
     * @param {string | number} categoria
    */
    function seleccionarCategoriaBI(categoria){
        if (isBrowserMode) { seleccionarCategoriaBIBrowser(categoria); return; }
        idCategoriaBioInsumo = Number(categoria)
        enviarMensaje("obtenerDatosCategoriaBioInsumo", idCategoriaBioInsumo);
    }
    /**
     * @param {string | number} insumo
    */
    function seleccionarBioInsumo(insumo){
        if (isBrowserMode) { seleccionarBioInsumoBrowser(insumo); return; }
        idBioInsumo = Number(insumo)
        enviarMensaje("obtenerDatosBioinsumo", idBioInsumo);
    }

    //--mostrar datos--
    //ciclo de cultivo
    /** @param {{id: number, nombre: string, duracion: string, descripcion: string, insumos: Array<{dia: number, nombre: string, dosis:string}>} | Array<{id: number, nombre: string, duracion: string, descripcion: string, insumos: Array<{dia: number, nombre: string, dosis:string}>}>} datos */
    function mostrarDatosEtapa(datos){
        // C# envía un array (lista), tomar el primer elemento
        const etapa = Array.isArray(datos) ? datos[0] : datos;
        if (!etapa) return;
        
        const panel = document.getElementById(`etapa-${etapa.id ?? idEtapa}`);
        if (!panel) return;

        /** @param {string} campo */
        const campo = (campo) => /** @type {HTMLElement} */ (panel.querySelector(`[data-field="${campo}"]`));

        campo("nombre").textContent = etapa.nombre;
        campo("duracion").textContent = etapa.duracion;
        campo("descripcion").textContent = etapa.descripcion;

        const contenedor = campo("insumos");
        contenedor.innerHTML = "";
        (etapa.insumos || []).forEach(insumo => {
            const card = document.createElement("div");
            card.className = "timeline-step";
            card.innerHTML = `
                <div class="timeline-line"></div>
                <span class="timeline-label">Día ${insumo.dia}</span>
                <div class="timeline-content">
                    <p class="insumo-name">${insumo.nombre}</p>
                    <p class="insumo-dosis">${insumo.dosis}</p>
                </div>
            `;
            contenedor.appendChild(card);
        });
    }
//calculadora de altura maxima
 /** @param {number} altura */
function mostrarAlturaMaxima(altura){
    /** @type {HTMLElement} */ (document.getElementById("lbAlturaMaxima")).textContent = String(altura);
}

//guias de poda
/** @param {{id: number, nombre: string, edad: string, objetivo: string, justificacion: string, procedimiento: string[]}} datos */
function mostrarInfoPoda(datos){
    document.querySelectorAll('.icon-filter')
        .forEach(filter => filter.classList.remove('active'));
    /** @type {HTMLElement} */ (document.querySelector(`.icon-filter[data-filter="${idPoda}"]`)).classList.add('active');
    
    /** @type {HTMLElement} */ (document.getElementById("podaTitulo")).textContent = datos.nombre;
    /** @type {HTMLElement} */ (document.getElementById("podaEdadCultivo")).textContent = datos.edad;
    /** @type {HTMLElement} */ (document.getElementById("podaObjetivo")).textContent = datos.objetivo;
    /** @type {HTMLElement} */ (document.getElementById("podaJustificacion")).textContent = datos.justificacion;
    
    const contenedor = /** @type {HTMLOListElement} */  (document.getElementById("pasos-poda"));
        contenedor.innerHTML = "";
        datos.procedimiento.forEach(paso => {
            const card = document.createElement("li");
            card.innerHTML = `${paso}`;
            contenedor.appendChild(card);
    });
}

//alerta que aparece en las guias de poda
/**
 * @param {{
 *     mostrar: boolean,
 *     titulo: string,
 *     mensaje: string,
 * }} alerta
 */
function mostrarAlertaClimatica(alerta) {

    const elemento = /** @type {HTMLElement} */ (document.getElementById("alerta"));
    
    if (!alerta.mostrar) {
        elemento.style.display = "none";
        return;
    }

    const titulo = document.getElementById("alertaTitulo");
    const mensaje = document.getElementById("alertaMensaje");

    if (titulo) {
        titulo.textContent = alerta.titulo;
    }

    if (mensaje) {
        mensaje.textContent = alerta.mensaje;
    }

    elemento.style.display = "flex";
}

//bioinsumos
/** @param {Array<{id: number, nombre: string, categoria: string, descripcion: string}>} datos */
function cargarBioinsumos(datos){
    const contenedor = /** @type {HTMLElement} */  (document.getElementById("bioinsumo-accordion"));
        contenedor.innerHTML = "";
        datos.forEach(bioinsumo => {
            const card = document.createElement("div");

            card.className = `accordion-item`;
            card.innerHTML = `
                        <div class="accordion-header">
                            <div class="header-left">
                                <span class="bioinsumo-title">${bioinsumo.nombre}</span>
                                <span class="tag green">${bioinsumo.categoria}</span>
                            </div>
                            <p class="accordion-subtitle">${bioinsumo.descripcion}</p>
                            <img src="../../images/svg-hackaton/angle-small-right.svg" alt="Colapsar" class="icon-svg accordion-arrow">
                        </div>
                        <div class="accordion-content">
                        </div>
            `;
            card.addEventListener("click", () => {
                const cards = contenedor.querySelectorAll(".accordion-item");

                cards.forEach(otraCard => {
                    if (otraCard !== card) {
                        otraCard.classList.remove("open");
                    }
                });

                card.classList.toggle("open");

                if (card.classList.contains("open")) {
                    seleccionarBioInsumo(bioinsumo.id)
                    // mostrarDetalleBioinsumo(detalleBioinsumoPrueba, card) //para probar :b
                }
            });
            contenedor.appendChild(card);

    });
}
/** 
 * @param {{ingredientes: Array<{ingrediente: string, cantidad: string}>, procedimiento:string[], tiempo: string, dosis:string}} datos 
  * @param {HTMLElement} card
*/
function mostrarDetalleBioinsumo(datos, card){
    const contenedor = /** @type {HTMLElement} */  (card.querySelector(".accordion-content"));
    contenedor.innerHTML = "";
       
    const ingredientesHTML = datos.ingredientes.map(ingrediente => `
        <div class="ingredient">
            <span>${ingrediente.ingrediente}</span>
            <span class="cant">${ingrediente.cantidad}</span>
        </div>
    `).join("");

    // Procedimiento
    const procedimientoHTML = datos.procedimiento.map(paso => `
        <li>${paso}</li>
    `).join("");
    contenedor.innerHTML = `
        <p class="inner-title">Ingredientes</p>

        <div class="ingredients-list">
            ${ingredientesHTML}
        </div>

        <p class="inner-title">Proceso</p>

        <ol class="procedure-list accordion">
            ${procedimientoHTML}
        </ol>

        <div class="info-pills">

            <div class="pill orange">
                <img 
                    src="../../images/svg-hackaton/clock.svg"
                    alt="Tiempo"
                    class="icon-svg"
                >

                <div>
                    <span>Tiempo</span>
                    <span class="pill-value">${datos.tiempo}</span>
                </div>
            </div>

            <div class="pill green">
                <img 
                    src="../../images/svg-hackaton/blood 1.svg"
                    alt="Dosis"
                    class="icon-svg"
                >

                <div>
                    <span>Dosis</span>
                    <span class="pill-value">${datos.dosis}</span>
                </div>
            </div>

        </div>
    `;
}
//--botones--
//ciclo cultivo - guias de poda - bioinsumos
    const navButtons = document.querySelectorAll('.sub-nav .nav-btn');    
    const tabContents = document.querySelectorAll('.tab-content');
    navButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetId =/** @type {HTMLElement} */(button).dataset.target;

            navButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(tab => tab.classList.remove('active'));

            button.classList.add('active');
            const targetTab = document.getElementById(String(targetId));
            if (targetTab) {
                targetTab.classList.add('active');
            }
            // mostrarAlertaClimatica(alertaPrueba) // para probar
        });
    });

    //filtros - cultivos - tipos de poda - categoria de insumo
    const filtersRow = document.querySelectorAll('.filters-row');
    const filtroCultivos = document.getElementById("filter-cultivos");
    const filtroPoda = document.getElementById("filter-poda");
    const filtroInsumo = document.getElementById("filter-insumos");
    filtersRow.forEach(row => {
        //Cultivos (ciclo del cultivo)
        if(filtroCultivos){
            const botones = row.querySelectorAll('.filter-btn');
            
            botones.forEach(btn => {
                btn.addEventListener('click', () => {
                    botones.forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    
                    const id = /** @type {HTMLElement} */(btn).dataset.filter;
                    
                    seleccionarCultivo(String(id))
                });
            });
        }
        //tipo de poda (Guias de poda)
        if(filtroPoda){
            const botones = row.querySelectorAll('.filter-btn');
            
            botones.forEach(btn => {
                btn.addEventListener('click', () => {
                    botones.forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    
                    const id = /** @type {HTMLElement} */(btn).dataset.filter;
                    seleccionarTipoPoda(String(id))
                    // mostrarInfoPoda(datosPodaPrueba) //para probar
                });
            });
        }
        //categoria de insumo (Bioinsumos)
        if(filtroInsumo){
            const botones = row.querySelectorAll('.filter-btn');
            
            botones.forEach(btn => {
                btn.addEventListener('click', () => {
                    botones.forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    
                    const id =/** @type {HTMLElement} */ (btn).dataset.filter;
                    
                    seleccionarCategoriaBI(String(id))
                    // cargarBioinsumos(datosBioinsumosPrueba) //para probar
                });
            });
        }
    });
    
// fases del cultivo (ciclo del cultivo)
    const step = document.querySelectorAll('.stepper-horizontal .step');
    step.forEach(boton => {
    boton.addEventListener("click", () => {
        step.forEach(o => o.classList.remove("active"));
        boton.classList.add("active");

        const id = /** @type {HTMLElement} */(boton).dataset.step;

        document.querySelectorAll('.etapa-panel').forEach(p => {
            p.classList.toggle("active", /** @type {HTMLElement} */(p).dataset.step === id);
        });

        seleccionarEtapa(String(id));
    });
});

    //valores de la calculadora de altura maxima
    const rango = /** @type {HTMLInputElement} */(document.getElementById("input-rango"));
    const valorRango = /** @type {HTMLInputElement} */(document.getElementById("input-numero"));
    const lbDist = document.querySelectorAll(".lbDistancia");

    rango.addEventListener("input", () => {
        distancia = Number(rango.value)
        valorRango.value = String(distancia);
        lbDist.forEach(dist => {
                dist.textContent = String(distancia);
            });
        seleccionarDistancia(distancia);
    }); 

    valorRango.addEventListener("input", () => {
        if (valorRango.value !== ""){
            distancia = Number(valorRango.value)
            rango.value = String(distancia);
            lbDist.forEach(dist => {
                dist.textContent = String(distancia);
            });
            seleccionarDistancia(distancia);
        }
    });
});







