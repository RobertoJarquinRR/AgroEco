// -mostrar / ocultar contraseña-
//iconos para mostrar / ocultar
const mostrarPassword = document.getElementById("eye");
const ocultarPassword = document.getElementById("eye-hidden");
const nuevaPasswordInput = /** @type {HTMLInputElement} */ (document.getElementById("nueva-password")); //input para la contraseña

mostrarPassword?.addEventListener("click", () => {
    if (nuevaPasswordInput) nuevaPasswordInput.type = "text";
    if (mostrarPassword) mostrarPassword.style.display = "none";
    if (ocultarPassword) ocultarPassword.style.display = "block";
});

ocultarPassword?.addEventListener("click", () => {
    if (nuevaPasswordInput) nuevaPasswordInput.type = "password";
    if (ocultarPassword) ocultarPassword.style.display = "none";
    if (mostrarPassword) mostrarPassword.style.display = "block";
});

// -obtener nueva contraseña-
const btnCambiar = document.getElementById("btnCambiar"); //boton para cambiar contraseña
const alertaContra = document.getElementById("alerta"); //alerta
const alertaMensajeContra = document.getElementById("alerta-mensaje"); //mensaje que se mostrara en la alerta

btnCambiar?.addEventListener("click", () =>{
    const nuevaPassword = nuevaPasswordInput ? nuevaPasswordInput.value : "";

    // restablecer estilos
    if (nuevaPasswordInput) nuevaPasswordInput.style.border = "0.15rem solid var(--input)";
    if (alertaContra) alertaContra.style.display = "none";

    // comprobar que el campo de la contraseña no este vacio
    if (nuevaPassword === "") {
        if (nuevaPasswordInput) nuevaPasswordInput.style.border = "0.15rem solid #DC2626";
        if (alertaContra) alertaContra.style.display = "flex";
        if (alertaMensajeContra) alertaMensajeContra.textContent = "Ingrese una contraseña";
        return;
    }
    // enviar datos a C#
    const win = /** @type {any} */ (window);
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({
            type: "nuevaPassword",
            payload: {
                nuevaPassword
            }
        });
    }

});