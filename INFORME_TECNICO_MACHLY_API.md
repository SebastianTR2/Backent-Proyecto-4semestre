# 📋 INFORME TÉCNICO COMPLETO - MACHLY.API

## 🎯 Resumen Ejecutivo

**Machly.Api** es una API REST desarrollada en **ASP.NET Core 8.0** que proporciona servicios para un sistema de alquiler de maquinaria agrícola. Utiliza **MongoDB** como base de datos NoSQL y **JWT** para autenticación y autorización. La API implementa un sistema de roles (ADMIN, PROVIDER, RENTER) y gestiona máquinas, reservas y cálculos agronómicos.

---

## 📁 1. ESTRUCTURA DEL PROYECTO

### 1.1 Organización de Carpetas

```
Machly.Api/
├── Config/                    # Configuraciones
│   └── MongoDbSettings.cs
├── Controllers/               # Controladores API
│   ├── AgroController.cs
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   ├── MachinesController.cs
│   └── WeatherForecastController.cs
├── DTOs/                      # Data Transfer Objects
│   ├── AgroCalculateRequest.cs
│   ├── BookingCreateRequest.cs
│   ├── LoginRequest.cs
│   └── RegisterRequest.cs
├── Models/                    # Modelos de dominio
│   ├── Booking.cs
│   ├── Machine.cs
│   └── User.cs
├── Repositories/              # Capa de acceso a datos
│   ├── BookingRepository.cs
│   ├── MachineRepository.cs
│   ├── MongoDbContext.cs
│   └── UserRepository.cs
├── Services/                   # Lógica de negocio
│   ├── AuthService.cs
│   ├── BookingService.cs
│   └── MachineService.cs
├── Utils/                      # Utilidades
│   ├── JwtHelper.cs
│   └── PasswordHasher.cs
├── Program.cs                  # Punto de entrada
├── appsettings.json           # Configuración
└── Machly.Api.csproj          # Archivo de proyecto
```

### 1.2 Tecnologías y Dependencias

**Framework:** .NET 8.0

**Paquetes NuGet:**
- `BCrypt.Net-Next` (v4.0.3) - Hashing de contraseñas
- `Microsoft.AspNetCore.Authentication.JwtBearer` (v8.0.0) - Autenticación JWT
- `MongoDB.Driver` (v3.5.1) - Driver de MongoDB
- `Swashbuckle.AspNetCore` (v6.6.2) - Documentación Swagger

---

## 🗄️ 2. MODELOS DE DOMINIO

### 2.1 User (Usuario)

**Ubicación:** `Models/User.cs`

**Descripción:** Representa un usuario del sistema con sus credenciales y rol.

```csharp
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }              // ID único MongoDB

    public string Name { get; set; }              // Nombre completo
    public string Email { get; set; }             // Email (único)
    public string PasswordHash { get; set; }      // Hash BCrypt de la contraseña
    public string Role { get; set; }              // ADMIN | PROVIDER | RENTER
}
```

**Colección MongoDB:** `users`

**Roles Disponibles:**
- `ADMIN` - Administrador del sistema
- `PROVIDER` - Proveedor de maquinaria
- `RENTER` - Arrendatario/cliente

**Índices:**
- **Email:** No hay índice explícito definido en el código, pero se recomienda crear un índice único en `Email` para búsquedas rápidas.

---

### 2.2 Machine (Máquina)

**Ubicación:** `Models/Machine.cs`

**Descripción:** Representa una máquina agrícola disponible para alquiler.

```csharp
public class Machine
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }              // ID único MongoDB

    public string ProviderId { get; set; }       // ID del proveedor (ObjectId como string)
    public string Title { get; set; }            // Título/nombre de la máquina
    public string Description { get; set; }      // Descripción detallada
    public decimal PricePerDay { get; set; }     // Precio por día en Bs

    public double Lat { get; set; }              // Latitud (geolocalización)
    public double Lng { get; set; }              // Longitud (geolocalización)

    public List<string> Photos { get; set; } = new();  // URLs de fotos
}
```

**Colección MongoDB:** `machines`

**Campos de Geolocalización:**
- `Lat` (Latitud) y `Lng` (Longitud) están presentes pero **NO hay índice geoespacial 2dsphere** implementado en el código actual.
- Para habilitar búsquedas con `NearSphere`, se requiere crear un índice compuesto en `{ Lat: 1, Lng: 1 }` con tipo `2dsphere`.

**Índices Recomendados:**
- `ProviderId` - Para filtrar máquinas por proveedor
- `{ Lat: "2dsphere", Lng: "2dsphere" }` - Para búsquedas geográficas (NO implementado)

---

### 2.3 Booking (Reserva)

**Ubicación:** `Models/Booking.cs`

**Descripción:** Representa una reserva de máquina por parte de un arrendatario.

```csharp
public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }              // ID único MongoDB

    [BsonRepresentation(BsonType.ObjectId)]
    public string MachineId { get; set; }        // ID de la máquina reservada

    [BsonRepresentation(BsonType.String)]
    public string RenterId { get; set; }         // ID del arrendatario (String, no ObjectId)

    public DateTime Start { get; set; }           // Fecha/hora de inicio
    public DateTime End { get; set; }            // Fecha/hora de fin

    public string Status { get; set; } = "CONFIRMED";  // Estado: CONFIRMED, PENDING, CANCELLED, COMPLETED
    public decimal TotalPrice { get; set; }      // Precio total calculado
}
```

**Colección MongoDB:** `bookings`

**Estados Posibles:**
- `CONFIRMED` - Reserva confirmada (valor por defecto)
- `PENDING` - Pendiente de confirmación
- `CANCELLED` - Cancelada
- `COMPLETED` - Completada

