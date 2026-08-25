//pls no pongas el motor de typescript robertooo xd
//@ts-nocheck
/* ============================================================
   1) PUENTE — comunicación con C# (WebView2)
   nota hecho con IA debido a la comlejidad y falta de tiempo
   AT:Diego
   ============================================================ */
const AgroBridge = (() => {
    const listeners = {};

    function send(type, payload = {}) {
        if (!window.chrome?.webview) {
            console.warn(`[AgroBridge] WebView2 no disponible. Se intentó enviar: ${type}`, payload);
            return;
        }
        window.chrome.webview.postMessage({ type, payload });
    }

    function on(type, callback) {
        if (!listeners[type]) listeners[type] = [];
        listeners[type].push(callback);
    }

    function off(type, callback) {
        if (!listeners[type]) return;
        listeners[type] = listeners[type].filter(cb => cb !== callback);
    }

    function handleIncoming(event) {
        const data = event.data;
        if (!data || !data.type) {
            console.warn('[AgroBridge] Mensaje recibido sin "type":', data);
            return;
        }
        const callbacks = listeners[data.type];
        if (callbacks) {
            callbacks.forEach(cb => cb(data.payload));
        } else {
            console.warn(`[AgroBridge] Nadie está escuchando el evento "${data.type}"`);
        }
    }

    if (window.chrome?.webview) {
        window.chrome.webview.addEventListener('message', handleIncoming);
    } else {
        console.warn('[AgroBridge] No estás corriendo dentro de WebView2. send() no hará nada.');
    }

    return { send, on, off };
})();


/* ============================================================
   2) TARJETAS — pintar la lista de tareas
   ============================================================ */
// AQUI ESTA LA UNICA DECLARACION DE LA VARIABLE GLOBAL
let tareasLocales = [];
let filtroActual = 'todas';

const estadoConfig = {
    pendiente:  { label: 'Pendiente',   color: '#4A6FA5', bg: '#E3EAF3' },
    progreso:   { label: 'En Progreso', color: '#B8860B', bg: '#FDF2D0' },
    porVencer:  { label: 'Por Vencer',  color: '#e67e22', bg: '#FCE8D6' },
    vencida:    { label: 'Vencida',     color: '#C0392B', bg: '#FBE0DE' },
    completada: { label: 'Completada',  color: '#276A2A', bg: '#E1F0E1' },
};

const prioridadConfig = {
    alta:  { label: 'Alta',  color: '#8B3A3A', bg: '#FBE0E0' },
    media: { label: 'Media', color: '#8A6D1D', bg: '#FBF0D3' },
    baja:  { label: 'Baja',  color: '#3A6B4A', bg: '#E1F0E1' },
};

function crearCardTarea(tarea) {
    const estado = estadoConfig[tarea.estado] || estadoConfig.pendiente;
    const prioridad = prioridadConfig[tarea.prioridad] || prioridadConfig.media;

    const card = document.createElement('div');
    card.className = 'tarea-card';
    card.style.borderLeftColor = estado.color;
    card.dataset.id = tarea.id;

    card.innerHTML = `
        <div class="tarea-header">
            <h3>${tarea.nombre}</h3>
            <span class="tarea-badge" style="color:${estado.color}; background:${estado.bg}">
                ● ${estado.label}
            </span>
        </div>
        <p class="tarea-desc">${tarea.descripcion ?? ''}</p>
        <div class="tarea-meta">
            <span>📅 <strong>Vence:</strong> ${tarea.fechaLimite}</span>
            <span>👤 <strong>Asignado:</strong> ${tarea.asignado}</span>
            <span class="tarea-prioridad" style="color:${prioridad.color}; background:${prioridad.bg}">
                ${prioridad.label}
            </span>
        </div>
        <div class="tarea-acciones">
            <button class="btn-completar">✓ Marcar Completada</button>
            <button class="btn-editar">✎ Editar</button>
            <button class="btn-eliminar">🗑 Eliminar</button>
        </div>
    `;

    card.querySelector('.btn-completar').addEventListener('click', () => {
        AgroBridge.send('actualizarEstadoTarea', { id: tarea.id, estado: 'completada' });
        tarea.estado = 'completada';
        renderizarTareas(tareasLocales);
    });

    card.querySelector('.btn-editar').addEventListener('click', () => {
        abrirDialogEditar(tarea);
    });

    card.querySelector('.btn-eliminar').addEventListener('click', () => {
        const confirmar = confirm(`¿Seguro que quieres eliminar "${tarea.nombre}"?`);
        if (confirmar) {
            AgroBridge.send('eliminarTarea', { id: tarea.id });
            tareasLocales = tareasLocales.filter(t => t.id !== tarea.id);
            renderizarTareas(tareasLocales);
        }
    });

    return card;
}


