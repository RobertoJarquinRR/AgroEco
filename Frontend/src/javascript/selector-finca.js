document.addEventListener("DOMContentLoaded", () => {
    
    // --- 1. LÓGICA DE ELIMINAR FINCAS ---
    const contenedorFincas = document.querySelector(".boxes");

    if (contenedorFincas) {
        contenedorFincas.addEventListener("click", (e) => {
            
            if (!(e.target instanceof Element)) return;

            // AMPLIAMOS LA BÚSQUEDA: Ahora busca por clase O por el atributo alt
            const esBotonEliminar = e.target.classList.contains('delete-finca') || e.target.matches('img[alt="borrar-icon"]');

            if (esBotonEliminar) {
                // Buscamos la tarjeta completa
                const cardFinca = e.target.closest(".box");
                
                if (cardFinca) {
                    if (confirm("¿Estás seguro de que deseas eliminar esta finca?")) {
                        cardFinca.remove();
                    }
                }
            }
        });
    }
    // --- 2. LÓGICA DE ABRIR Y CERRAR EL MODAL ---
    const modal = document.getElementById("modalFinca");
    const btnAbrirModal = document.querySelector(".btnAdd");
    const btnCerrarModal = document.querySelector(".close-btn");

    if (btnAbrirModal && modal) {
        btnAbrirModal.addEventListener("click", () => {
            modal.style.display = "flex";
        });
    }

    if (btnCerrarModal && modal) {
        btnCerrarModal.addEventListener("click", () => {
            modal.style.display = "none";
        });
    }

    if (modal) {
        window.addEventListener("click", (e) => {
            if (e.target === modal) {
                modal.style.display = "none";
            }
        });
    }

    // --- 3. LÓGICA PARA CREAR LA NUEVA FINCA ---
    
    // Envolvemos la búsqueda para "forzar" al editor a entender qué tipo de elemento es
    const formulario = /** @type {HTMLFormElement} */ (document.querySelector("#formNuevaFinca"));

    if (formulario) {
        formulario.addEventListener("submit", (e) => {
            e.preventDefault();

            // Le decimos directamente "Confía en mí, esto es un HTMLInputElement"
            const inputNombre = /** @type {HTMLInputElement} */ (document.getElementById("inputNombre"));
            const inputUbicacion = /** @type {HTMLInputElement} */ (document.getElementById("inputUbicacion"));
            const inputCultivo = /** @type {HTMLInputElement} */ (document.getElementById("inputCultivo"));

            // Validamos que existan
            if (!inputNombre || !inputUbicacion || !inputCultivo || !contenedorFincas) return;

            const nombre = inputNombre.value;
            const ubicacion = inputUbicacion.value;
            const cultivo = inputCultivo.value;

            // Extraemos la primera letra del nombre para el ícono de la tarjeta
            const iniciales = nombre.substring(0, 2).toUpperCase();

            const nuevaTarjetaHTML = `
                <div class="box">
                    <span class="icon">${iniciales}</span>
                    <div class="union-vertical">
                        <p class="nombre-finca">${nombre}</p>
                        <div class="unionDescription">
                            <span class="fruta-finca">${cultivo}</span>
                            <span class="location-finca">${ubicacion}</span>
                        </div>                           
                    </div>
                    <img class="delete-finca" src="/images/svg-hackaton/trash 1.svg" alt="borrar-icon" style="cursor: pointer;">
                    <a href="dashboard.html">
                        <span class="aDash">Ir al DashBoard</span>
                        <img src="/images/svg-hackaton/Arrow 2.svg" alt="">
                    </a>
                </div>
            `;

            contenedorFincas.insertAdjacentHTML('beforeend', nuevaTarjetaHTML);

            formulario.reset();
            
            if (modal) {
                modal.style.display = "none";
            }
        });
    }

});