**Nota Importante:** `RenterId` se almacena como `String` en lugar de `ObjectId`, lo que permite flexibilidad pero requiere consistencia en el formato.

**Índices Recomendados:**
- `MachineId` - Para buscar reservas de una máquina
- `RenterId` - Para buscar reservas de un usuario (ya usado en consultas)
- `{ MachineId: 1, Start: 1, End: 1 }` - Para validar disponibilidad (NO implementado)

**Funcionalidades NO Implementadas:**
- ❌ Check-in / Check-out con fotos
- ❌ Reseñas (Reviews)
- ❌ Historial detallado
- ❌ Notificaciones

---

## 📦 3. DTOs (DATA TRANSFER OBJECTS)

### 3.1 RegisterRequest

**Ubicación:** `DTOs/RegisterRequest.cs`

**Uso:** Endpoint de registro de usuarios.

```csharp
public class RegisterRequest
{
    public string Name { get; set; }      // Nombre completo
    public string Email { get; set; }     // Email (debe ser único)
    public string Password { get; set; }  // Contraseña en texto plano
    public string Role { get; set; }      // PROVIDER | RENTER (no ADMIN por seguridad)
}
```

**Validaciones:**
- Email debe ser único (validado en `AuthService`)
- Role debe ser `PROVIDER` o `RENTER` (no se valida explícitamente en el código)

**Ejemplo Request:**
```json
{
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "password": "MiPassword123",
  "role": "RENTER"
}
```

---

### 3.2 LoginRequest

**Ubicación:** `DTOs/LoginRequest.cs`

**Uso:** Endpoint de inicio de sesión.

```csharp
public class LoginRequest
{
    public string Email { get; set; }     // Email del usuario
    public string Password { get; set; }  // Contraseña en texto plano
}
```

**Ejemplo Request:**
```json
{
  "email": "juan@example.com",
  "password": "MiPassword123"
}
```

---

### 3.3 BookingCreateRequest

**Ubicación:** `DTOs/BookingCreateRequest.cs`

**Uso:** Crear una nueva reserva.

```csharp
public class BookingCreateRequest
{
    public string MachineId { get; set; }  // ID de la máquina (ObjectId)
    public string RenterId { get; set; }   // ID del arrendatario
    public DateTime Start { get; set; }   // Fecha/hora inicio
    public DateTime End { get; set; }     // Fecha/hora fin
}
```

**Validaciones:**
- `MachineId` debe existir (validado en `BookingService`)
- `End` debe ser posterior a `Start` (no validado explícitamente)
- No se valida disponibilidad de la máquina en el rango de fechas

**Ejemplo Request:**
```json
{
  "machineId": "507f1f77bcf86cd799439011",
  "renterId": "507f191e810c19729de860ea",
  "start": "2024-12-01T08:00:00Z",
  "end": "2024-12-05T18:00:00Z"
}
```

---

### 3.4 AgroCalculateRequest

**Ubicación:** `DTOs/AgroCalculateRequest.cs`

**Uso:** Calcular precio de servicios agronómicos.

```csharp
public class AgroCalculateRequest
{
    public double? Hectareas { get; set; }   // Hectáreas a trabajar (opcional)
    public double? Toneladas { get; set; }   // Toneladas a transportar (opcional)
    public double? Km { get; set; }          // Kilómetros a transportar (opcional)
}
```

**Nota:** Todos los campos son opcionales (`double?`). El cálculo se realiza solo con los valores proporcionados.

**Ejemplo Request:**
```json
{
  "hectareas": 10.5,
  "toneladas": 25.0,
  "km": 50.0
}
```

---

## 🗃️ 4. REPOSITORIOS

### 4.1 MongoDbContext

**Ubicación:** `Repositories/MongoDbContext.cs`

**Descripción:** Contexto de base de datos MongoDB. Singleton que gestiona la conexión.

```csharp
public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        Database.GetCollection<T>(name);
}
```

**Configuración:**
- **ConnectionString:** `mongodb://localhost:27017/` (desde `appsettings.json`)
- **DatabaseName:** `machly_db`
- **Lifetime:** Singleton (una instancia para toda la aplicación)

---

### 4.2 UserRepository

**Ubicación:** `Repositories/UserRepository.cs`

**Descripción:** Acceso a datos de usuarios.

**Métodos:**

#### `GetByEmailAsync(string email)`
- **Retorna:** `User?` - Usuario encontrado o `null`
- **Uso:** Búsqueda por email (login, validación de duplicados)

#### `GetByIdAsync(string id)`
- **Retorna:** `User?` - Usuario encontrado o `null`
- **Uso:** Obtener usuario por ID

#### `CreateAsync(User user)`
- **Retorna:** `Task` (void)
- **Uso:** Crear nuevo usuario

**Colección:** `users`

**Índices:** No hay índices explícitos en el código. Se recomienda:
- Índice único en `Email` para búsquedas rápidas

---

### 4.3 MachineRepository

**Ubicación:** `Repositories/MachineRepository.cs`

**Descripción:** Acceso a datos de máquinas.

**Métodos:**

#### `GetAllAsync()`
- **Retorna:** `List<Machine>` - Todas las máquinas
- **Uso:** Listar todas las máquinas disponibles

#### `GetByIdAsync(string id)`
- **Retorna:** `Machine?` - Máquina encontrada o `null`
- **Uso:** Obtener máquina por ID
- **Nota:** Convierte el string ID a `ObjectId` para la búsqueda

#### `CreateAsync(Machine machine)`
- **Retorna:** `Task` (void)
- **Uso:** Crear nueva máquina

**Colección:** `machines`