// --- LÓGICA DE FILTROS ---
function inicializarFiltros() {
    const tarjetasFiltro = document.querySelectorAll('.card-filtro');
    
    tarjetasFiltro.forEach(tarjeta => {
        tarjeta.addEventListener('click', () => {
            // 1. Restaurar el borde por defecto a todas las tarjetas
            tarjetasFiltro.forEach(t => t.style.border = '2px solid #e1dfdf');
            
            // 2. Obtener el filtro seleccionado
            const tipoFiltro = tarjeta.dataset.filtro;
            
            // 3. Aplicar el color correspondiente al borde según tu configuración
            if (tipoFiltro !== 'todas' && estadoConfig[tipoFiltro]) {
                tarjeta.style.border = `2px solid ${estadoConfig[tipoFiltro].color}`;
            } else {
                tarjeta.style.border = `2px solid #2C2C2C`; // Color negro de 'Total'
            }
            
            // 4. Actualizar el filtro actual y re-renderizar
            filtroActual = tipoFiltro;
            renderizarTareas(tareasLocales);
        });
    });
}
// Inicializar los eventos de clic en las tarjetas
inicializarFiltros();


// --- RENDERIZADO CON FILTROS Y ESTADO VACÍO ---
function renderizarTareas(tareas) {
    tareasLocales = tareas;
    const contenedor = document.querySelector('.listadeTareas');
    if (!contenedor) return;
    
    contenedor.innerHTML = '';
    
    // Filtrar las tareas según la tarjeta seleccionada
    const tareasFiltradas = filtroActual === 'todas' 
        ? tareasLocales 
        : tareasLocales.filter(t => t.estado === filtroActual);
        
    // Si no hay tareas para mostrar, inyectamos el diseño idéntico a tu imagen
    if (tareasFiltradas.length === 0) {
        contenedor.innerHTML = `
            <div style="grid-column: 1 / -1; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; border: 2px dashed #b5b5b5; border-radius: 12px; background-color: #FFFFFF; text-align: center; margin-top: 10px;">
                <span style="font-size: 50px; margin-bottom: 15px;">📋🏼</span>
                <h2 style="font-size: 22px; font-weight: 700; color: #111827; margin: 0 0 10px 0;">No hay tareas para mostrar</h2>
                <p style="color: #6b7280; font-size: 15px; margin: 0;">Prueba con otro filtro o crea una nueva tarea.</p>
            </div>
        `;
    } else {
        // Si hay tareas, se renderizan de forma normal
        tareasFiltradas.forEach(tarea => {
            contenedor.appendChild(crearCardTarea(tarea));
        });
    }
    
    // Actualizar siempre los números de arriba basados en el total real
    actualizarContadores(tareasLocales);
}

function actualizarContadores(tareas) {
    document.getElementById('total-tareas').textContent = tareas.length;
    document.getElementById('pendientes').textContent = tareas.filter(t => t.estado === 'pendiente').length;
    document.getElementById('en-progreso').textContent = tareas.filter(t => t.estado === 'progreso').length;
    document.getElementById('por-vencer').textContent = tareas.filter(t => t.estado === 'porVencer').length;
    document.getElementById('vencidas').textContent = tareas.filter(t => t.estado === 'vencida').length;
    document.getElementById('completadas').textContent = tareas.filter(t => t.estado === 'completada').length;

    const porVencer = tareas.filter(t => t.estado === 'porVencer').length;
    const elemWarn = document.getElementById('tareasAVencer');
    if (elemWarn) elemWarn.textContent = porVencer;
}


