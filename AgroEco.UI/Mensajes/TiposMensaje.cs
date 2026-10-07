namespace AgroEco.UI.Mensajes;

public static class TiposMensaje
{
    // Plagas
    public const string ListoPlagas = "listo_plagas";
    public const string ObtenerPlagas = "obtener_plagas";
    public const string ObtenerDetallePlaga = "obtener_detalle_plaga";
    public const string ObtenerDetalleCultivo = "obtener_detalle_cultivo";
    public const string ObtenerTodasPlagas = "obtener_todas_plagas";

    // Educación
    public const string ListoEducacion = "listo_educacion";
    public const string ObtenerInfoPoda = "obtener_info_poda";
    public const string ObtenerDatosEtapa = "obtener_datos_etapa";
    public const string ObtenerBioinsumos = "obtener_bioinsumos";
    public const string ObtenerDetalleBioinsumo = "obtener_detalle_bioinsumo";
    public const string ObtenerAlertaClimatica = "obtener_alerta_climatica";

    // Sensores
    public const string ListoSensores = "listo_sensores";
    public const string LecturaSensor = "lectura_sensor";

    // Tareas
    public const string ObtenerTareas = "obtener_tareas";
    public const string CrearTarea = "crear_tarea";
    public const string ActualizarTarea = "actualizar_tarea";
    public const string EliminarTarea = "eliminar_tarea";
    public const string EjecutarTarea = "ejecutar_tarea";
    public const string ObtenerTriggersDisponibles = "obtener_triggers_disponibles";
    public const string ObtenerActionsDisponibles = "obtener_actions_disponibles";
    public const string ObtenerTareaDetalle = "obtener_tarea_detalle";
    public const string ObtenerHistorialTarea = "obtener_historial_tarea";
    public const string ExportarTareasCsv = "exportar_tareas_csv";

    // Inventario
    public const string ObtenerInsumos = "obtener_insumos";
    public const string CrearInsumo = "crear_insumo";
    public const string EliminarInsumo = "eliminar_insumo";
    public const string ActualizarInsumo = "actualizar_insumo";

    // Finanzas
    public const string ObtenerRegistros = "obtener_registros";
    public const string CrearRegistro = "crear_registro";
    public const string EliminarRegistro = "eliminar_registro";
}