**Índices:** No hay índices explícitos en el código. Se recomienda:
- Índice en `ProviderId` para filtrar por proveedor
- Índice geoespacial `2dsphere` en `{ Lat: 1, Lng: 1 }` para búsquedas geográficas (NO implementado)

**Funcionalidades NO Implementadas:**
- ❌ Búsqueda por geolocalización (`NearSphere`)
- ❌ Filtrado por rango de precio
- ❌ Filtrado por proveedor
- ❌ Búsqueda por texto (título/descripción)

---

### 4.4 BookingRepository

**Ubicación:** `Repositories/BookingRepository.cs`

**Descripción:** Acceso a datos de reservas.

**Métodos:**

#### `CreateAsync(Booking booking)`
- **Retorna:** `Task` (void)
- **Uso:** Crear nueva reserva

#### `GetByUserAsync(string renterId)`
- **Retorna:** `List<Booking>` - Reservas del usuario
- **Uso:** Obtener historial de reservas de un arrendatario
- **Nota:** Busca por `RenterId` (string)

**Colección:** `bookings`

**Índices:** No hay índices explícitos en el código. Se recomienda:
- Índice en `RenterId` para búsquedas rápidas (ya usado)
- Índice en `MachineId` para buscar reservas de una máquina
- Índice compuesto `{ MachineId: 1, Start: 1, End: 1 }` para validar disponibilidad

**Funcionalidades NO Implementadas:**
- ❌ Obtener reservas por máquina
- ❌ Obtener reservas por proveedor
- ❌ Validar disponibilidad de máquina en rango de fechas
- ❌ Actualizar estado de reserva
- ❌ Cancelar reserva

---

## 🔧 5. SERVICIOS (LÓGICA DE NEGOCIO)

### 5.1 AuthService

**Ubicación:** `Services/AuthService.cs`

**Descripción:** Servicio de autenticación y registro.

**Dependencias:**
- `UserRepository` - Acceso a usuarios
- `JwtHelper` - Generación de tokens JWT

**Métodos:**

#### `RegisterAsync(RegisterRequest request)`
- **Retorna:** `Task<string?>` - Token JWT o `null` si el email ya existe
- **Lógica:**
  1. Verifica si el email ya existe
  2. Si existe, retorna `null`
  3. Crea nuevo usuario con contraseña hasheada (BCrypt)
  4. Guarda en MongoDB
  5. Genera y retorna token JWT

**Ejemplo de Uso:**
```csharp
var token = await authService.RegisterAsync(new RegisterRequest 
{ 
    Name = "Juan", 
    Email = "juan@example.com", 
    Password = "pass123", 
    Role = "RENTER" 
});
```

#### `LoginAsync(LoginRequest request)`
- **Retorna:** `Task<string?>` - Token JWT o `null` si credenciales inválidas
- **Lógica:**
  1. Busca usuario por email
  2. Si no existe, retorna `null`
  3. Verifica contraseña con BCrypt
  4. Si es incorrecta, retorna `null`
  5. Genera y retorna token JWT

**Ejemplo de Uso:**
```csharp
var token = await authService.LoginAsync(new LoginRequest 
{ 
    Email = "juan@example.com", 
    Password = "pass123" 
});
```

---

### 5.2 MachineService

**Ubicación:** `Services/MachineService.cs`

**Descripción:** Servicio de gestión de máquinas.

**Dependencias:**
- `MachineRepository` - Acceso a máquinas

**Métodos:**

#### `GetAllAsync()`
- **Retorna:** `Task<List<Machine>>` - Lista de todas las máquinas
- **Uso:** Listar máquinas disponibles

#### `GetByIdAsync(string id)`
- **Retorna:** `Task<Machine?>` - Máquina encontrada o `null`
- **Uso:** Obtener detalles de una máquina

#### `CreateAsync(Machine machine)`
- **Retorna:** `Task` (void)
- **Uso:** Crear nueva máquina
- **Nota:** No valida permisos (debería verificar que el usuario sea PROVIDER)

**Funcionalidades NO Implementadas:**
- ❌ Actualizar máquina
- ❌ Eliminar máquina
- ❌ Filtrar por proveedor
- ❌ Filtrar por geolocalización
- ❌ Filtrar por precio
- ❌ Búsqueda por texto

---

### 5.3 BookingService

**Ubicación:** `Services/BookingService.cs`

**Descripción:** Servicio de gestión de reservas.

**Dependencias:**
- `BookingRepository` - Acceso a reservas
- `MachineRepository` - Acceso a máquinas (para calcular precio)

**Métodos:**

#### `CreateAsync(BookingCreateRequest request)`
- **Retorna:** `Task<Booking?>` - Reserva creada o `null` si la máquina no existe
- **Lógica:**
  1. Busca la máquina por ID
  2. Si no existe, retorna `null`
  3. Calcula días de alquiler: `(End.Date - Start.Date).TotalDays`
  4. Si días < 1, establece días = 1 (mínimo 1 día)
  5. Calcula precio total: `PricePerDay * días`
  6. Crea reserva con estado `CONFIRMED`
  7. Guarda en MongoDB
  8. Retorna la reserva creada

**Validaciones NO Implementadas:**
- ❌ Verificar disponibilidad de la máquina en el rango de fechas
- ❌ Validar que `End` sea posterior a `Start`
- ❌ Validar que las fechas no sean pasadas
- ❌ Verificar permisos (solo RENTER puede crear reservas)

**Ejemplo de Uso:**
```csharp
var booking = await bookingService.CreateAsync(new BookingCreateRequest 
{ 
    MachineId = "507f1f77bcf86cd799439011",
    RenterId = "507f191e810c19729de860ea",
    Start = DateTime.Parse("2024-12-01"),
    End = DateTime.Parse("2024-12-05")
});
// Calcula: 4 días * PricePerDay
```

