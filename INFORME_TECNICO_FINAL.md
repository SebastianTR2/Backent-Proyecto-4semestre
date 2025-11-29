# Informe de Verificación de Requerimientos Funcionales – Proyecto Machly

Este informe evalúa el cumplimiento de los Requerimientos Funcionales (RF-01 al RF-16) en el backend (`Machly.Api`) y en la aplicación web (`Machly.Web`).

## 1. Evaluación del Backend (Machly.Api)

| RF | Descripción | Estado | Evidencia Técnica / Archivos |
|----|-------------|--------|------------------------------|
| **RF-01** | Registro con roles | ✔ Implementado | `AuthController.cs` permite registro con roles ADMIN, PROVIDER, RENTER. |
| **RF-02** | Login con JWT | ✔ Implementado | `AuthController.cs`, `JwtHelper.cs`. Genera token válido. |
| **RF-03** | Maquinaria urbana | ✔ Implementado | `MachinesController.cs` (POST). Modelo `Machine` soporta tipo URBANA. |
| **RF-04** | Maquinaria agronómica | ✔ Implementado | `MachinesController.cs`. Modelo `Machine` incluye `TariffsAgro` (ha/ton/km). |
| **RF-05** | Filtros de búsqueda | ✔ Implementado | `MachinesController.GetFilteredAsync`. Filtra por categoría, tipo, precio, ubicación. |
| **RF-06** | Geolocalización | ✔ Implementado | `Machine.cs` tiene `Location` (GeoJson). `MachinesController` soporta búsqueda por radio. |
| **RF-07** | Reservas estándar | ✔ Implementado | `BookingsController.cs`. Soporta fechas y cálculo por día. |
| **RF-08** | Reservas agronómicas | ✔ Implementado | `BookingsController.cs`. Soporta cálculo basado en métricas agro. |
| **RF-09** | Cálculo automático | ✔ Implementado | `BookingService.cs` contiene la lógica de precios. |
| **RF-10** | Check-in/out con fotos | ✔ Implementado | `BookingsController.cs` endpoints `checkin`/`checkout` aceptan fotos. |
| **RF-11** | Estado de máquinas | ✔ Implementado | `MachinesController.cs` endpoint `status`. |
| **RF-12** | Gestión de usuarios | ✔ Implementado | `AdminController.cs` permite listar y ver detalles de usuarios. |
| **RF-13** | Reportes | ⚠️ Parcial | `AdminController.cs` tiene `Dashboard` y `ExportUsersCsv`. Falta parametrización avanzada. |
| **RF-14** | Calificaciones | ✔ Implementado | `BookingsController.cs` endpoint `review`. |
| **RF-15** | Notificaciones | ✔ Implementado | `NotificationsController.cs` permite enviar y listar. |
| **RF-16** | Parámetros globales | ❌ No Implementado | No existe controlador ni modelo para configuraciones globales. |

## 2. Evaluación del Frontend Web (Machly.Web)

| RF | Descripción | Estado | Evidencia / Observaciones |
|----|-------------|--------|---------------------------|
| **RF-01** | Registro | ✔ Implementado | `AccountController.Register`. |
| **RF-02** | Login con JWT | ✔ Implementado | `AccountController.Login` persiste token en cookie y `JwtDelegatingHandler` lo usa. |
| **RF-03** | Registro Maq. Urbana | ✔ Implementado | `ProviderMachinesController.Create`. |
| **RF-04** | Registro Maq. Agro | ✔ Implementado | `ProviderMachinesController.Create` soporta tarifas agro. |
| **RF-05** | Búsqueda | ✔ Implementado | `RenterController.Explore` con filtros laterales. |
| **RF-06** | Mapa | ✔ Implementado | `RenterController.Map` (vista existe, consume API). |
| **RF-07** | Reservas estándar | ✔ Implementado | Flujo de reserva funcional. |
| **RF-08** | Reservas agronómicas | ✔ Implementado | Flujo de reserva funcional. |
| **RF-09** | Cálculo automático | ✔ Implementado | UI muestra total estimado. |
| **RF-10** | Check-in/out | ✔ Implementado | `ProviderMachinesController` con formularios POST (corregido). |
| **RF-11** | Estado de máquinas | ✔ Implementado | `ProviderMachinesController` permite togglear estado. |
| **RF-12** | Gestión admin | ✔ Implementado | `AdminController` lista usuarios y permite acciones. |
| **RF-13** | Reportes | ⚠️ Parcial | `Admin/Dashboard` muestra stats. Faltan reportes customizables. |
| **RF-14** | Calificaciones | ✔ Implementado | Modal de calificación en `Renter/Index`. |
| **RF-15** | Notificaciones | ⚠️ Parcial | `NotificationsController` existe, UI básica. |
| **RF-16** | Parámetros globales | ❌ No Implementado | No hay vista ni controlador para esto. |

