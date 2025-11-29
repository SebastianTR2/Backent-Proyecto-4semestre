# 🚀 GUÍA DE EJECUCIÓN - MACHLY

## 📋 Requisitos Previos

- .NET 8.0 SDK
- MongoDB (local o remoto)
- Visual Studio 2022 o VS Code

---

## 🔧 Configuración de Machly.Api

### 1. Configurar MongoDB

Asegúrate de que MongoDB esté ejecutándose en `localhost:27017` o actualiza la cadena de conexión en `appsettings.json`:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017/",
    "DatabaseName": "machly_db"
  }
}
```

### 2. Configurar JWT

La clave JWT está en `appsettings.json`. En producción, muévela a variables de entorno.

### 3. Ejecutar Machly.Api

```bash
cd Machly.Api
dotnet restore
dotnet run
```

La API estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001/`

### 4. Seed Data

El seed se ejecuta automáticamente en desarrollo al iniciar la aplicación. Se crean:

- **Admin:** admin@machly.com / Admin123!
- **Provider 1:** provider1@machly.com / Provider123!
- **Provider 2:** provider2@machly.com / Provider123!
- **Renter 1:** renter1@machly.com / Renter123!
- **Renter 2:** renter2@machly.com / Renter123!

---

## 🌐 Configuración de Machly.Web

### 1. Configurar URL de la API

Edita `appsettings.json` y asegúrate de que la URL de la API sea correcta:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

### 2. Ejecutar Machly.Web

```bash
cd Machly.Web
dotnet restore
dotnet run
```

La aplicación web estará disponible en:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`

---

## 🧪 Pruebas Rápidas

### 1. Login como Admin

1. Ve a `https://localhost:7001/Account/Login`
2. Email: `admin@machly.com`
3. Password: `Admin123!`
4. Deberías ser redirigido al Dashboard de Admin

### 2. Login como Provider

1. Ve a `https://localhost:7001/Account/Login`
2. Email: `provider1@machly.com`
3. Password: `Provider123!`
4. Deberías ver el panel de "Mis Máquinas"

### 3. Login como Renter

1. Ve a `https://localhost:7001/Account/Login`
2. Email: `renter1@machly.com`
3. Password: `Renter123!`
4. Deberías ver "Mis Reservas"

---

## 📝 Endpoints de la API

### Autenticación
- `POST /auth/register` - Registrar usuario
- `POST /auth/login` - Iniciar sesión

### Máquinas (Público)
- `GET /machines` - Listar máquinas (con filtros opcionales)
- `GET /machines/{id}` - Obtener máquina por ID
- `POST /machines` - Crear máquina (PROVIDER)

### Provider
- `GET /provider/machines` - Máquinas del proveedor
- `PUT /provider/machines/{id}` - Editar máquina
- `DELETE /provider/machines/{id}` - Eliminar máquina
- `GET /provider/bookings` - Reservas del proveedor

### Reservas
- `POST /bookings` - Crear reserva (RENTER)
- `GET /bookings/user/{renterId}` - Reservas del usuario
- `POST /bookings/{id}/checkin` - Check-in (PROVIDER)
- `POST /bookings/{id}/checkout` - Check-out (PROVIDER)
- `POST /bookings/review/{id}` - Agregar reseña (RENTER)

### Admin
- `GET /admin/users` - Listar usuarios
- `GET /admin/machines` - Listar máquinas
- `GET /admin/bookings` - Listar reservas
- `PUT /admin/provider/verify/{id}` - Verificar proveedor
- `GET /admin/reports/basic` - Reportes básicos

### Historial
- `GET /history/user/{id}` - Historial por usuario
- `GET /history/machine/{id}` - Historial por máquina

### Notificaciones
- `GET /notifications/{userId}` - Obtener notificaciones
- `POST /notifications/send` - Enviar notificación (ADMIN)
- `PUT /notifications/{id}/read` - Marcar como leída

---

## ⚠️ Solución de Problemas

### Error: "Cannot connect to MongoDB"

- Verifica que MongoDB esté ejecutándose
- Verifica la cadena de conexión en `appsettings.json`
- Asegúrate de que el puerto 27017 esté disponible

### Error: "JWT token invalid"

- Verifica que la clave JWT en `appsettings.json` sea la misma en ambos proyectos
- Limpia las cookies del navegador
- Vuelve a iniciar sesión

### Error: "CORS policy"

- La API tiene CORS configurado para permitir todos los orígenes
- Si persiste, verifica que la URL de la API en `Machly.Web/appsettings.json` sea correcta

### Error: "404 Not Found" en vistas

- Asegúrate de que las vistas estén en las carpetas correctas:
  - `Views/Home/Index.cshtml`
  - `Views/Account/Login.cshtml`
  - `Views/ProviderMachines/Index.cshtml`
  - etc.

---

## 📚 Estructura de Proyectos

### Machly.Api
```
Machly.Api/
├── Controllers/      # Endpoints API
├── Models/           # Modelos de dominio
├── DTOs/             # Data Transfer Objects
├── Repositories/     # Acceso a datos
├── Services/         # Lógica de negocio
├── Utils/            # Utilidades
└── Config/           # Configuraciones
```

### Machly.Web
```
Machly.Web/
├── Controllers/      # Controladores MVC
├── Views/            # Vistas Razor
├── Models/           # ViewModels
├── Services/         # Clientes HTTP para API
└── wwwroot/         # Archivos estáticos
```

---

## ✅ Checklist de Verificación

- [ ] MongoDB ejecutándose
- [ ] Machly.Api ejecutándose en puerto 5001
- [ ] Machly.Web ejecutándose en puerto 7001
- [ ] Seed data cargado (verificar en MongoDB)
- [ ] Login funciona con credenciales de seed
- [ ] Panel Admin accesible
- [ ] Panel Provider accesible
- [ ] Panel Renter accesible

---

## 🎯 Próximos Pasos

1. **Mejorar UI/UX:** Agregar más estilos y funcionalidades visuales
2. **Subida de Fotos:** Implementar almacenamiento de imágenes (Azure Blob, AWS S3, etc.)
3. **Notificaciones en Tiempo Real:** Implementar SignalR
4. **Paginación:** Agregar paginación en listados
5. **Búsqueda Avanzada:** Mejorar filtros de máquinas
6. **Mapas:** Integrar Google Maps o Mapbox para visualización geográfica

---

**¡Listo para usar!** 🚀

