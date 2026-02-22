# API Gateway - Microservicios

API Gateway construido con .NET 8 y YARP (Yet Another Reverse Proxy) que proporciona enrutamiento, autenticación y autorización centralizada para los microservicios.

## 🚀 Características

- **Autenticación JWT** con Keycloak
- **Generación dinámica** de rutas/clusters vía código (sin `ReverseProxy` en `appsettings.json`).
- **Proxy reverso** con YARP
- **Contenedorización** con Docker
- **Políticas** de autorización reutilizables (`AuthPolicies`).
- **Listo para ampliarse** con nuevos microservicios.

## 📋 Requisitos Previos

- [Docker](https://www.docker.com/get-started) (versión 20.10 o superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versión 2.0 o superior)

## 📁 Estructura

```bash
nur-tricenter-api-gateway/
 ├── ApiGateway/
 │   ├── Configuration/
 │   │   ├── MicroserviceConfig.cs
 │   │   ├── MicroserviceRoute.cs
 │   │   ├── MicroserviceRegistry.cs
 │   │   ├── ReverseProxyConfigBuilder.cs
 │   │   └── Microservices/
 │   │       ├── KeycloakConfig.cs
 │   │       └── LogisticsConfig.cs
 │   ├── Security/AuthPolicies.cs
 │   ├── Program.cs
 │   ├── appsettings.json
 │   └── Dockerfile
 └── docker-compose.yml
```

## ⚙️ Configuración dinámica

1. `MicroserviceConfig` define nombre, cluster, base URL y rutas.
2. Cada microservicio implementa su clase (`LogisticsConfig`, `KeycloakConfig`, etc.).
3. `MicroserviceRegistry` registra las configuraciones.
4. `ReverseProxyConfigBuilder` recorre el registro y genera la sección `ReverseProxy` en memoria.
5. `Program.cs` llama al builder antes de `AddReverseProxy()`.

Para agregar otro microservicio:
1. Crear `Configuration/Microservices/NuevoServicioConfig.cs` heredando de `MicroserviceConfig`.
2. Definir rutas y políticas usando `AuthPolicies`.
3. Registrar la clase en `MicroserviceRegistry`.
4. Reiniciar el gateway.

## 🔐 Autenticación y Políticas

```bash
| Política           | Roles requeridos          | Descripción                    |
|--------------------|---------------------------|--------------------------------|
| `Authenticated`    | Usuario autenticado       | Acceso básico autenticado      |
| `AdminOnly`        | `admin`                   | Operaciones administrativas    |
| `DriverOnly`       | `driver`                  | Funciones del conductor        |
| `LogisticsAccess`  | `logistics`, `admin`      | Lecturas del dominio logística |
```

`AuthPolicies.Register(options)` se invoca en `Program.cs` para registrar todas las políticas en ASP.NET Core.

### Keycloak

El API Gateway está configurado para autenticarse contra un servidor Keycloak externo:

- **URL**: `http://154.38.180.80:8080`
- **Realm**: `group3realm`

## 🔧 Configuración mínima (`appsettings.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
Todo el bloque `ReverseProxy` se genera en tiempo de ejecución. Agregar o modificar microservicios solo requiere tocar su clase de configuración y el registro.

## 🐳 Ejecutar con Docker Compose

### 1. Construir la imagen

Desde el directorio raíz del proyecto (`modulo-6-actividad-1-juanmmc/`):

```bash
docker-compose build
```

### 2. Iniciar el contenedor

```bash
docker-compose up -d
```

### 3. Verificar que el contenedor está corriendo

```bash
docker ps -f "apigateway"
```

Debería ver:

```bash
CONTAINER ID   IMAGE                                     COMMAND                  CREATED              STATUS              PORTS                                         NAMES
------------   modulo-6-actividad-1-juanmmc-apigateway   "dotnet ApiGateway.d…"   About a minute ago   Up About a minute   0.0.0.0:5000->8080/tcp, [::]:5000->8080/tcp   apigateway
```

### 4. Ver los logs

```bash
docker-compose logs -f apigateway
```

### 5. Detener el contenedor

```bash
docker-compose down
```

## 🌐 Acceso al API Gateway

Una vez iniciado el contenedor, el API Gateway estará disponible en: http://localhost:5000

### Endpoints de prueba:

- **Login**: `http://localhost:5000/api/login`
- **Users**: `http://localhost:5000/api/logout` (requiere token)
- **Posts**: `http://localhost:5000/api/posts` (requiere token)

## 🔐 Autenticación (Keycloak)

- **Grant Type**: `password`
- **Client ID**: `group3app`
- **Client Secret**: `pS9x84Qm0FkOJVrueg5OTtNYCWCAGtEp`
- **Username**: `juanmurielc`
- **Password**: `123456`

### Obtener Token (Login)

```bash
curl -X POST http://localhost:5000/api/login
  -H "Content-Type: application/x-www-form-urlencoded" 
  -d "grant_type=password" 
  -d "client_id=group3app" 
  -d "client_secret=pS9x84Qm0FkOJVrueg5OTtNYCWCAGtEp"
  -d "username=juanmurielc" 
  -d "password=123456"
```

O también de la siguiente manera en PowerShell de Windows:

```bash
Invoke-WebRequest -Uri "http://localhost:5000/api/login" `
  -Method POST `
  -Headers @{"Content-Type"="application/x-www-form-urlencoded"} `
  -Body "grant_type=password&client_id=group3app&client_secret=pS9x84Qm0FkOJVrueg5OTtNYCWCAGtEp&username=juanmurielc&password=123456"
```

**Respuesta:**

```bash
{ "access_token": "eyJhbGci...", "refresh_token": "eyJhbGci...", "expires_in": 300, "token_type": "Bearer" }
```

### Usar el Token

```bash
curl -X GET http://localhost:5000/logistics/api/Driver/getDriver
  -H "Authorization: Bearer eyJhbGci..."
```

O también de la siguiente manera en PowerShell de Windows:

```bash
Invoke-WebRequest -Uri "http://localhost:5000/logistics/api/Driver/getDriver" `
  -Method GET `
  -Headers @{"Authorization"="Bearer eyJhbGci..."}
```