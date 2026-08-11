updateSidebar();
/** @type {any[]} */
let insumos = [];
// obtener datos
const Win = /** @type {any} */ (window);
if (Win.chrome && Win.chrome.webview) {
    Win.chrome.webview.addEventListener("message", (/** @type {MessageEvent} */ event) => {
        const { type, payload } = event.data;
        switch (type) {
             case "listaFincas":
                cargarFincas(payload);
                break;

            case "listaInsumos":
                insumos = payload;
                cargarInsumos(insumos);
                actualizarResumen(insumos);
                actualizarAlerta(insumos);
                break;

            default:
                console.warn("Tipo de mensaje no reconocido:", type);
        }
    });

    Win.chrome.webview.postMessage({
        type: "ready"
    });
}

// -------- Esto es para probarlo --------
/*
const fincasPrueba = [
    {nombre: "Finca1"},
    {nombre: "Finca2"},
    {nombre: "Finca3"},
    {nombre: "Finca4"}
];
const insumosPrueba = [
    {id: 1, nombre: "Urea", categoria: "Fertilizante", cantidad: 3, unidad: "kg", minimo: 3,},
    {id: 2, nombre: "Abono Orgánico", categoria: "Injerto", cantidad: 15, unidad: "kg", minimo: 11,},
];
insumos = insumosPrueba
cargarFincas(fincasPrueba)
actualizarResumen(insumos);
actualizarAlerta(insumos);
cargarInsumos(insumos);
*/
// ---------------------------------------

// abrir / cerrar ventana para nuevo insumo
const ventana = document.getElementById("nuevo-insumo");
const btnNuevoInsumo = document.getElementById("btnNuevoInsumo");
const btnCancelar = document.getElementById("btnCancelar");

//abrir
if (btnNuevoInsumo && ventana) {
    btnNuevoInsumo.addEventListener("click", () => {
        ventana.style.display = "flex";
    });
}
//cerrar
if (btnCancelar && ventana) {
    btnCancelar.addEventListener("click", () => {
        ventana.style.display = "none";
    });
}
//cerrar al tocar afuera
if (ventana) {
    window.addEventListener("click", (e) => {
        if (e.target === ventana) {
            ventana.style.display = "none";
        }
    });
}

//crear nuevo insumo
const formulario = document.getElementById("formNuevoInsumo");
if(formulario){
    formulario.addEventListener("submit", (event) => {
        event.preventDefault();
        //obtiene los datos
        const insumo = {
            nombre: /** @type {HTMLInputElement} */(document.getElementById("inputNombre")).value.trim(),
            categoria: /** @type {HTMLElement} */(document.getElementById("selectCategoria")).dataset.value,
            cultivo:/** @type {HTMLInputElement} */ (document.getElementById("inputCultivo")).value.trim(),
            cantidad: Number( /** @type {HTMLInputElement} */ (document.getElementById("inputCantidad")).value),
            unidad: /** @type {HTMLElement} */(document.getElementById("selectUnidad")).dataset.value,
            stockMin: Number( /** @type {HTMLInputElement} */ (document.getElementById("inputStockMin")).value),
            caducidad: /** @type {HTMLInputElement} */(document.getElementById("inputCaducidad")).value,        
            finca: /** @type {HTMLElement} */(document.getElementById("selectFinca")).dataset.value,
            descripcion: /** @type {HTMLInputElement} */(document.getElementById("inputDescripcion")).value
        };
        Win.chrome.webview.postMessage({
            type: "crearInsumo",
            payload: insumo
        });
    });
    
}
//dropdown menu de la ventana para nuevo insumo
/** @type {NodeListOf<HTMLElement>} */
const selects = document.querySelectorAll(".select");
selects.forEach(select => {
    const boton = select.querySelector(".select__boton");
    const texto = /** @type {HTMLSpanElement} */ (select.querySelector(".select__texto"));
   
    /** @type {NodeListOf<HTMLElement>} */
    const opciones = select.querySelectorAll(".select__opcion");
    
    //por defecto se selecciona la primera opcion del menu
    const opcionDefault= opciones[0];
    opcionDefault.classList.add("seleccionado");
    texto.textContent = opcionDefault.textContent;
    select.dataset.value = opcionDefault.dataset.value;
    
    //abrir / cerrar menu
    if(boton){
        boton.addEventListener("click", () => {
            // cierra los demás menus
            selects.forEach(s => {
                if (s !== select) {
                    s.classList.remove("abierto");
                }
            });
            //abre el menu que se clickeo
            select.classList.toggle("abierto");
        });
    }
    
    //seleccionar opciones
    opciones.forEach(opcion => {
        opcion.addEventListener("click", () => {
            opciones.forEach(o => o.classList.remove("seleccionado"));
            opcion.classList.add("seleccionado");
            texto.textContent = opcion.textContent;
            // guarda el valor seleccionado en el propio select
            select.dataset.value = opcion.dataset.value;
            
            //cierra el menu despues de la seleccion
            select.classList.remove("abierto");
        });
    });
});