#### `GetByUserAsync(string renterId)`
- **Retorna:** `Task<List<Booking>>` - Lista de reservas del usuario
- **Uso:** Obtener historial de reservas

**Funcionalidades NO Implementadas:**
- ❌ Actualizar estado de reserva
- ❌ Cancelar reserva
- ❌ Check-in / Check-out
- ❌ Agregar fotos de check-in/check-out
- ❌ Obtener reservas por máquina
- ❌ Obtener reservas por proveedor

---

## 🎮 6. CONTROLADORES Y ENDPOINTS

### 6.1 AuthController

**Ruta Base:** `/auth`

**Autenticación:** No requiere autenticación

#### `POST /auth/register`

**Descripción:** Registra un nuevo usuario.

**Request Body:**
```json
{
  "name": "string",
  "email": "string",
  "password": "string",
  "role": "PROVIDER | RENTER"
}
```

**Response 200 OK:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response 400 Bad Request:**
```json
"Email already exists"
```

**Ejemplo cURL:**
```bash
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Juan Pérez",
    "email": "juan@example.com",
    "password": "MiPassword123",
    "role": "RENTER"
  }'
```

---

#### `POST /auth/login`

**Descripción:** Inicia sesión y obtiene token JWT.

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response 200 OK:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response 401 Unauthorized:**
```json
"Invalid credentials"
```

**Ejemplo cURL:**
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan@example.com",
    "password": "MiPassword123"
  }'
```

---

### 6.2 MachinesController

**Ruta Base:** `/machines`

**Autenticación:** No requiere autenticación (debería requerirla para POST)

#### `GET /machines`

**Descripción:** Obtiene todas las máquinas disponibles.

**Response 200 OK:**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "providerId": "000000000000000000000000",
    "title": "Tractor John Deere 5075E",
    "description": "Tractor agrícola de 75 HP",
    "pricePerDay": 1500.00,
    "lat": -17.3935,
    "lng": -66.1570,
    "photos": [
      "https://example.com/photo1.jpg",
      "https://example.com/photo2.jpg"
    ]
  }
]
```

**Ejemplo cURL:**
```bash
curl -X GET https://localhost:5001/machines
```

---

#### `GET /machines/{id}`

**Descripción:** Obtiene una máquina por ID.

**Parámetros:**
- `id` (path) - ID de la máquina (ObjectId)

**Response 200 OK:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "providerId": "000000000000000000000000",
  "title": "Tractor John Deere 5075E",
  "description": "Tractor agrícola de 75 HP",
  "pricePerDay": 1500.00,
  "lat": -17.3935,
  "lng": -66.1570,
  "photos": []
}
```

**Response 404 Not Found:**
```json
(empty body)
```

**Ejemplo cURL:**
```bash
curl -X GET https://localhost:5001/machines/507f1f77bcf86cd799439011
```

---

#### `POST /machines`

**Descripción:** Crea una nueva máquina.

**Autenticación:** No requiere autenticación (debería requerir rol PROVIDER)

**Request Body:**
```json
{
  "providerId": "string",
  "title": "string",
  "description": "string",
  "pricePerDay": 0.00,
  "lat": 0.0,
  "lng": 0.0,
  "photos": ["string"]
}
```

**Nota:** Si `providerId` está vacío, se asigna un valor mock: `"000000000000000000000000"`

**Response 200 OK:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "providerId": "000000000000000000000000",
  "title": "Tractor John Deere 5075E",
  "description": "Tractor agrícola de 75 HP",
  "pricePerDay": 1500.00,
  "lat": -17.3935,
  "lng": -66.1570,
  "photos": []
}
```

**Ejemplo cURL:**
```bash
curl -X POST https://localhost:5001/machines \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tractor John Deere 5075E",
    "description": "Tractor agrícola de 75 HP",
    "pricePerDay": 1500.00,
    "lat": -17.3935,
    "lng": -66.1570,
    "photos": []
  }'
```

**Funcionalidades NO Implementadas:**
- ❌ `PUT /machines/{id}` - Actualizar máquina
- ❌ `DELETE /machines/{id}` - Eliminar máquina
- ❌ `GET /machines?providerId={id}` - Filtrar por proveedor
- ❌ `GET /machines?lat={lat}&lng={lng}&radius={km}` - Filtrar por geolocalización
- ❌ `GET /machines?minPrice={min}&maxPrice={max}` - Filtrar por precio
- ❌ `GET /machines?search={text}` - Búsqueda por texto

---

### 6.3 BookingsController

**Ruta Base:** `/bookings`

**Autenticación:** No requiere autenticación (debería requerirla)

#### `POST /bookings`

**Descripción:** Crea una nueva reserva.

**Request Body:**
```json
{
  "machineId": "507f1f77bcf86cd799439011",
  "renterId": "507f191e810c19729de860ea",
  "start": "2024-12-01T08:00:00Z",
  "end": "2024-12-05T18:00:00Z"
}
```

**Response 200 OK:**
```json
{
  "id": "507f1f77bcf86cd799439012",
  "machineId": "507f1f77bcf86cd799439011",
  "renterId": "507f191e810c19729de860ea",
  "start": "2024-12-01T08:00:00Z",
  "end": "2024-12-05T18:00:00Z",
  "status": "CONFIRMED",
  "totalPrice": 6000.00
}
```

**Response 400 Bad Request:**
```json
"Machine not found"
```

**Ejemplo cURL:**
```bash
curl -X POST https://localhost:5001/bookings \
  -H "Content-Type: application/json" \
  -d '{
    "machineId": "507f1f77bcf86cd799439011",
    "renterId": "507f191e810c19729de860ea",
    "start": "2024-12-01T08:00:00Z",
    "end": "2024-12-05T18:00:00Z"
  }'
```

