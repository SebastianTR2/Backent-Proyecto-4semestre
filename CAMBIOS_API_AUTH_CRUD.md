# Documentación de Cambios en la API Machly

Este documento detalla las modificaciones realizadas en la API para implementar autenticación robusta, autorización por roles (ADMIN, PROVEEDOR, ARRENDADOR) y operaciones CRUD completas.

## 1. Autenticación y Autorización

### Roles Implementados
- **ADMIN**: Acceso total a todas las operaciones CRUD de todas las entidades.
- **PROVIDER**: Acceso CRUD a sus propias máquinas y gestión de reservas relacionadas.
- **RENTER**: Acceso a crear reservas y consultar su historial.

### Seguridad
- Se ha reforzado la validación de tokens JWT en `Program.cs` y en los controladores.
- Se utilizan atributos `[Authorize(Roles = "...")]` para restringir el acceso.
- Se implementó lógica interna en los controladores para validar que los usuarios solo accedan a sus propios recursos (excepto ADMIN).

## 2. Cambios en Controladores (CRUD)

### MachinesController (`/machines`)
| Método | Endpoint | Roles Permitidos | Descripción |
|--------|----------|------------------|-------------|
| POST | `/machines` | ADMIN, PROVIDER | Crear máquina. Admin puede asignar `ProviderId`. |
| PUT | `/machines/{id}` | ADMIN, PROVIDER (Dueño) | Actualizar datos de la máquina. |
| DELETE | `/machines/{id}` | ADMIN, PROVIDER (Dueño) | Eliminar máquina. |
| PUT | `/machines/{id}/status` | ADMIN, PROVIDER (Dueño) | Cambiar estado (Fuera de servicio). |
| GET | `/machines` | Público | Listar máquinas (filtros disponibles). |

### BookingsController (`/bookings`)
| Método | Endpoint | Roles Permitidos | Descripción |
|--------|----------|------------------|-------------|
| POST | `/bookings` | ADMIN, RENTER | Crear reserva. Admin puede asignar `RenterId`. |
| GET | `/bookings` | ADMIN | Listar todas las reservas (filtros de fecha). |
| PUT | `/bookings/{id}` | ADMIN | Actualizar reserva (principalmente estado). |
| DELETE | `/bookings/{id}` | ADMIN | Eliminar reserva. |
| GET | `/bookings/user/{id}` | ADMIN, RENTER (Propio) | Historial de reservas de un usuario. |

### AdminController (`/admin`)
Se agregaron endpoints para la gestión de usuarios (Proveedores y Arrendadores):

| Método | Endpoint | Roles Permitidos | Descripción |
|--------|----------|------------------|-------------|
| POST   | `/admin/users` | ADMIN | Crear nuevo usuario. |
| PUT    | `/admin/users/{id}` | ADMIN | Actualizar usuario existente. |
| DELETE | `/admin/users/{id}` | ADMIN | Eliminar usuario. |
| GET    | `/admin/users` | ADMIN | Listar todos los usuarios. |

## 3. Arquitectura Frontend
- Se verificó que el proyecto `Machly.Web` (MVC) **NO** realiza accesos directos a la base de datos `MongoDbContext`.
- Toda la comunicación se realiza a través de servicios `ApiClient` (`MachinesApiClient`, `AdminApiClient`, etc.) que consumen la API mediante HTTP.
- Se utiliza `JwtDelegatingHandler` para adjuntar el token JWT automáticamente a las peticiones desde el frontend.

## 4. Swagger
- Swagger está configurado para soportar autenticación JWT.
- Use el botón "Authorize" en la interfaz de Swagger e ingrese `Bearer <su_token>` para probar los endpoints protegidos.

## Instrucciones de Prueba
1. Ejecute la API (`Machly.Api`).
2. Obtenga un token de ADMIN (login en `/auth/login`).
3. Use Swagger para verificar que puede crear, editar y eliminar máquinas y usuarios.
4. Verifique que un usuario estándar (PROVIDER/RENTER) solo pueda modificar sus propios recursos.
