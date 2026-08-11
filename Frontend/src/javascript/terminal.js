// @ts-nocheck

const input = document.getElementById("input-cli");
const output = document.getElementById("output");

// focus permanente
document.addEventListener('click', () => {
    input.focus();
});

// ============================================
// ESTADO DE LA CONEXIÓN
// ============================================
let conectado = false;        // ¿el usuario pidió "start"?
let intervaloRevision = null; // referencia al setInterval, para poder cancelarlo
let ultimoDatoRecibido = 0;   // timestamp del último mensaje real del C#

// ============================================
// IMPRIMIR EN PANTALLA
// ============================================
function print(text, className) {
    const line = document.createElement('div');
    line.textContent = text;
    if (className) { line.classList.add(className); }
    output.appendChild(line);
    output.scrollTop = output.scrollHeight;
}

// ============================================
// START: empieza a "escuchar" datos del C#
// ============================================
function iniciarConexion() {
    if (conectado) {
        print('⚠ Ya está conectado', 'error');
        return;
    }

    conectado = true;
    ultimoDatoRecibido = Date.now();
    print('▶ Conectando con sensores...', 'highlight');

    // Cada 1 segundo revisa si llegó algo nuevo
    intervaloRevision = setInterval(() => {
        const tiempoSinDatos = Date.now() - ultimoDatoRecibido;

        if (tiempoSinDatos > 1000) {
            print('⚠ No se reciben datos', 'error');
        }
    }, 1000);
}

// ============================================
// STOP: corta el bucle
// ============================================
function detenerConexion() {
    if (!conectado) {
        print('⚠ No hay ninguna conexión activa', 'error');
        return;
    }

    conectado = false;
    clearInterval(intervaloRevision); 
    intervaloRevision = null;
    print('■ Conexión detenida', 'error');
}

// ============================================
// INPUT DEL USUARIO: solo entiende start/stop/clean
// ============================================
input.addEventListener('keydown', function (e) {
    if (e.key === "Enter") {
        const comando = input.value.trim().toLowerCase();

        if (comando === 'clean') {
            output.innerHTML = '';
        } else if (comando === 'start') {
            iniciarConexion();
        } else if (comando === 'stop') {
            detenerConexion();
        }
        // cualquier otra cosa: no hace nada

        input.value = '';
    }
});

// ============================================
// RECIBIR DATOS DEL C#
// ============================================
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', (event) => {
        if (!conectado) return;

        ultimoDatoRecibido = Date.now(); // resetea el timer de "sin datos"
        print(event.data);
    });
} else {
    print('⚠ WebView2 no disponible en este contexto', 'error');
}