---

#### `GET /bookings/user/{renterId}`

**Descripción:** Obtiene todas las reservas de un usuario.

**Parámetros:**
- `renterId` (path) - ID del arrendatario

**Response 200 OK:**
```json
[
  {
    "id": "507f1f77bcf86cd799439012",
    "machineId": "507f1f77bcf86cd799439011",
    "renterId": "507f191e810c19729de860ea",
    "start": "2024-12-01T08:00:00Z",
    "end": "2024-12-05T18:00:00Z",
    "status": "CONFIRMED",
    "totalPrice": 6000.00
  }
]
```

**Ejemplo cURL:**
```bash
curl -X GET https://localhost:5001/bookings/user/507f191e810c19729de860ea
```

**Funcionalidades NO Implementadas:**
- ❌ `PUT /bookings/{id}/status` - Actualizar estado
- ❌ `POST /bookings/{id}/checkin` - Check-in con fotos
- ❌ `POST /bookings/{id}/checkout` - Check-out con fotos
- ❌ `GET /bookings/machine/{machineId}` - Reservas de una máquina
- ❌ `GET /bookings/provider/{providerId}` - Reservas de un proveedor
- ❌ `DELETE /bookings/{id}` - Cancelar reserva

---

### 6.4 AgroController

**Ruta Base:** `/agro`

**Autenticación:** No requiere autenticación

#### `POST /agro/calculate`

**Descripción:** Calcula el precio de servicios agronómicos basado en hectáreas, toneladas y kilómetros.

**Request Body:**
```json
{
  "hectareas": 10.5,
  "toneladas": 25.0,
  "km": 50.0
}
```

**Todos los campos son opcionales.** El cálculo se realiza solo con los valores proporcionados.

**Tarifas Mock (Sprint 1):**
- **Hectáreas:** 350 Bs/ha
- **Toneladas:** 45 Bs/ton
- **Transporte:** 0.9 Bs/km*ton (solo si hay toneladas y km)

**Response 200 OK:**
```json
{
  "total": 5425.0,
  "detalles": [
    "Hectáreas: 10.5 * 350 = 3675",
    "Toneladas: 25 * 45 = 1125",
    "Transporte: 50 km * 25 ton * 0.9 = 1125"
  ],
  "isMock": true
}
```

**Lógica de Cálculo:**
1. Si hay `hectareas`: `total += hectareas * 350`
2. Si hay `toneladas`: `total += toneladas * 45`
3. Si hay `km` Y `toneladas`: `total += km * toneladas * 0.9`

**Ejemplo cURL:**
```bash
curl -X POST https://localhost:5001/agro/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "hectareas": 10.5,
    "toneladas": 25.0,
    "km": 50.0
  }'
```

**Nota:** Este es un cálculo MOCK. En producción, las tarifas deberían venir de una configuración o base de datos (modelo `TariffAgro` NO implementado).

**Funcionalidades NO Implementadas:**
- ❌ Modelo `TariffAgro` para tarifas configurables
- ❌ Endpoint para gestionar tarifas (CRUD)
- ❌ Historial de cálculos
- ❌ Integración con reservas agronómicas

---

### 6.5 WeatherForecastController

**Ruta Base:** `/WeatherForecast`

**Descripción:** Controlador de ejemplo generado por la plantilla de ASP.NET Core. **NO es parte de la funcionalidad de Machly.**

**Endpoint:** `GET /WeatherForecast`

**Uso:** Solo para pruebas/demostración. Puede eliminarse en producción.

---

## ⚙️ 7. CONFIGURACIONES

### 7.1 MongoDbSettings

**Ubicación:** `Config/MongoDbSettings.cs`

```csharp
public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}
```

