/** @type {NodeListOf<HTMLInputElement>} */
const inputs = document.querySelectorAll(".codigo-input"); //obtiene todos los inputs para el codigo
// -avanzar / volver al escribir y borrar el codigo-
inputs.forEach((input, index) => {
    input.addEventListener("input", () => {
        input.value = input.value.replace(/\D/g, ""); //hace que solo se puedan escribir numeros
        //pasa al siguiente input despues de ingresar un numero del codigo
        if (input.value && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }
    });
    
    //vuelve al input anterior al borrar un numero del codigo
    input.addEventListener("keydown", (e) => {
        if (e.key === "Backspace" && input.value === "" && index > 0) {
            inputs[index - 1].focus();
        }
    });
});
const correo = sessionStorage.getItem("correoRecuperacion"); //correo guardado en el login
const correoRecuperacion = document.getElementById("correo-recuperacion"); //elemento que mostrará el correo
if(correoRecuperacion) correoRecuperacion.textContent = correo; //muestra el correo que se ingresó en login
// -verificar codigo-
const btnVerificar = document.getElementById("btnVerificar"); //boton para verificar el codigo
const alertaCodigo = document.getElementById("alerta"); //alerta
const alertaCodigoMensaje = document.getElementById("alerta-mensaje"); //mensaje que se mostrara en la alerta

btnVerificar?.addEventListener("click", () => {
    // restablecer alerta
    if (alerta) alerta.style.display = "none";

   // verifica que todos los campos tengan un valor
    const codigoCompleto = Array.from(inputs)
        .every(input => input.value !== "");

    //muestra una alerta en caso de que no esten completos
    if (!codigoCompleto) {
        if (alertaCodigo) alertaCodigo.style.display = "flex";
        if (alertaCodigoMensaje) alertaCodigoMensaje.textContent = "Debe completar el código de verificación.";
        return;
    }
    // obtener el código completo
    // tipo: string
    const codigo = Array.from(inputs)
        .map(input => input.value)
        .join("");

    // enviar el código a C#
    const win = /** @type {any} */ (window);
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({
            type: "codigoVerificacion",
            payload: {
                codigo
            }
        });
    }
});

const btnReenviar = document.getElementById("btnReenviar"); //boton para reenviar el codigo
let tiempoEspera = 30; //tiempo del cooldown para reenviar
btnReenviar?.addEventListener("click", () =>{
    // restablecer alerta
    if (alerta) alerta.style.display = "none";

    if (btnReenviar instanceof HTMLButtonElement) {btnReenviar.disabled = true;} //desactiva el boton

    //alerta por si acaso no se encuentra el correo
    if (!correo) {
        if (alertaCodigo) alertaCodigo.style.display = "flex";
        if (alertaCodigoMensaje) alertaCodigoMensaje.textContent = "No se encontró el correo de recuperación.";
        return;
    }

    // enviar el correo a C#
    const win = /** @type {any} */ (window);
    if (win.chrome && win.chrome.webview) {
        win.chrome.webview.postMessage({
            type: "reenviarCodigo",
            payload: {
                correo
            }
        });
    }

    //cooldown para reeenviar
     const intervalo = setInterval(() => {
        tiempoEspera--;

        btnReenviar.textContent = `Enviar de nuevo (${tiempoEspera}s)`;

        if (tiempoEspera <= 0) {
            clearInterval(intervalo);
            if (btnReenviar instanceof HTMLButtonElement) {btnReenviar.disabled = false;} //reactiva el boton despues de pasado el cooldown
            btnReenviar.textContent = "Enviar de nuevo";
            tiempoEspera = 30;
        }
    }, 1000);
})