// cerrar todos los menus al hacer clic fuera
document.addEventListener("click", (e) => {
    const target = /** @type {Node} */ (e.target);
    selects.forEach(select => {
        if (!select.contains(target)) {
            select.classList.remove("abierto");
        }
    });

});

/** @param {Array<{nombre: string}>} fincas */
function cargarFincas(fincas) {    
    const menu = /** @type {HTMLElement} */ (document.querySelector("#selectFinca .select__menu"));
    // Elimina las opciones anteriores
    menu.innerHTML = "";

    fincas.forEach(finca => {
        const opcion = document.createElement("button");
        opcion.type = "button";
        opcion.className = "select__opcion";
        opcion.dataset.value = finca.nombre;
        opcion.textContent = finca.nombre;

        menu.appendChild(opcion);
    });
}
//actualizar total de isumos / stock bajo / total de categorias
/** @param {Array<{id: number, nombre: string, categoria: string, cantidad: number, unidad: string, minimo: number}>} insumos */
function actualizarResumen(insumos) {
    const totalInsumos = insumos.length;
    const totalStockBajo = insumos.filter(insumo => insumo.cantidad <= insumo.minimo).length;
    const totalCategorias = new Set(
        insumos.map(insumo => insumo.categoria)
    ).size;

    (
    /** @type {HTMLElement} */ (document.getElementById("total-insumos"))).textContent= String(totalInsumos);
    /** @type {HTMLElement} */ (document.getElementById("total-stock-bajo")).textContent = String(totalStockBajo);
    /** @type {HTMLElement} */ (document.getElementById("total-categorias")).textContent = String(totalCategorias);
}

// filtrar los insumos por categoria
/** @type {NodeListOf<HTMLElement>} */
const botonesCategoria = document.querySelectorAll(".btnCategoria");

botonesCategoria.forEach(boton => {
    boton.addEventListener("click", () => {
        botonesCategoria.forEach(o => o.classList.remove("seleccionado"));
        boton.classList.add("seleccionado");

        
        const categoria = /** @type {string} */ (boton.dataset.categoria); //obtener categoria seleccionada
        filtrarCategoria(categoria); //mostrar los insumos segun la categoria seleccionada
    });
});

/** @param {string} categoria*/
function filtrarCategoria(categoria) {
    let insumosFiltrados;
    if (categoria === "todos") {
        insumosFiltrados = insumos;
    } else {
        insumosFiltrados = insumos.filter(
            insumo => insumo.categoria === categoria
        );
    }
    cargarInsumos(insumosFiltrados);
}