**Configuración en `appsettings.json`:**
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017/",
    "DatabaseName": "machly_db"
  }
}
```

**Uso:** Configuración de conexión a MongoDB mediante `IOptions<MongoDbSettings>`.

---

### 7.2 JwtSettings

**Configuración en `appsettings.json`:**
```json
{
  "JwtSettings": {
    "Key": "S3guridadMaximaClaveSecretaMachly2024_ProyectoFinal"
  }
}
```

**⚠️ IMPORTANTE:** Esta clave debe ser cambiada en producción y almacenada de forma segura (variables de entorno, Azure Key Vault, etc.).

**Configuración JWT en `Program.cs`:**
- **Algoritmo:** HMAC SHA256
- **Expiración:** 7 días
- **Claims incluidos:**
  - `id` - ID del usuario
  - `email` - Email del usuario
  - `role` - Rol del usuario (ADMIN, PROVIDER, RENTER)
- **Validación:**
  - No valida Issuer
  - No valida Audience
  - Valida la firma del token

---

### 7.3 Program.cs - Configuración de Servicios

**Lifetime de Servicios:**

| Servicio | Lifetime | Razón |
|----------|----------|-------|
| `MongoDbContext` | Singleton | Una conexión para toda la app |
| `JwtHelper` | Singleton | Helper sin estado |
| `AuthService` | Scoped | Por request HTTP |
| `MachineService` | Scoped | Por request HTTP |
| `BookingService` | Scoped | Por request HTTP |
| `UserRepository` | Scoped | Por request HTTP |
| `MachineRepository` | Scoped | Por request HTTP |
| `BookingRepository` | Scoped | Por request HTTP |

**CORS:**
- Política `AllowAll` configurada
- Permite cualquier origen, método y header
- ⚠️ En producción, restringir a dominios específicos

**Swagger:**
- Habilitado en desarrollo
- Ruta: `/` (raíz)
- Endpoint JSON: `/swagger/v1/swagger.json`

---

## 🔐 8. SEGURIDAD Y AUTENTICACIÓN

### 8.1 Autenticación JWT

**Implementación:** `Utils/JwtHelper.cs`

**Generación de Token:**
```csharp
public string GenerateToken(User user)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim("id", user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**Uso del Token:**
- Incluir en header: `Authorization: Bearer {token}`
- El middleware JWT valida automáticamente el token en requests autenticados

**⚠️ Problemas de Seguridad Actuales:**
1. **No hay autorización por roles:** Los endpoints no usan `[Authorize(Roles = "PROVIDER")]`
2. **Endpoints públicos:** `POST /machines` y `POST /bookings` deberían requerir autenticación
3. **Clave JWT en código:** Debe estar en variables de entorno
4. **CORS abierto:** Permite cualquier origen

---

### 8.2 Hashing de Contraseñas

**Implementación:** `Utils/PasswordHasher.cs`

**Uso de BCrypt:**
```csharp
public static string Hash(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

public static bool Verify(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

**Seguridad:**
- ✅ Contraseñas hasheadas con BCrypt (sal automático)
- ✅ Nunca se almacenan en texto plano
- ✅ Verificación segura en login

---

### 8.3 Autorización por Roles

**Estado Actual:** ❌ NO IMPLEMENTADO

**Recomendación:** Agregar atributos `[Authorize]` en controladores:

```csharp
[Authorize(Roles = "PROVIDER")]
[HttpPost]
public async Task<IActionResult> Create([FromBody] Machine machine)
{
    // Solo PROVIDER puede crear máquinas
}

[Authorize(Roles = "RENTER")]
[HttpPost]
public async Task<IActionResult> Create([FromBody] BookingCreateRequest request)
{
    // Solo RENTER puede crear reservas
}
```

---

## 📊 9. REGLAS DE NEGOCIO IMPLEMENTADAS

### 9.1 ✅ Registro/Login JWT

- **Registro:** Valida email único, hashea contraseña, genera token
- **Login:** Verifica credenciales, genera token
- **Token:** Expira en 7 días, incluye ID, email y rol

---

### 9.2 ⚠️ Seguridad por Roles

- **Modelo:** Roles definidos (ADMIN, PROVIDER, RENTER)
- **JWT:** Incluye rol en claims
- **Autorización:** ❌ NO implementada en endpoints

---

### 9.3 ✅ Reservas Estándar

- **Creación:** Valida existencia de máquina, calcula precio por días
- **Cálculo:** `TotalPrice = PricePerDay * días` (mínimo 1 día)
- **Estado:** Por defecto `CONFIRMED`
- **Historial:** Endpoint para obtener reservas por usuario

---

### 9.4 ✅ Reservas Agronómicas (Cálculo)

- **Endpoint:** `/agro/calculate`
- **Cálculo:** Hectáreas, toneladas y transporte
- **Tarifas:** Mock (350 Bs/ha, 45 Bs/ton, 0.9 Bs/km*ton)
- **Estado:** ❌ NO se integra con reservas (solo cálculo)

---

### 9.5 ✅ Cálculo Agronómico

- **Hectáreas:** `hectareas * 350`
- **Toneladas:** `toneladas * 45`
- **Transporte:** `km * toneladas * 0.9` (solo si hay toneladas)

---

### 9.6 ❌ Check-in / Check-out con Fotos

**NO IMPLEMENTADO**

**Recomendación:**
- Agregar campos a `Booking`:
  ```csharp
  public DateTime? CheckInDate { get; set; }
  public List<string> CheckInPhotos { get; set; } = new();
  public DateTime? CheckOutDate { get; set; }
  public List<string> CheckOutPhotos { get; set; } = new();
  ```
- Endpoints:
  - `POST /bookings/{id}/checkin` - Subir fotos de check-in
  - `POST /bookings/{id}/checkout` - Subir fotos de check-out

---

### 9.7 ❌ Reseñas (Reviews)

**NO IMPLEMENTADO**

**Recomendación:**
- Crear modelo `Review`:
  ```csharp
  public class Review
  {
      public string BookingId { get; set; }
      public string RenterId { get; set; }
      public string MachineId { get; set; }
      public int Rating { get; set; } // 1-5
      public string Comment { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```
- Endpoint: `POST /bookings/{id}/review`

---

### 9.8 ⚠️ Historial

**Estado Parcial:**
- ✅ Endpoint `GET /bookings/user/{renterId}` obtiene reservas del usuario
- ❌ No hay filtros (por fecha, estado, máquina)
- ❌ No hay paginación
- ❌ No incluye detalles de máquina (solo IDs)

**Recomendación:**
- Agregar filtros: `?status=COMPLETED&startDate=...&endDate=...`
- Agregar paginación: `?page=1&pageSize=10`
- Incluir datos de máquina en respuesta (join o proyección)

---

### 9.9 ❌ Notificaciones Mock

**NO IMPLEMENTADO**

**Recomendación:**
- Crear modelo `Notification`:
  ```csharp
  public class Notification
  {
      public string UserId { get; set; }
      public string Title { get; set; }
      public string Message { get; set; }
      public string Type { get; set; } // BOOKING_CREATED, BOOKING_CONFIRMED, etc.
      public DateTime CreatedAt { get; set; }
      public bool IsRead { get; set; }
  }
  ```
- Endpoints:
  - `GET /notifications/user/{userId}` - Obtener notificaciones
  - `PUT /notifications/{id}/read` - Marcar como leída

---

### 9.10 ❌ Filtros con Geolocalización (NearSphere)

**NO IMPLEMENTADO**

**Recomendación:**
1. **Crear índice geoespacial en MongoDB:**
   ```javascript
   db.machines.createIndex({ "location": "2dsphere" })
   ```
2. **Modificar modelo `Machine`:**
   ```csharp
   public GeoJsonPoint<GeoJson2DCoordinates> Location { get; set; }
   ```
3. **Agregar método en `MachineRepository`:**
   ```csharp
   public async Task<List<Machine>> GetNearAsync(double lat, double lng, double radiusKm)
   {
       var point = GeoJson.Point(GeoJson.Geographic(lng, lat));
       var filter = Builders<Machine>.Filter.NearSphere(
           m => m.Location, point, radiusKm * 1000); // radiusKm en metros
       return await _machines.Find(filter).ToListAsync();
   }
   ```
4. **Endpoint:** `GET /machines?lat={lat}&lng={lng}&radius={km}`

---

## 🔍 10. ÍNDICES DE MONGODB

### 10.1 Índices Actuales

**⚠️ NINGUNO EXPLÍCITO EN EL CÓDIGO**

MongoDB crea automáticamente un índice único en `_id` para cada colección.

---

### 10.2 Índices Recomendados

#### Colección `users`:
```javascript
// Índice único en Email para búsquedas rápidas
db.users.createIndex({ "email": 1 }, { unique: true })

// Índice en Role para filtros
db.users.createIndex({ "role": 1 })
```

#### Colección `machines`:
```javascript
// Índice en ProviderId para filtrar por proveedor
db.machines.createIndex({ "providerId": 1 })

// Índice geoespacial 2dsphere (requiere cambio en modelo)
db.machines.createIndex({ "location": "2dsphere" })

// Índice de texto para búsqueda
db.machines.createIndex({ "title": "text", "description": "text" })
```

#### Colección `bookings`:
```javascript
// Índice en RenterId (ya usado en consultas)
db.bookings.createIndex({ "renterId": 1 })

// Índice en MachineId
db.bookings.createIndex({ "machineId": 1 })

// Índice compuesto para validar disponibilidad
db.bookings.createIndex({ "machineId": 1, "start": 1, "end": 1 })

// Índice en Status para filtros
db.bookings.createIndex({ "status": 1 })
```

---

## 📝 11. SEEDS DE DATOS

### 11.1 Estado Actual

**❌ NO HAY SEEDS IMPLEMENTADOS**

No existe código para poblar la base de datos con datos iniciales.

---

### 11.2 Recomendación

Crear un servicio `SeedDataService` o un endpoint temporal para inicializar datos:

```csharp
// Ejemplo de seed
public class SeedDataService
{
    public async Task SeedAsync()
    {
        // Crear usuario admin
        var admin = new User
        {
            Name = "Administrador",
            Email = "admin@machly.com",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            Role = "ADMIN"
        };

        // Crear máquinas de ejemplo
        var machines = new List<Machine>
        {
            new Machine
            {
                ProviderId = "000000000000000000000000",
                Title = "Tractor John Deere 5075E",
                Description = "Tractor agrícola de 75 HP",
                PricePerDay = 1500.00m,
                Lat = -17.3935,
                Lng = -66.1570,
                Photos = new List<string>()
            }
        };

        // Insertar en MongoDB
    }
}
```

---

## 🚀 12. ENDPOINTS COMPLETOS - RESUMEN

### Endpoints Públicos (Sin Autenticación)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/auth/register` | Registrar usuario |
| POST | `/auth/login` | Iniciar sesión |
| GET | `/machines` | Listar máquinas |
| GET | `/machines/{id}` | Obtener máquina |
| POST | `/machines` | Crear máquina ⚠️ |
| POST | `/bookings` | Crear reserva ⚠️ |
| GET | `/bookings/user/{renterId}` | Reservas de usuario ⚠️ |
| POST | `/agro/calculate` | Calcular precio agronómico |

⚠️ = Debería requerir autenticación

---

### Endpoints que Faltan (Recomendados)

| Método | Ruta | Descripción |
|--------|------|-------------|
| PUT | `/machines/{id}` | Actualizar máquina |
| DELETE | `/machines/{id}` | Eliminar máquina |
| GET | `/machines?lat={lat}&lng={lng}&radius={km}` | Filtrar por geolocalización |
| GET | `/machines?providerId={id}` | Filtrar por proveedor |
| PUT | `/bookings/{id}/status` | Actualizar estado |
| POST | `/bookings/{id}/checkin` | Check-in con fotos |
| POST | `/bookings/{id}/checkout` | Check-out con fotos |
| GET | `/bookings/machine/{machineId}` | Reservas de máquina |
| POST | `/bookings/{id}/review` | Agregar reseña |
| GET | `/notifications/user/{userId}` | Notificaciones |
| PUT | `/notifications/{id}/read` | Marcar notificación como leída |

---

## 📋 13. EJEMPLOS DE USO COMPLETOS

### 13.1 Flujo Completo: Registro → Login → Crear Reserva

```bash
# 1. Registrar usuario
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Juan Pérez",
    "email": "juan@example.com",
    "password": "MiPassword123",
    "role": "RENTER"
  }'

# Respuesta: { "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }

# 2. Listar máquinas disponibles
curl -X GET https://localhost:5001/machines

# 3. Crear reserva (usar token en header)
curl -X POST https://localhost:5001/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "machineId": "507f1f77bcf86cd799439011",
    "renterId": "507f191e810c19729de860ea",
    "start": "2024-12-01T08:00:00Z",
    "end": "2024-12-05T18:00:00Z"
  }'

# 4. Ver historial de reservas
curl -X GET https://localhost:5001/bookings/user/507f191e810c19729de860ea \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 13.2 Flujo: Proveedor Crea Máquina

```bash
# 1. Registrar como PROVIDER
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "AgroMaq S.A.",
    "email": "agroma@example.com",
    "password": "Provider123",
    "role": "PROVIDER"
  }'

# 2. Crear máquina (usar token)
curl -X POST https://localhost:5001/machines \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "title": "Tractor John Deere 5075E",
    "description": "Tractor agrícola de 75 HP, ideal para labranza",
    "pricePerDay": 1500.00,
    "lat": -17.3935,
    "lng": -66.1570,
    "photos": [
      "https://example.com/tractor1.jpg",
      "https://example.com/tractor2.jpg"
    ]
  }'
```

---

### 13.3 Flujo: Cálculo Agronómico

```bash
# Calcular precio de servicios agronómicos
curl -X POST https://localhost:5001/agro/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "hectareas": 10.5,
    "toneladas": 25.0,
    "km": 50.0
  }'

