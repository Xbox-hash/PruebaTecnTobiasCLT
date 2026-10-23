# Prueba Técnica — API de Usuarios, Direcciones y Divisas

API REST desarrollada en **.NET 9** utilizando **Minimal API**, **Entity Framework Core con SQLite**, validaciones con **FluentValidation**, arquitectura **CQRS (Commands & Queries)** y procesamiento concurrente con **Task.WhenAll** e **IDbContextFactory**.

---

## Tecnologías y Versiones

- **.NET SDK**: 9.0
- **ASP.NET Core Minimal API**
- **Entity Framework Core**: 9.0 (Proveedor SQLite)
- **FluentValidation**: 12.2
- **Documentación**: Swagger / OpenAPI (Swashbuckle)
- **Base de datos**: SQLite (`usersapi.db`)

---

## 🚀 Cómo Correr el Proyecto

### 1. Clonar el repositorio
```bash
git clone <URL_DEL_REPOSITORIO>
cd PruebaTecnTobiasCLT
```

### 2. Restaurar dependencias y compilar
```bash
dotnet restore
dotnet build
```

### 3. Ejecutar la API
```bash
dotnet run --project src/UsersApi
```

Una vez levantada, la consola indicará los puertos de escucha (por defecto `http://localhost:5243` y `https://localhost:7106`).

### 4. Acceder a Swagger UI
Abrir en el navegador:
**`http://localhost:5243/swagger`** o **`https://localhost:7106/swagger`**

---

## 🗄️ Base de Datos y Migraciones (SQLite)

La API cuenta con **migración y seed automáticos al arrancar**. 
Al levantar el proyecto por primera vez, `Program.cs` utiliza `IDbContextFactory<AppDbContext>` para aplicar las migraciones pendientes y precargar las monedas iniciales (`PYG` y `USD`), por lo que **no es necesario ejecutar comandos manuales para inicializar la base de datos**.

Si se desea gestionar las migraciones manualmente:

```bash
# Aplicar migraciones a la base de datos
dotnet ef database update --project src/UsersApi

# Crear una nueva migración
dotnet ef migrations add <NombreMigracion> --project src/UsersApi
```

---

## Seguridad y API Key

Toda la API (a excepción de las rutas de Swagger) se encuentra protegida por el header obligatorio:
```http
X-API-KEY: 2004
```

- **Configuración**: Se define en `appsettings.json` bajo la clave `"ApiKey:Value"`.
- **Comportamiento**: Si el header está ausente o el valor no coincide, la API retorna inmediatamente:
  ```json
  // HTTP 401 Unauthorized
  {
    "error": "API key invalida o ausente"
  }
  ```

### Autenticación en Swagger UI:
1. Hacer clic en el botón verde **Authorize** (arriba a la derecha).
2. Ingresar el valor: `2004`.
3. Hacer clic en **Authorize** y luego **Close**.

---

## Endpoints Implementados (100% de los requisitos)

### Usuarios (`Users`)
- `POST /users`: Crear usuario con validación de nombre no vacío y correo electrónico válido con extensión (ej: `.com`, `.com.py`). Retorna `201 Created` o `409 Conflict` si el correo ya existe.
- `GET /users`: Listado de todos los usuarios usando `AsNoTracking()` y proyección DTO. Soporta filtro opcional `?isActive=true|false`.
- `GET /users/{id}`: Obtener usuario por ID (`200 OK` o `404 Not Found`).
- `PUT /users/{id}`: Actualizar `name`, `email` e `isActive` (`200 OK`, `404 Not Found` o `409 Conflict` si el nuevo correo pertenece a otro usuario).
- `DELETE /users/{id}`: Eliminar usuario (`204 No Content` o `404 Not Found`) con eliminación en cascada de sus direcciones asociadas.
- `POST /users/bulk`: **Carga masiva en paralelo**.

### Direcciones (`Addresses` - Relación 1:N)
- `POST /users/{userId}/addresses`: Crear dirección para un usuario específico. Valida existencia del usuario (`404 Not Found` si no existe) y datos obligatorios (`street`, `city`, `country`). Retorna `201 Created`.
- `GET /users/{userId}/addresses`: Listar direcciones de un usuario. Valida existencia del usuario (`404 Not Found` si no existe) y devuelve `200 OK` con `[]` si no tiene direcciones registradas.
- `PUT /addresses/{id}`: Modificar dirección existente (`200 OK` o `404 Not Found`).
- `DELETE /addresses/{id}`: Eliminar dirección (`204 No Content` o `404 Not Found`).

### Divisas y Conversión (`Currencies`)
- `GET /currencies`: Listar monedas disponibles.
- `POST /currencies`: Crear moneda (`code` único, `name`, `rateToBase > 0`). Retorna `201 Created` o `409 Conflict`.
- `POST /currency/convert`: Conversión entre dos divisas:
  - Busca la moneda de origen y destino de forma **concurrente en paralelo usando `Task.WhenAll`**.
  - Realiza los cálculos monetarios estrictamente en **`decimal`**:
    - $\text{montoBase} = \text{amount} \times \text{from.RateToBase}$
    - $\text{convertedAmount} = \frac{\text{montoBase}}{\text{to.RateToBase}}$
  - Retorna `200 OK` con el desglose exacto o `404 Not Found` si alguna moneda no existe.

---

## Puntos de Evaluación Obligatoria Destacados

### 1. Paralelismo con `Task.WhenAll`
Implementado en:
- `POST /users/bulk`: Procesa el lote de usuarios de forma paralela y no secuencial.
- `POST /currency/convert`: Consulta las dos monedas (`from` y `to`) concurrentemente.

### 2. Uso estricto de `IDbContextFactory<AppDbContext>`
`DbContext` no es thread-safe. Para evitar condiciones de carrera (*race conditions*) y excepciones de contexto en operaciones paralelas, se registró y utilizó `IDbContextFactory<AppDbContext>`. Cada tarea asíncrona instancia y destruye su propio contexto independiente con `await using var db = await factory.CreateDbContextAsync(ct)`.

### 3. Carga Masiva Resiliente con Throttling
El endpoint `POST /users/bulk` incluye:
- Validación individual por registro con FluentValidation sin abortar el lote.
- Control de concurrencia mediante `SemaphoreSlim(8)` para prevenir bloqueos de archivo en SQLite (*database is locked*).
- Respuesta detallada con cantidad de creados, fallidos y detalle de errores por índice.

---

## Archivos de Prueba para Carga Masiva

Ubicados en la carpeta `sample-data/`:
1. **`sample-data/bulk_users_test.json`**: Lote de usuarios válidos provisto en el enunciado oficial para probar el caso de éxito total.
2. **`sample-data/bulk_users_with_errors.json`**: Lote mixto con usuarios válidos, nombres vacíos, formatos de correo inválidos y correos duplicados para comprobar el reporte granular de fallos.