## 3. Estado Final Detallado

| RF | Backend | Web | Estado | Observaciones |
|----|---------|-----|--------|---------------|
| RF-01 | ✔ | ✔ | **OK** | Registro funciona correctamente. |
| RF-02 | ✔ | ✔ | **OK** | Login seguro y persistencia de token OK. |
| RF-03 | ✔ | ✔ | **OK** | Creación de máquinas OK. |
| RF-04 | ✔ | ✔ | **OK** | Soporte agro OK. |
| RF-05 | ✔ | ✔ | **OK** | Filtros OK. |
| RF-06 | ✔ | ✔ | **OK** | Geo OK. |
| RF-07 | ✔ | ✔ | **OK** | Reservas OK. |
| RF-08 | ✔ | ✔ | **OK** | Reservas Agro OK. |
| RF-09 | ✔ | ✔ | **OK** | Cálculo precios OK. |
| RF-10 | ✔ | ✔ | **OK** | Check-in/out OK. |
| RF-11 | ✔ | ✔ | **OK** | Estado OK. |
| RF-12 | ✔ | ✔ | **OK** | Admin Users OK. |
| RF-13 | ⚠️ | ⚠️ | **INCOMPLETO** | Faltan reportes avanzados/customizables. |
| RF-14 | ✔ | ✔ | **OK** | Reviews OK. |
| RF-15 | ✔ | ⚠️ | **PARCIAL** | Backend OK, Web UI básica. |
| RF-16 | ❌ | ❌ | **NO IMPLEMENTADO** | Falta totalmente. |

---

## 4. Código Faltante (RF-16: Parámetros Globales)

Para cumplir con el **RF-16**, se debe agregar la gestión de configuraciones globales (ej. categorías permitidas, comisiones, textos legales).

### A. Backend (`Machly.Api`)

**1. Modelo (`Models/GlobalSettings.cs`)**
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class GlobalSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public List<string> AllowedCategories { get; set; } = new();
        public decimal CommissionRate { get; set; } = 0.10m; // 10% default
        public string TermsAndConditions { get; set; } = string.Empty;
    }
}
```

**2. Controlador (`Controllers/SettingsController.cs`)**
```csharp
using Machly.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IMongoCollection<GlobalSettings> _settings;

        public SettingsController(MongoDbContext context)
        {
            _settings = context.GetCollection<GlobalSettings>("settings");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _settings.Find(_ => true).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalSettings { AllowedCategories = new List<string> { "Tractor", "Excavadora" } };
                await _settings.InsertOneAsync(settings);
            }
            return Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update([FromBody] GlobalSettings settings)
        {
            var existing = await _settings.Find(_ => true).FirstOrDefaultAsync();
            if (existing == null)
            {
                await _settings.InsertOneAsync(settings);
            }
            else
            {
                settings.Id = existing.Id;
                await _settings.ReplaceOneAsync(s => s.Id == existing.Id, settings);
            }
            return Ok(settings);
        }
    }
}
```

### B. Frontend (`Machly.Web`)

**1. ApiClient (`Services/SettingsApiClient.cs`)**
```csharp
using Machly.Web.Models; // Asumir modelo similar en Web
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class SettingsApiClient
    {
        private readonly HttpClient _httpClient;

        public SettingsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GlobalSettings?> GetAsync()
        {
            return await _httpClient.GetFromJsonAsync<GlobalSettings>("/settings");
        }

        public async Task<bool> UpdateAsync(GlobalSettings settings)
        {
            var response = await _httpClient.PutAsJsonAsync("/settings", settings);
            return response.IsSuccessStatusCode;
        }
    }
}
```

**2. Controlador (`Controllers/AdminController.cs`) - Agregar Acción**
```csharp
// Inyectar SettingsApiClient en constructor

[HttpGet]
public async Task<IActionResult> Settings()
{
    var settings = await _settingsApiClient.GetAsync();
    return View(settings);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Settings(GlobalSettings model)
{
    if (!ModelState.IsValid) return View(model);
    
    var success = await _settingsApiClient.UpdateAsync(model);
    if (success)
    {
        TempData["Success"] = "Configuración actualizada";
        return RedirectToAction(nameof(Settings));
    }
    
    ModelState.AddModelError("", "Error al actualizar");
    return View(model);
}
```