# Respuesta:
# {
#   "total": 5425.0,
#   "detalles": [
#     "Hectáreas: 10.5 * 350 = 3675",
#     "Toneladas: 25 * 45 = 1125",
#     "Transporte: 50 km * 25 ton * 0.9 = 1125"
#   ],
#   "isMock": true
# }
```

---

## ⚠️ 14. LIMITACIONES Y MEJORAS RECOMENDADAS

### 14.1 Seguridad

1. **Autorización por roles:** Implementar `[Authorize(Roles = "...")]` en endpoints
2. **Validación de entrada:** Agregar Data Annotations o FluentValidation
3. **Clave JWT:** Mover a variables de entorno
4. **CORS:** Restringir a dominios específicos en producción
5. **HTTPS:** Forzar HTTPS en producción

---

### 14.2 Validaciones Faltantes

1. **Reservas:**
   - Validar que `End > Start`
   - Validar disponibilidad de máquina
   - Validar que fechas no sean pasadas
   - Validar que solo RENTER pueda crear reservas

2. **Máquinas:**
   - Validar que solo PROVIDER pueda crear/editar máquinas
   - Validar coordenadas geográficas válidas
   - Validar precio positivo

3. **Usuarios:**
   - Validar formato de email
   - Validar fortaleza de contraseña
   - Validar que Role sea válido

---

### 14.3 Funcionalidades Faltantes

1. **Gestión de Máquinas:**
   - Actualizar máquina
   - Eliminar máquina
   - Filtrar por proveedor
   - Búsqueda por texto

2. **Gestión de Reservas:**
   - Validar disponibilidad
   - Cancelar reserva
   - Check-in / Check-out
   - Reseñas

3. **Geolocalización:**
   - Índice 2dsphere
   - Búsqueda NearSphere
   - Filtro por radio

4. **Notificaciones:**
   - Modelo y repositorio
   - Endpoints CRUD
   - Integración con reservas

5. **Historial:**
   - Filtros avanzados
   - Paginación
   - Incluir datos de máquina

---

## 📚 15. DOCUMENTACIÓN ADICIONAL

### 15.1 Swagger

La API incluye Swagger UI disponible en:
- **URL:** `https://localhost:5001/` (raíz)
- **JSON:** `https://localhost:5001/swagger/v1/swagger.json`

