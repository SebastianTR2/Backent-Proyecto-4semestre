# Machly – Phase 1 Notes

## Nuevos endpoints API
- `GET /admin/users/{id}` – Detalle de usuario para ADMIN.
- `GET /admin/machines` – Listado global con filtros (type, category, providerId, withOperator, precio).
- `GET /admin/machines/{id}` – Detalle completo de máquina.
- `DELETE /admin/machines/{id}` – Eliminar máquina (solo ADMIN).
- `PUT /admin/provider/verify/{id}` – Verificar/Desverificar proveedor (requiere body `{ isVerified: bool }`).
- `GET /admin/bookings?from&to` – Reservas con filtros de fecha.
- `GET /bookings/user/{renterId}?from&to` – Historial del renter con rango.
- `GET /provider/bookings?from&to` – Reservas de máquinas del proveedor con rango.
- `GET /renter/bookings?from&to` – Alias protegido para renter autenticado.

## Controladores / servicios modificados
- **Machly.Api**
  - `AdminController`, `ProviderController`, `RenterController`, `BookingsController`
  - `BookingService`, `BookingRepository`, `MachineService`, `MachineRepository`, `UserRepository`, `AuthService`
- **Machly.Web**
  - `AdminController`, `ProviderMachinesController`, `RenterController`
  - `AdminApiClient`, `BookingsApiClient`
  - Modelos: `User`, `Machine`, `Booking` (alíneados con API) + nuevos DTO en `Models/Admin`

## Nuevas vistas Razor
- `Views/Admin/UserDetails.cshtml`
- `Views/Admin/MachineDetails.cshtml`

## Vistas actualizadas
- `Views/Admin/Users.cshtml`, `Machines.cshtml`, `Bookings.cshtml`
- `Views/ProviderMachines/Bookings.cshtml`
- `Views/Renter/Index.cshtml`

## Navegación
- **Admin → Usuarios:** `Admin/Users` lista. “Ver detalles” → `Admin/UserDetails/{id}`. Desde allí se puede verificar o ver actividad (máquinas/reservas).
- **Admin → Máquinas:** `Admin/Machines` muestra filtros + tabla. Botón “Detalles” → `Admin/MachineDetails/{id}` (incluye tarifas, fotos, reseñas, botón eliminar).
- **Admin → Reservas:** `Admin/Bookings` ofrece filtros rápidos y rango personalizado.
- **Proveedor → Reservas:** `ProviderMachines/Bookings` ahora incluye los mismos rangos.
- **Renter → Mis Reservas:** `Renter/Index` con filtros y acciones de review.

