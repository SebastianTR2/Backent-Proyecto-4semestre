# 📘 Guía Técnica y Manual de Uso: Machly GraphQL

Este documento detalla el diagnóstico, las correcciones aplicadas al backend y la guía completa para utilizar Banana Cake Pop con la API de Machly.

---

## 🛠 1. Diagnóstico y Solución de Errores

### 🔍 Problema Detectado
El esquema GraphQL fallaba o estaba incompleto debido a:
1.  **Incompatibilidad de Tipos BSON:** HotChocolate no sabe cómo serializar tipos nativos de MongoDB como `BsonValue`, `ObjectId` o `GeoJsonPoint` si se exponen directamente en el esquema.
    *   *Error típico:* `Unable to infer or resolve schema type from the type reference 'BsonValue'`.
2.  **Falta de DTOs Específicos:** El modelo `Machine` contenía propiedades complejas (`TariffsAgro`, `KmTariff`) que no estaban mapeadas en `MachineDto`, impidiendo su consulta.

### ✅ Soluciones Aplicadas
Se realizaron los siguientes cambios en el código (`Machly.Api`):

1.  **Nuevos DTOs:** Se crearon `TariffAgroDto` y `KmTariffDto` en `Dtos.cs` para evitar exponer las clases del modelo directamente.
2.  **Actualización de Mappers:** Se actualizaron `Query.cs` y `Mutation.cs` para transformar los datos del modelo a estos nuevos DTOs.
3.  **Registro de Tipos:** Se registraron explícitamente los nuevos tipos en `Program.cs` y `Types.cs`.
4.  **Safety Net (Red de Seguridad):** Se agregó `.BindRuntimeType<BsonValue, StringType>()` en `Program.cs` para que, si algún tipo BSON se escapa, se trate como un String en lugar de romper el esquema.

---

## 🍌 2. Guía de Banana Cake Pop (BCP)

### A) Panel "Request" (Izquierda)
Aquí escribes tus consultas GraphQL. **NO** pongas JSON, solo sintaxis GraphQL.

**Ejemplo de Query Pública:**
```graphql
query {
  machines {
    id
    title
    pricePerDay
    category
    tariffsAgro {
      hectarea
      tonelada
    }
  }
}
```

**Ejemplo de Query Autenticada:**
```graphql
query {
  me {
    id
    name
    email
    role
  }
}
```

### B) Pestaña "GraphQL Variables" (Abajo)
Usa esta sección para pasar datos dinámicos. Formato **JSON Estricto**.

**Si tu query es:**
```graphql
query GetMachine($id: String!) {
  machineById(id: $id) { ... }
}
```

**Tus variables son:**
```json
{
  "id": "656f8a..."
}
```

### C) Pestaña "HTTP Headers" (Abajo)
**¡CRÍTICO!** Aquí va tu autenticación.

```json
{
  "Authorization": "Bearer TU_TOKEN_AQUI"
}
```
*Nota: Asegúrate de dejar un espacio entre "Bearer" y el token.*

### D) Cómo obtener el Token
1.  Ve a Swagger (`/swagger`) o usa Postman.
2.  Ejecuta el endpoint `POST /api/auth/login`.
3.  Copia el valor de `token` de la respuesta JSON.
4.  Pégalo en los Headers de BCP siguiendo el formato de arriba.

---

## 🧪 3. Queries Listas para Copiar y Pegar

Usa estos ejemplos probados con el nuevo esquema.

### 🔹 1. Obtener Usuario Actual (Requiere Token)
```graphql
query Me {
  me {
    id
    name
    email
    role
    photoUrl
  }
}
```

### 🔹 2. Listar Máquinas (Incluyendo Tarifas Agro)
```graphql
query GetMachines {
  machines {
    id
    title
    category
    pricePerDay
    tariffsAgro {
      hectarea
      tonelada
      kmTariffs {
        minKm
        maxKm
        tarifaPorKm
      }
    }
    photos
  }
}
```

### 🔹 3. Obtener Máquina por ID (Usando Variables)
**Query:**
```graphql
query GetMachine($id: String!) {
  machineById(id: $id) {
    id
    title
    description
    pricePerDay
    providerId
    isOutOfService
  }
}
```
**Variables:**
```json
{
  "id": "ID_REAL_DE_MONGO"
}
```

### 🔹 4. Crear Reserva (Mutation)
**Mutation:**
```graphql
mutation CreateBooking($input: CreateBookingInput!) {
  createBooking(input: $input) {
    id
    status
    totalPrice
    checkInDate
    checkOutDate
  }
}
```
**Variables:**
```json
{
  "input": {
    "machineId": "ID_DE_MAQUINA",
    "start": "2023-12-01T09:00:00Z",
    "end": "2023-12-05T18:00:00Z"
  }
}
```

---

## 🔧 4. Solución de Problemas Comunes

| Error | Causa Probable | Solución |
| :--- | :--- | :--- |
| `Schema Fetch Failed` | La API no está corriendo o URL incorrecta. | Verifica que `Machly.Api` esté en ejecución y la URL sea correcta. |
| `The field X does not exist on type Query` | Estás pidiendo un campo que no existe o no tienes permisos. | Revisa la documentación (pestaña Schema en BCP) o tu nivel de acceso. |
| `User not authenticated` | No enviaste el token o expiró. | Revisa la pestaña **HTTP Headers**. |
| `Expected Name-token but found String` | Error de sintaxis en Query. | Quitaste o pusiste comillas donde no debías (ej. en nombres de campos). |

---

**¡Todo listo!** Ahora tu backend soporta correctamente los tipos complejos y tienes las herramientas para probarlo.