### 15.2 Base de Datos

- **MongoDB:** `mongodb://localhost:27017/`
- **Database:** `machly_db`
- **Colecciones:** `users`, `machines`, `bookings`

---

## ✅ 16. CHECKLIST PARA FRONTEND (Machly.Web)

### Modelos a Usar

- [ ] `User` - Para autenticación y perfiles
- [ ] `Machine` - Para listar y mostrar máquinas
- [ ] `Booking` - Para reservas e historial

### DTOs a Usar

- [ ] `RegisterRequest` - Formulario de registro
- [ ] `LoginRequest` - Formulario de login
- [ ] `BookingCreateRequest` - Formulario de reserva
- [ ] `AgroCalculateRequest` - Formulario de cálculo agronómico

### Endpoints a Consumir

- [ ] `POST /auth/register` - Registro
- [ ] `POST /auth/login` - Login
- [ ] `GET /machines` - Listar máquinas
- [ ] `GET /machines/{id}` - Detalle de máquina
- [ ] `POST /machines` - Crear máquina (PROVIDER)
- [ ] `POST /bookings` - Crear reserva (RENTER)
- [ ] `GET /bookings/user/{renterId}` - Historial
- [ ] `POST /agro/calculate` - Calcular precio agronómico

### Autenticación

- [ ] Guardar token JWT en localStorage/sessionStorage
- [ ] Incluir token en header: `Authorization: Bearer {token}`
- [ ] Manejar expiración de token (7 días)
- [ ] Redirigir a login si token inválido

### Roles y Permisos

- [ ] Mostrar panel PROVIDER solo si rol = "PROVIDER"
- [ ] Mostrar panel ADMIN solo si rol = "ADMIN"
- [ ] Restringir creación de máquinas a PROVIDER
- [ ] Restringir creación de reservas a RENTER

---

## 🎯 CONCLUSIÓN

**Machly.Api** es una API REST funcional con las bases implementadas para un sistema de alquiler de maquinaria agrícola. Incluye autenticación JWT, gestión de usuarios, máquinas y reservas, y cálculo agronómico básico.

**Estado Actual:** MVP funcional (Sprint 1)

**Próximos Pasos Recomendados:**
1. Implementar autorización por roles
2. Agregar validaciones de negocio
3. Implementar funcionalidades faltantes (check-in/out, reseñas, notificaciones)
4. Agregar índices de MongoDB
5. Implementar búsqueda geográfica
6. Mejorar seguridad (CORS, HTTPS, variables de entorno)

---

**Versión del Informe:** 1.0  
**Fecha:** 2024  
**Proyecto:** Machly.Api  
**Framework:** ASP.NET Core 8.0  
**Base de Datos:** MongoDB 3.5.1

