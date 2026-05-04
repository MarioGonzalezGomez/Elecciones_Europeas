# Elecciones Europeas - Operación de Gráficos Electorales

Aplicación de operación para la noche electoral, diseñada para controlar y actualizar gráficos en tiempo real (faldones, cartones y superfaldones) a partir de datos oficiales y de sondeo.

Este repositorio incluye dos aplicaciones complementarias:

- `Elecciones` (WPF, escritorio): herramienta principal de control de realizaci�n.
- `EleccionesWeb` (Blazor Server): interfaz web para operaci�n remota/multioperador.

## Qué hace este proyecto

- Consulta datos electorales desde MySQL (con soporte de conexión principal/reserva/local).
- Permite operar distintos módulos gráficos por pantalla:
- `FALDÓN`
- `CARTÓN`
- `SUPERFALDÓN`
- Genera y envía señales/comandos a motores gráficos (IPF y Prime) por TCP.
- Gestiona modos `Oficiales` y `Sondeo`, incluyendo variaciones por medio de sondeo.
- Exporta datos auxiliares (CSV/JSON) para flujos de producción.
- Mantiene actualización en vivo y automatiza transiciones de entrada/actualización/salida de gráficos.

## Estructura del repositorio

- `src/` lógica principal de la app de escritorio (controladores, servicios, repositorios, builders de mensajes).
- `view/` ventanas WPF (operación, configuración, pactos, botonera, splash).
- `styles/` temas visuales de escritorio.
- `EleccionesWeb/` solución web independiente para operación distribuida.

## Arquitectura (alto nivel)

- Capa de acceso a datos con Entity Framework + MySQL.
- Capa de dominio y servicios para construir snapshots electorales y reglas de operación.
- Capa de mensajería con builders especializados (`FaldonMensajes`, `CartonMensajes`, `SuperfaldonMensajes`) para componer señales IPF/Prime.
- Capa de interfaz (WPF/Blazor) para control operativo en directo.

## Requisitos

- .NET 8 SDK
- Windows (para la app WPF de escritorio)
- Acceso a base de datos MySQL con el esquema electoral correspondiente

## Configuraci�n

La aplicación de escritorio usa `config.ini` (copiado al output en build), donde se definen:

- Endpoints de gráficos (`IPF`, `Prime`)
- Parámetros de base de datos (host, puerto, BDs, usuario)
- Ruta de exportación de archivos
- Tipo de elección (nacional/autonómica), tema, y opciones de operación

La aplicación web usa `EleccionesWeb/src/Elecciones.Web/appsettings.json` para:

- Base de datos
- Endpoints TCP de gráficos
- Carpeta de CSV
- Permisos de operador por módulo

## Ejecución local

### Escritorio (WPF)

```bash
dotnet build Elecciones.sln
dotnet run --project Elecciones.csproj
```

### Web (Blazor Server)

```bash
dotnet build EleccionesWeb/EleccionesWeb.sln
dotnet run --project EleccionesWeb/src/Elecciones.Web/Elecciones.Web.csproj
```