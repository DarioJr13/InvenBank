# 🏪 InvenBank API - Sistema de Inventario Multi-Proveedor

API REST desarrollada en .NET Core 8 para el sistema de inventario InvenBank, que permite gestionar productos con múltiples proveedores, precios diferenciados y control de stock en tiempo real.

## 🚀 **FASE 1 COMPLETADA**

### ✅ **Funcionalidades Implementadas**
- ✅ Proyecto base .NET Core 8 estructurado
- ✅ Configuración Dapper + SQL Server
- ✅ Autenticación JWT completa
- ✅ CORS configurado para Angular e Ionic
- ✅ Swagger/OpenAPI documentación
- ✅ Logging estructurado con Serilog
- ✅ Manejo global de errores
- ✅ Arquitectura limpia (Services, Repositories, DTOs)
- ✅ Health checks y monitoring

### 🏗️ **Arquitectura Implementada**
```
InvenBank.API/
├── 📂 Controllers/           # Endpoints REST
├── 📂 Services/             # Lógica de negocio + JWT
├── 📂 Repositories/         # Acceso a datos con Dapper
├── 📂 DTOs/                # Data Transfer Objects
├── 📂 Entities/            # Modelos de dominio
├── 📂 Middleware/          # JWT, Logging, CORS, Errors
├── 📂 Configuration/       # Settings tipados
└── Program.cs              # Configuración DI
```

## 🔧 **Configuración e Instalación**

### **📋 Prerrequisitos**
- .NET 8.0 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 / VS Code
- Base de datos InvenBank_db (scripts proporcionados)

### **🔌 Configuración de Base de Datos**

1. **Ejecutar scripts DDL** (en orden):
```sql
-- 1. Crear base de datos y tablas
.\DDL_CreateDBAndTable.sql

-- 2. Crear stored procedures
.\Store_Procedure.sql

-- 3. Crear vistas
.\Views.sql

-- 4. Crear triggers
.\Triggers.sql

-- 5. Crear funciones
.\Functions.sql

-- 6. Insertar datos de prueba
.\Test_Data_I.sql
```

2. **Configurar connection string** en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvenBank_db;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### **🚀 Ejecutar la Aplicación**

1. **Restaurar paquetes**:
```bash
dotnet restore
```

2. **Ejecutar**:
```bash
dotnet run
```

3. **Acceder a Swagger**:
```
https://localhost:7000
```

## 📊 **Endpoints de Health Check**

### **🏥 Verificar Estado General**
```
GET /api/health
```

### **💾 Verificar Base de Datos**
```
GET /api/health/database
```

### **📋 Verificar Tablas**
```
GET /api/health/tables
```

### **🔐 Verificar JWT** (requiere token)
```
GET /api/health/auth
Authorization: Bearer {token}
```

### **💻 Información del Sistema**
```
GET /api/health/system
```

## 🔐 **Autenticación JWT**

### **Configuración**
- **Algoritmo**: HMAC-SHA256
- **Expiración**: 60 minutos (configurable)
- **Refresh Token**: 7 días
- **Claims incluidos**: UserId, Email, Role, FirstName, LastName

### **Usar en Swagger**
1. Hacer clic en **"Authorize"** 🔒
2. Ingresar: `Bearer {tu_token_jwt}`
3. Los endpoints protegidos funcionarán automáticamente

## 📝 **Estructura de Respuestas**

### **Respuesta Exitosa**
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": { ... },
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **Respuesta con Error**
```json
{
  "success": false,
  "message": "Error de validación",
  "data": null,
  "errors": ["El email es requerido"],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **Respuesta Paginada**
```json
{
  "success": true,
  "message": "Datos obtenidos exitosamente",
  "data": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalRecords": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## 🛠️ **Tecnologías Utilizadas**

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET Core | 8.0 | Framework base |
| Dapper | 2.1.35 | ORM ligero |
| JWT Bearer | 8.0.0 | Autenticación |
| Serilog | 8.0.0 | Logging estructurado |
| Swagger | 6.5.0 | Documentación API |
| BCrypt | 4.0.3 | Hash de contraseñas |
| AutoMapper | 12.0.1 | Mapeo de objetos |
| FluentValidation | 11.3.0 | Validaciones |

## 🔍 **Logging**

### **Configuración Multi-Target**
- **Console**: Para desarrollo
- **File**: Logs rotativos diarios
- **SQL Server**: Errores críticos

### **Localización de Logs**
- **Archivos**: `./Logs/invenbank-YYYY-MM-DD.log`
- **Base de datos**: Tabla `Logs` (auto-creada)
- **Console**: Tiempo real durante desarrollo

## 🚧 **Próximos Pasos - FASE 2**

### **🎯 CRUD Administrativo a Implementar**
1. **CategoriesController** - Gestión de categorías jerárquicas
2. **SuppliersController** - CRUD de proveedores
3. **ProductsController** - CRUD de productos
4. **ProductSuppliersController** - Asociaciones y precios
5. **AuthController** - Login/Register/Refresh

### **📱 FASE 3: Endpoints Cliente (App Móvil)**
1. **CatalogController** - Búsqueda y filtros
2. **WishlistController** - Lista de deseos
3. **OrdersController** - Compras y historial

## ⚙️ **Configuraciones Importantes**

### **JWT Settings**
```json
{
  "JwtSettings": {
    "SecretKey": "InvenBank2024!SuperSecretKey@#$%^&*()1234567890",
    "Issuer": "InvenBank.API",
    "Audience": "InvenBank.Clients",
    "ExpiryInMinutes": 60
  }
}
```

### **CORS Settings**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",  // Angular
      "http://localhost:3000",  // React
      "http://localhost:8100"   // Ionic
    ]
  }
}
```

## 🐛 **Troubleshooting**

### **Error de Conexión a BD**
```bash
# Verificar connection string
GET /api/health/database

# Verificar que SQL Server esté ejecutándose
services.msc -> SQL Server
```

### **Error de JWT**
```bash
# Verificar configuración
GET /api/health/auth

# Revisar logs de autenticación
tail -f ./Logs/invenbank-*.log
```

## 📞 **Soporte**

Para problemas técnicos, revisar:
1. **Logs de la aplicación**: `./Logs/`
2. **Health checks**: `/api/health/*`
3. **Swagger docs**: `/swagger`

---

🎉 **¡FASE 1 COMPLETADA!** Lista para proceder con **FASE 2: CRUD Administrativo**