/* ============================================================
   3) DIALOG — crear / editar tarea
   ============================================================ */
const dialogNuevaTarea = document.getElementById('dialog-n-tarea');
const botonesDialog = dialogNuevaTarea.querySelectorAll('.join button');
const btnCancelar = botonesDialog[0];
const btnGuardar = botonesDialog[1];

let modoEdicion = false;
let idTareaEditando = null;

function abrirDialogCrear() {
    modoEdicion = false;
    idTareaEditando = null;
    dialogNuevaTarea.querySelector('h1').textContent = 'Nueva tarea';
    btnGuardar.textContent = 'Guardar tarea';
    limpiarCampos();
    dialogNuevaTarea.showModal();
}

function abrirDialogEditar(tarea) {
    modoEdicion = true;
    idTareaEditando = tarea.id;
    dialogNuevaTarea.querySelector('h1').textContent = 'Editar tarea';
    btnGuardar.textContent = 'Guardar cambios';

    document.getElementById('name-tarea').value = tarea.nombre;
    document.getElementById('desc-tarea').value = tarea.descripcion ?? '';
    document.getElementById('respo-tarea').value = tarea.asignado ?? '';
    document.getElementById('inputPrioridad').value = tarea.prioridad;
    document.getElementById('inputFecha').value = tarea.fechaLimiteISO ?? tarea.fechaLimite;
    document.getElementById('estado-inicial').value = tarea.estado;

    dialogNuevaTarea.showModal();
}

function limpiarCampos() {
    document.getElementById('name-tarea').value = '';
    document.getElementById('desc-tarea').value = '';
    document.getElementById('respo-tarea').value = '';
    document.getElementById('inputPrioridad').value = 'baja';
    document.getElementById('inputFecha').value = '';
    document.getElementById('estado-inicial').value = 'pendiente';
}

function cerrarDialog() {
    dialogNuevaTarea.close();
}

document.getElementById('makeTarea').addEventListener('click', abrirDialogCrear);
btnCancelar.addEventListener('click', cerrarDialog);

btnGuardar.addEventListener('click', () => {
    const nombre = document.getElementById('name-tarea').value.trim();
    const fechaLimite = document.getElementById('inputFecha').value;

    if (!nombre || !fechaLimite) {
        alert('Nombre y fecha límite son obligatorios.');
        return;
    }

    const payload = {
        nombre: nombre,
        descripcion: document.getElementById('desc-tarea').value,
        asignado: document.getElementById('respo-tarea').value,
        prioridad: document.getElementById('inputPrioridad').value,
        fechaLimite: fechaLimite,
        estado: document.getElementById('estado-inicial').value,
    };

    if (modoEdicion) {
        payload.id = idTareaEditando;
        AgroBridge.send('actualizarTarea', payload);

        const index = tareasLocales.findIndex(t => t.id === idTareaEditando);
        if (index !== -1) tareasLocales[index] = { ...tareasLocales[index], ...payload };
    } else {
        payload.id = Date.now();
        AgroBridge.send('crearTarea', payload);

        tareasLocales.push(payload);
    }

    renderizarTareas(tareasLocales);
    cerrarDialog();
});


/* ============================================================
   4) ARRANQUE — pedir datos y reaccionar a las respuestas de C#
   ============================================================ */
AgroBridge.send('obtenerTareas');
AgroBridge.on('tareasCargadas', renderizarTareas);

AgroBridge.on('tareaCreada', () => AgroBridge.send('obtenerTareas'));
AgroBridge.on('tareaActualizada', () => AgroBridge.send('obtenerTareas'));
AgroBridge.on('tareaEliminada', () => AgroBridge.send('obtenerTareas'));