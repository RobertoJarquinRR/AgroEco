# AgroEco

Sistema de monitoreo y gestión agrícola que funciona **100% sin conexión a internet**, diseñado para productores de café y aguacate en zonas rurales de Nicaragua. Conecta sensores, tareas, inventario y finanzas en un solo sistema — no son módulos sueltos, es un ecosistema.

## El problema

En Nicaragua, un mismo productor suele cuidar varias fincas sin poder visitarlas todos los días. Cuando una plaga o un problema de agua se detecta a simple vista, el daño ya está hecho. La mayoría de soluciones tecnológicas asumen internet constante y usuarios con experiencia técnica — condiciones que no existen en el campo real.

## Qué nos diferencia

- **Funciona sin internet.** No es una app que "se degrada" sin señal — está diseñada desde cero para operar 100% offline, porque la conectividad en las parcelas nunca es confiable.
- **Un ecosistema conectado, no módulos sueltos.** Cuando un sensor detecta una condición fuera de rango, el sistema genera automáticamente la tarea correspondiente. Al completarla, descuenta el insumo usado del inventario y registra el gasto real en finanzas — sin que el productor tenga que anotar nada por separado.
- **Diseñado para el productor real**, no para el usuario ideal de una app de oficina: interfaz simple, sin jerga técnica, pensada para alguien sin experiencia previa con sistemas agrícolas.

## Descripción

AgroEco recopila datos del campo mediante sensores de hardware conectados a dispositivos Arduino, los interpreta de acuerdo con el estado del cultivo, y notifica al usuario cuando existe algo que requiere su atención. A partir de esas alertas, genera automáticamente las tareas, descuenta insumos y registra gastos — todo desde una sola plataforma, con reportes automáticos que facilitan la lectura de la información recopilada.

## Vista previa

*(capturas de pantalla de la interfaz — dashboard, gestor de tareas ,sensores)*
# *Login*
<img width="1647" height="832" alt="image" src="https://github.com/user-attachments/assets/14f55f30-7159-45a2-9639-826a98178a49" />
# *Selector de Fincas*
<img width="1908" height="855" alt="image" src="https://github.com/user-attachments/assets/3e300686-0987-4438-82f4-e8b506c1502f" />

# *DashBoard*
<img width="1880" height="857" alt="image" src="https://github.com/user-attachments/assets/6cd032bc-171c-449c-b902-2d185be2d672" />

# *Modulo de tareas*
<img width="1906" height="851" alt="image" src="https://github.com/user-attachments/assets/51f7370e-d4b7-44e6-a50b-9f52a0295c53" />

# *Modulo sensores*
<img width="1878" height="867" alt="image" src="https://github.com/user-attachments/assets/0c3d0c78-30c9-4884-921a-6a96595b6181" />

---

>Nota: ya todos están diseñados; estas se agregaron solo para demostración. 



## Características

**Monitoreo y alertas**
- Recepción de datos en tiempo real desde sensores Arduino
- Monitoreo de temperatura y humedad
- Detección automática de valores fuera de los parámetros establecidos
- Generación de alertas

**Ecosistema conectado**
- Generación automática de tareas a partir de alertas de sensores
- Descuento automático de insumos en inventario al completar una tarea
- Registro automático del gasto correspondiente en finanzas

**Gestión y reportes**
- Registro y administración de cultivos y parcelas
- Registro de usuarios y agricultores
- Almacenamiento de datos históricos
- Generación automática de informes con modelos predefinidos
- Consulta de informes anteriores
- Visualización de datos mediante gráficos

## Sensores utilizados

### DHT22
Sensor utilizado para obtener:
- Temperatura ambiental
- Humedad ambiental

### DS18B20
Sensor utilizado para medir la temperatura.

## Tecnologías utilizadas

### Lenguajes y tecnologías
- **HTML** — estructura de las interfaces
- **CSS** — diseño y estilos de las interfaces
- **JavaScript** — interacción y comportamiento de las interfaces
- **C#** — lógica principal y procesamiento de la información
- **C++** — programación de los dispositivos Arduino y manejo de datos de sensores
- **WPF** — interfaz de la aplicación de escritorio

### Librerías y dependencias
- `System.IO.Ports` — comunicación mediante puertos seriales
- `System.Reactive` — manejo de eventos y flujos de datos

### Herramientas
- **Visual Studio 2022** — desarrollo principal del proyecto
- **Visual Studio Code** — desarrollo y edición de código
- **GitHub** — control de versiones y seguimiento del código fuente
- **Trello** — organización y administración de tareas del proyecto

## Metodología

El desarrollo de AgroEco se organizó con la metodología **Scrum**, apoyándose en Trello para gestionar tareas, dar seguimiento a los avances y controlar los plazos de entrega.

## Requisitos

- Una computadora
- Dispositivo Arduino
- Sensores DHT22 y DS18B20
- Componentes de hardware necesarios para el sistema

## Instalación

### 1. Descargar el proyecto
Clonar el repositorio desde GitHub:
```
git clone https://github.com/RobertoJarquinRR/AgroEco.git
```

### 2. Abrir el proyecto
Abrir la solución con Visual Studio 2022.

> Los pasos adicionales de configuración e instalación de dependencias serán agregados conforme el proyecto avance.

## Hardware

AgroEco utiliza dispositivos Arduino para recopilar información mediante sensores físicos. Los datos obtenidos se envían al sistema para su procesamiento, generación de alertas y organización en informes.

## Estado del proyecto

**En desarrollo — Hackathon Nicaragua KRONOX 2026.**

## Autores

*Equipo AgroEco — [agregar nombres del equipo antes de la entrega final]*

## Licencia

*Pendiente de definir.*
