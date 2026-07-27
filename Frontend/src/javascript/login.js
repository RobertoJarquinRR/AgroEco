// -mostrar / ocultar contraseña-
//iconos para mostrar / ocultar
const mostrar = document.getElementById("eye");
const ocultar = document.getElementById("eye-hidden");
const passwordInput = document.getElementById("login-password"); //input para la contraseña

mostrar.addEventListener("click", () => {
    passwordInput.type = "text";
    mostrar.style.display = "none";
    ocultar.style.display = "block";
});

ocultar.addEventListener("click", () => {
    passwordInput.type = "password";
    ocultar.style.display = "none";
    mostrar.style.display = "block";
});

// -obtener los datos-
const btnLogin = document.getElementById("btnLogin"); //boton de login
const correoInput = document.getElementById("login-correo"); //input para el correo
const alerta = document.getElementById("alerta"); //alerta
const alertaMensaje = document.getElementById("alerta-mensaje"); //mensaje que se mostrara en la alerta
btnLogin.addEventListener("click", () => {
    //obtener correo, tipo: string
    const correo = document.getElementById("login-correo").value;
    // obtener contraseña, tipo: string
    const password = document.getElementById("login-password").value;
    
    // restablecer estilos
    correoInput.style.border = "0.15rem solid var(--input)";
    passwordInput.style.border = "0.15rem solid var(--input)";
    alerta.style.display = "none";
    
    // comprobar que el campo de correo no este vacio
    if (correo === "") {
        correoInput.style.border = "0.15rem solid #DC2626";
        alerta.style.display = "flex";
        alertaMensaje.textContent = "Ingrese su correo";
        return;
    }
    // comprobar que el formato del correo sea valido
    if (!correoInput.checkValidity()) {
        correoInput.style.border = "0.15rem solid #DC2626";
        alerta.style.display = "flex";
        alertaMensaje.textContent = "Correo inválido";
        return;
    }
    // comprobar que el campo de la contraseña no este vacio
    if (password === "") {
        passwordInput.style.border = "0.15rem solid #DC2626";
        alerta.style.display = "flex";
        alertaMensaje.textContent = "Ingrese su contraseña";
        return;
    }

    // enviar datos a C#
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({
            type: "login",
            payload: {
                correo,
                password
            }});
    }
});