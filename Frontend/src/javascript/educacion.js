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
    if (Win.chrome && win.chrome.webview) {

    Win.chrome.webview.addEventListener(
        "message",
        (/** @type {MessageEvent} */ event) => {

            const { type, payload } = event.data;

            switch (type) {
                case "datosEtapa":
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

                case "detalleBioinsumo":
                    const card = /** @type {HTMLElement | null} */ (
                        document.querySelector(
                        `.accordion-item[data-id="${idBioInsumo}"]`
                        )
                    );

                    if (card) {
                        mostrarDetalleBioinsumo(payload, card);
                    }

                    break;

                case "alertaClimatica":
                    mostrarAlertaClimatica(payload);
                    break;

                default:
                    console.warn(
                        "Tipo de mensaje no reconocido:",
                        type
                    );
                    break;
            }
        });
        Win.chrome.webview.postMessage({
            type: "ready"
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
            type: type,
            payload: payload
        });
    }
    //Ciclo del cultivo
    /**
    * @param {string | number} cultivo
    */
    function seleccionarCultivo(cultivo) {
        idCultivo = Number(cultivo)
        solicitarDatosEtapa();
    }
    /**
     * @param {string | number} etapa
    */
    function seleccionarEtapa(etapa) {

        idEtapa = Number(etapa);
        solicitarDatosEtapa();
    }

    function solicitarDatosEtapa() {
        if (!idCultivo || !idEtapa) {
            return;
        }

        enviarMensaje("obtenerDatosEtapa", {
            idCultivoSelec: idCultivo,
            idEtapaSelec: idEtapa
        });
    }
    
    //Calculadora de altura maxima
    /**
     * @param {string | number} dist
    */
    function seleccionarDistancia(dist) {
        distancia = Number(dist);
        enviarMensaje("obtenerAlturaMaxima", distancia);
    }

    //Guias de poda
    /**
     * @param {string | number} poda
    */
    function seleccionarTipoPoda(poda){
        idPoda = Number(poda)
        enviarMensaje("obtenerDatosPoda", idPoda);
    }
    
    //Bioinsumos
    /**
     * @param {string | number} categoria
    */
    function seleccionarCategoriaBI(categoria){
        idCategoriaBioInsumo = Number(categoria)
        enviarMensaje("obtenerDatosCategoriaBioInsumo", idCategoriaBioInsumo);
    }
    /**
     * @param {string | number} insumo
    */
    function seleccionarBioInsumo(insumo){
        idBioInsumo = Number(insumo)
        enviarMensaje("obtenerDatosBioinsumo", idBioInsumo);
    }

    //--mostrar datos--
    //ciclo de cultivo
    /** @param {{id: number, nombre: string, duracion: string, descripcion: string, insumos: Array<{dia: number, nombre: string, dosis:string}>}} datos */
     function mostrarDatosEtapa(datos){
        document.querySelectorAll('.icon-step')
            .forEach(svg => svg.classList.remove('active'));

        /** @type {HTMLElement} */ (document.querySelector(`.icon-step[data-step="${idEtapa}"]`)).classList.add('active');
        /** @type {HTMLElement} */ (document.getElementById("etapaTitulo")).textContent = datos.nombre;
        /** @type {HTMLElement} */ (document.getElementById("etapaDuracion")).textContent = datos.duracion;
        /** @type {HTMLElement} */ (document.getElementById("etapaDescripcion")).textContent = datos.descripcion;

        const contenedor = /** @type {HTMLElement} */  (document.getElementById("vertical-timeline-box"));
        contenedor.innerHTML = "";
        datos.insumos.forEach(insumo => {
            const card = document.createElement("div");

            card.className = `timeline-step`;

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
            
            seleccionarEtapa(String(id))
            // mostrarDatosEtapa(datosEtapaPrueba) //para probar xd
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
            lbDist.forEach(dist => {
                dist.textContent = String(distancia);
            });
            seleccionarDistancia(distancia);
        }
    });
});