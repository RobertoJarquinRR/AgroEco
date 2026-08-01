// -mostrar / ocultar contraseña-
//iconos para mostrar / ocultar
const mostrar = document.getElementById("eye");
const ocultar = document.getElementById("eye-hidden");
const passwordInput = /** @type {HTMLInputElement} */ (document.getElementById("login-password")); //input para la contraseña

mostrar?.addEventListener("click", () => {
    if (passwordInput) passwordInput.type = "text";
    if (mostrar) mostrar.style.display = "none";
    if (ocultar) ocultar.style.display = "block";
});

ocultar?.addEventListener("click", () => {
    if (passwordInput) passwordInput.type = "password";
    if (ocultar) ocultar.style.display = "none";
    if (mostrar) mostrar.style.display = "block";
});

// -obtener los datos-
const btnLogin = document.getElementById("btnLogin"); //boton de login
const correoInput = /** @type {HTMLInputElement} */ (document.getElementById("login-correo")); //input para el correo
const alerta = document.getElementById("alerta"); //alerta
const alertaMensaje = document.getElementById("alerta-mensaje"); //mensaje que se mostrara en la alerta

btnLogin?.addEventListener("click", () => {
    //obtener correo, tipo: string
    const correo = correoInput ? correoInput.value : "";
    // obtener contraseña, tipo: string
    const password = passwordInput ? passwordInput.value : "";
    
    // restablecer estilos
    if (correoInput) correoInput.style.border = "0.15rem solid var(--input)";
    if (passwordInput) passwordInput.style.border = "0.15rem solid var(--input)";
    if (alerta) alerta.style.display = "none";
    
    // comprobar que el campo de correo no este vacio
    if (correo === "") {
        if (correoInput) correoInput.style.border = "0.15rem solid #DC2626";
        if (alerta) alerta.style.display = "flex";
        if (alertaMensaje) alertaMensaje.textContent = "Ingrese su correo";
        return;
    }
    // comprobar que el formato del correo sea valido
    if (correoInput && !correoInput.checkValidity()) {
        correoInput.style.border = "0.15rem solid #DC2626";
        if (alerta) alerta.style.display = "flex";
        if (alertaMensaje) alertaMensaje.textContent = "Correo inválido";
        return;
    }
    // comprobar que el campo de la contraseña no este vacio
    if (password === "") {
        if (passwordInput) passwordInput.style.border = "0.15rem solid #DC2626";
        if (alerta) alerta.style.display = "flex";
        if (alertaMensaje) alertaMensaje.textContent = "Ingrese su contraseña";
        return;
    }

    // enviar datos a C#
    const win = /** @type {any} */ (window);
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({
            type: "login",
            payload: {
                correo,
                password
            }
        });
    }
});

//-recuperar contraseña-
const btnRecuperarContra = document.getElementById("btnRecuperarContra"); //boton para recuperar contraseña
btnRecuperarContra?.addEventListener("click", () =>{
    //obtener correo, tipo: string
    const correo = correoInput ? correoInput.value : "";
    sessionStorage.setItem("correoRecuperacion", correo) //guarda el correo temporalemte para la pantalla de codigo de recuperacion
    
    // restablecer estilos
    if (correoInput) correoInput.style.border = "0.15rem solid var(--input)";
    if (alerta) alerta.style.display = "none";

    // comprobar que el campo de correo no este vacio
    if (correo === "") {
        if (correoInput) correoInput.style.border = "0.15rem solid #DC2626";
        if (alerta) alerta.style.display = "flex";
        if (alertaMensaje) alertaMensaje.textContent = "Ingrese un correo para enviar el código";
        return;
    }
    // comprobar que el formato del correo sea valido
    if (correoInput && !correoInput.checkValidity()) {
        correoInput.style.border = "0.15rem solid #DC2626";
        if (alerta) alerta.style.display = "flex";
        if (alertaMensaje) alertaMensaje.textContent = "Correo inválido";
        return;
    }
    // enviar datos a C#
    const win = /** @type {any} */ (window);
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({
            type: "recuperacion",
            payload: {correo}
        });
    }
})