/** @param {Array<{id: number, nombre: string, categoria: string, cantidad: number, unidad: string, minimo: number}>} insumos */
function cargarInsumos(insumos) {
    const contenedor = /** @type {HTMLElement} */  (document.getElementById("inventario__insumos"));
    contenedor.innerHTML = "";
    insumos.forEach(insumo => {
        const card = document.createElement("div");
        const stockBajo = insumo.cantidad <= insumo.minimo;
        
        card.className = `card ${insumo.categoria.toLowerCase()} ${stockBajo ? "stock-bajo" : ""}`;

        card.innerHTML = `
            <svg class="icono" xmlns="http://www.w3.org/2000/svg" id="Layer_1" data-name="Layer 1" viewBox="0 0 24 24">
            <path d="m0,3v7h10V0H3C1.346,0,0,1.346,0,3Zm8,5H2V3c0-.551.449-1,1-1h5v6Zm14-5c0-1.654-1.346-3-3-3h-7v10h10V3Zm-2,5h-6V2h5c.551,0,1,.449,1,1v5ZM0,19c0,1.654,1.346,3,3,3h7v-10H0v7Zm2-5h6v6H3c-.551,0-1-.449-1-1v-5Zm21.979,8.564l-2.812-2.812c.524-.791.833-1.736.833-2.753,0-2.757-2.243-5-5-5s-5,2.243-5,5,2.243,5,5,5c1.017,0,1.962-.309,2.753-.833l2.812,2.812,1.414-1.414Zm-6.979-2.564c-1.654,0-3-1.346-3-3s1.346-3,3-3,3,1.346,3,3-1.346,3-3,3Z"/>
            </svg>   
            <div class="card-info">
            <div class="card-info__cont">
            <p class="insumo-nombre">${insumo.nombre}</p>
                    <p class="insumo-categoria">${insumo.categoria}</p>
                    <p class="insumo-alerta">${stockBajo ? "Stock bajo" : ""}</p>
                </div>
                <div class="card-info__cont">
                    <p>${insumo.cantidad} ${insumo.unidad}</p>
                    <p class="sub-gris">min: ${insumo.minimo}</p>
                </div>
            </div>
            <svg class="eliminar-insumo" data-id="${insumo.id}" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M28.0001 5.33333H23.8667C23.5573 3.82855 22.7385 2.47646 21.5484 1.50496C20.3583 0.533453 18.8697 0.00193937 17.3334 0L14.6667 0C13.1305 0.00193937 11.6418 0.533453 10.4517 1.50496C9.26166 2.47646 8.44288 3.82855 8.13341 5.33333H4.00008C3.64646 5.33333 3.30732 5.47381 3.05727 5.72386C2.80722 5.97391 2.66675 6.31305 2.66675 6.66667C2.66675 7.02029 2.80722 7.35943 3.05727 7.60948C3.30732 7.85952 3.64646 8 4.00008 8H5.33341V25.3333C5.33553 27.1008 6.03859 28.7953 7.28837 30.045C8.53816 31.2948 10.2326 31.9979 12.0001 32H20.0001C21.7675 31.9979 23.462 31.2948 24.7118 30.045C25.9616 28.7953 26.6646 27.1008 26.6667 25.3333V8H28.0001C28.3537 8 28.6928 7.85952 28.9429 7.60948C29.1929 7.35943 29.3334 7.02029 29.3334 6.66667C29.3334 6.31305 29.1929 5.97391 28.9429 5.72386C28.6928 5.47381 28.3537 5.33333 28.0001 5.33333ZM14.6667 2.66667H17.3334C18.1604 2.66768 18.9669 2.9245 19.6422 3.40192C20.3175 3.87935 20.8286 4.55399 21.1054 5.33333H10.8947C11.1715 4.55399 11.6826 3.87935 12.3579 3.40192C13.0332 2.9245 13.8397 2.66768 14.6667 2.66667ZM24.0001 25.3333C24.0001 26.3942 23.5787 27.4116 22.8285 28.1618C22.0784 28.9119 21.0609 29.3333 20.0001 29.3333H12.0001C10.9392 29.3333 9.9218 28.9119 9.17165 28.1618C8.42151 27.4116 8.00008 26.3942 8.00008 25.3333V8H24.0001V25.3333Z" fill="currentColor"/>
                    <path d="M13.3333 24C13.687 24 14.0261 23.8595 14.2761 23.6095C14.5262 23.3594 14.6667 23.0203 14.6667 22.6667V14.6667C14.6667 14.313 14.5262 13.9739 14.2761 13.7239C14.0261 13.4738 13.687 13.3333 13.3333 13.3333C12.9797 13.3333 12.6406 13.4738 12.3905 13.7239C12.1405 13.9739 12 14.313 12 14.6667V22.6667C12 23.0203 12.1405 23.3594 12.3905 23.6095C12.6406 23.8595 12.9797 24 13.3333 24Z" fill="currentColor"/>
                    <path d="M18.6666 24C19.0202 24 19.3593 23.8595 19.6094 23.6095C19.8594 23.3594 19.9999 23.0203 19.9999 22.6667V14.6667C19.9999 14.313 19.8594 13.9739 19.6094 13.7239C19.3593 13.4738 19.0202 13.3333 18.6666 13.3333C18.313 13.3333 17.9738 13.4738 17.7238 13.7239C17.4737 13.9739 17.3333 14.313 17.3333 14.6667V22.6667C17.3333 23.0203 17.4737 23.3594 17.7238 23.6095C17.9738 23.8595 18.313 24 18.6666 24Z" fill="currentColor"/>
            </svg>
        `;
        contenedor.appendChild(card);
        
        //eliminar un insumo
        const btnEliminar =/** @type {SVGElement | null} */( card.querySelector(".eliminar-insumo"));
        
        if(btnEliminar){
            btnEliminar.addEventListener("click", () => {
                const id = btnEliminar.dataset.id;
                if (!confirm("¿Desea eliminar este insumo?")) {
                    return;
                }
                //envia el id del insumo que se elminó
                Win.chrome.webview.postMessage({
                    type: "eliminarInsumo",
                    payload: {
                        id
                    }
                });
            }); 
        }
    });
}

/** @param {Array<{id: number, nombre: string, categoria: string, cantidad: number, unidad: string, minimo: number}>} insumos */
function actualizarAlerta(insumos){
    const alerta = /** @type {HTMLElement} */(document.getElementById("inventario__alerta-stock"))
    const stockBajo = insumos.filter(
        insumo => insumo.cantidad <= insumo.minimo //filtra los insumos que tienen el stock bajo
    );
    //mostrar / ocultar la alerta
    if (stockBajo.length > 0) {
        alerta.style.display = "flex";
    } else {
        alerta.style.display = "none";
    }

    //agregar los insumos que tienen el stock bajo a la alerta
    const contenedor = /** @type {HTMLElement} */( document.getElementById("inventario__alerta-stock__insumos"));
    contenedor.innerHTML = "";
    stockBajo.forEach(stockBajoInsumo => {
        const card = document.createElement("p");
        card.className = "alerta-stock-dato";

        card.innerHTML = `${stockBajoInsumo.nombre}: ${stockBajoInsumo.cantidad} ${stockBajoInsumo.unidad} (min: ${stockBajoInsumo.minimo})`;

        contenedor.appendChild(card);
    });
}
