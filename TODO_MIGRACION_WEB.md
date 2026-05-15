# TODO Migracion Funcionalidad WPF -> Web

## Objetivo
- [ ] Dejar `EleccionesWeb` funcionalmente equivalente a la operacion actual de `Elecciones` (WPF), con soporte multioperador y despliegue estable en produccion.

## Estado actual detectado (resumen rapido)
- [x] Botonera web base por modulo con `PREPARA/ENTRA/ACTUALIZA/SALE/RESET`.
- [x] Bloqueo de modulos y permisos por operador configurables.
- [x] Consulta de snapshots con modos `Circunscripcion`, `MasVotadosAutonomias`, `MasVotadosProvincias`, `PartidoAutonomias`, `PartidoProvincias`.
- [x] Overlay de sondeo por medio (`medio_partido`) implementado.
- [x] Envio TCP a IPF/Prime y composicion de senales base.
- [ ] Paridad completa de escenas, comandos, reglas de negocio y herramientas operativas de WPF.

## Fase 0 - Inventario y criterio de cierre
- [ ] Crear matriz de paridad funcional `WPF -> Web` (modulo, escena, accion, comando, parametros, salida esperada).
- [ ] Definir alcance operativo minimo de salida (que debe funcionar si o si en noche electoral).
- [ ] Acordar lista de pruebas UAT por realizacion/operadores.

## Fase 1 - Paridad de datos y contexto electoral
- [x] Implementar seleccion manual de avance (`1,2,3,Final`) en Web y propagarla a snapshot/CSV/senales.
- [x] Implementar soporte de elecciones simultaneas por slot (`numEleccionesSimultaneas` + seleccion de `DB1/DB2/DB3` en web).
- [x] Implementar mapeo completo de `conexionDefault1..3` (principal/reserva/local por eleccion) en backend web.
- [x] Implementar selector de origen BD en UI web (principal/reserva/local) y/o estrategia de failover equivalente.
- [x] Replicar comportamiento `regional` y `codigoRegionalBDx` en filtros de circunscripciones.
- [x] Verificar calculo de participacion/historicos/media por avance respecto a la logica de `CircunscripcionDTO`.
- [x] Revisar y completar campos de `BrainStormSnapshot` para paridad con `BrainStormDTO` legacy.

## Fase 2 - Paridad de exportacion de ficheros
- [x] Igualar formato CSV web al CSV legacy (cabeceras, columnas, orden y placeholders).
- [x] Implementar alineacion por plantilla de partidos (relleno de huecos por circunscripcion padre) validada con casos reales.
- [x] Incluir `NombreSondeo`, `Ultimo`, `Siguiente`, `Resto`, `Vot.Faltan` y campos historicos faltantes del legacy.
- [x] Implementar exportacion `Sedes.csv` equivalente cuando haya partido seleccionado.
- [x] Validar si se necesita tambien exportacion JSON (`Recuentos`) para flujos de Prime.

## Fase 3 - Paridad de senales por modulo
- [x] Auditar y corregir mapeo de escenas/eventos en `DefaultSignalComposer` frente a builders legacy (`FaldonMensajes`, `CartonMensajes`, `SuperfaldonMensajes`).
- [x] Corregir escena de cuenta atras (`CuentaAtras`) para incluir `TIMER_LENGTH`, `PonerEnInicio`, `Play` y segundos configurables.
- [x] Corregir escenas/eventos de `Superfaldon` (rutas legacy usan `Superfaldon/Oficial|Sondeo/...`, `ULTIMO/Entra`, etc.).
- [x] Corregir comandos de sedes en `Superfaldon` (`DespliegaSede`, `PreparaEncadenaSede`, `EncadenaSede`, `RepliegaSede`).
- [x] Añadir comandos faltantes de ticker: `TickerTDEntra`, `TickerTDActualiza`, `TickerTDSale`.
- [x] Añadir comandos faltantes de video: `VideoIn`, `VideoOut`, `VideoInTodos`, `VideoOutTodos`.
- [x] Añadir acciones de animacion/estado faltantes: `PrimerosResultados`, `AnimacionSondeo`, `DeSondeoAOficiales`, `CambioElecciones`.
- [x] Añadir control de rotulos Prime (`Subir/Bajar TD`, `Subir/Bajar Especiales`) si se mantiene en operacion.
- [x] Validar paridad de `Pactometro` y `Ultimo escano` con pruebas de acumulados/ancho/orden en ambos lados.

## Fase 4 - Paridad de UX operativa
- [x] Crear vistas operativas especializadas por modulo (no solo pagina unica) con atajos equivalentes a realizacion.
- [x] Replicar comportamiento de seleccion de grafico activo y filtros de tabla por grafico.
- [x] Implementar visualizacion de datos de apoyo (tabla partidos) con filtros equivalentes (`FICHAS`, `SEDES`, `CARTON PARTIDOS`, `ULTIMO ESCANO`).
- [x] Añadir interacciones de doble click/seleccion para acciones de `Sedes` y `Ultimo`.
- [x] Implementar pantalla de `Pactos` web con listas izquierda/derecha y altas/bajas de partidos.
- [x] Implementar botonera extra web (ticker/video/rotulos) equivalente a `Botonera.xaml`.

## Fase 5 - Configuracion operativa web
- [x] Migrar configuraciones de `config.ini` relevantes a `appsettings` + UI de administracion.
- [x] Añadir configuracion de rutas y fuentes de video (hasta 6) con modo `Directo/Pregrabado`.
- [x] Añadir configuracion de horas operativas (`horaAvance*`, `horaParticipacion*`) y uso en UI.
- [x] Añadir gestion de activacion IPF/Prime y estado de conexion visible.
- [x] Evaluar si se necesita endpoint secundario IPF2 en Web y, si aplica, implementarlo.

## Fase 6 - Actualizacion en vivo y concurrencia
- [x] Implementar escuchador de cambios de datos equivalente a `Escuchador` (polling o evento) para auto-actualizar cuando hay modulos en aire.
- [x] Definir modo manual vs autoactualizacion y controles para operador.
- [x] Hacer persistente/distribuido el lock de modulos (actualmente es en memoria de proceso).
- [x] Añadir auditoria de acciones por operador (quien, que, cuando, modulo, escena, payload).

## Fase 7 - Seguridad y operacion real
- [x] Sustituir identificacion por texto libre (`operatorId`) por autenticacion real (SSO/AD/OIDC o alternativa aprobada).
- [x] Endurecer autorizacion por rol/modulo/accion/comando.
- [x] Ocultar o desactivar paginas de plantilla (`Counter`, `Weather`) para entorno productivo.
- [x] Revisar manejo seguro de secretos (`Password` BD, endpoints) y estrategia de despliegue.

## Fase 8 - Calidad, pruebas y despliegue
- [ ] Crear tests de regresion de composicion de senales comparando output web vs output legacy para casos canonicos.
- [ ] Crear tests de integracion de snapshots con BD real y datos de sondeo por medio.
- [ ] Crear tests de extremo a extremo de operacion multiusuario con bloqueo de modulos.
- [ ] Ejecutar simulacro completo de noche electoral con checklist de GO/NO-GO.
- [ ] Definir plan de rollback (volver a WPF o modo mixto) y runbook de incidencias.

## Criterio de "Web Operativa"
- [ ] Todas las escenas/comandos usados en realizacion tienen paridad validada con la salida legacy.
- [ ] Todos los CSV requeridos por graficos se generan con formato esperado y datos correctos.
- [ ] Multioperador estable, con bloqueo persistente y trazabilidad.
- [ ] Pruebas UAT firmadas por operacion y simulacro completo superado.
