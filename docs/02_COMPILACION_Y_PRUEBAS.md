# Guía de Compilación y Pruebas

## 🚀 Cómo Compilar y Probar la Aplicación

### Paso 1: Verificar Requisitos Previos
Antes de compilar, asegúrate de tener instalado:
- **.NET 8.0 SDK** o superior
- **SQL Server Express** (localhost\SQLEXPRESS) O **LocalDB** (el proyecto usa LocalDB como fallback automático)
- **Visual Studio 2022** o **VS Code** (opcional, pero recomendado)

Verifica la instalación de .NET:
```powershell
dotnet --version
# Debe mostrar 8.0.x o superior
```

### Paso 2: Clonar o Descargar el Proyecto
```powershell
cd C:\tu\carpeta\destino
# Si tienes el proyecto, navega a la carpeta DSM_Prueba
```

### Paso 3: Restaurar Dependencias NuGet
```powershell
# Desde la carpeta raíz del proyecto (donde está Solution.sln)
dotnet restore Solution.sln
```

Este comando descarga todos los paquetes NuGet necesarios:
- NHibernate 5.5.2
- System.Data.SqlClient 4.8.6
- Microsoft.Extensions.DependencyInjection 8.0.0

### Paso 4: Compilar la Solución Completa
```powershell
dotnet build Solution.sln --configuration Release
```

Deberías ver:
```
✓ ApplicationCore realizado correctamente
✓ Infrastructure realizado correctamente
✓ InitializeDb realizado correctamente
Compilación realizado correctamente en X.Xs
```

### Paso 5: Ejecutar InitializeDb (Primera Vez)
```powershell
cd InitializeDb
dotnet run
```

**¿Qué hace InitializeDb?**
1. **Intenta conectar a SQL Server Express** (localhost\SQLEXPRESS)
2. **Si falla, usa LocalDB automáticamente** como fallback
3. **Crea el esquema de base de datos** usando NHibernate SchemaExport
4. **Inserta datos de prueba**:
   - 4 Usuarios (player1, player2, player3, newplayer)
   - 3 Comunidades (Gamers Pro, Casual Players, Elite Squad)
   - 1 Equipo (Team Alpha)
   - 2 Juegos (League of Legends, FIFA 24)
   - 1 Torneo (Copa de Verano 2025)
   - Membresías y publicaciones
5. **Prueba todos los métodos**:
   - ✓ Login de usuario
   - ✓ Métodos Custom (Promocionar, Actualizar, Banear)
   - ✓ Custom Transactions (CPs)
   - ✓ Filtros (ReadFilter)
6. **Muestra un resumen** con contadores de entidades creadas

**Salida esperada:**
```
=== Iniciando InitializeDb ===

1. Configurando base de datos...
   ✓ Conectado a SQL Server Express.
   (o)
   ✗ No se pudo conectar a SQL Server Express
   → Usando LocalDB como fallback...
   ✓ Configurado LocalDB.

2. Creando esquema de base de datos...
   ✓ Esquema creado correctamente.

3. Inicializando SessionFactory y repositorios...
   ✓ Componentes inicializados.

4. Creando entidades de prueba...
   --- USUARIOS ---
   ✓ Usuario creado: player1 (ID: 1)
   ✓ Usuario creado: player2 (ID: 2)
   ...

5. Probando métodos CUSTOM (CEN)...
   --- LOGIN ---
   ✓ Login exitoso: player1
   ...

6. Probando CUSTOM TRANSACTIONS (CP)...
   ✓ CP ejecutado: Usuario + Perfil creados
   ...

7. Probando FILTROS (ReadFilter)...
   ✓ Usuarios encontrados: 4
   ...

=== RESUMEN DE INICIALIZACIÓN ===
✓ Usuarios creados: 4
✓ Comunidades creadas: 3
✓ Equipos creados: 1
✓ Juegos creados: 2
✓ Torneos creados: 1
✓ Miembros comunidad: 2
✓ Miembros equipo: 1
✓ Publicaciones: 1

✓✓✓ InitializeDb COMPLETADO EXITOSAMENTE ✓✓✓
```

### Paso 6: Verificar la Base de Datos

**Si usaste SQL Server Express:**
```sql
-- Conéctate con SQL Server Management Studio (SSMS) o Azure Data Studio
Server: localhost\SQLEXPRESS
Database: ProjectDatabase
Authentication: Windows Authentication

-- Verifica las tablas creadas
SELECT * FROM Usuario;
SELECT * FROM Comunidad;
SELECT * FROM Equipo;
-- etc.
```

**Si usaste LocalDB:**
```powershell
# La base de datos se crea en:
# InitializeDb\bin\Debug\net8.0\Data\ProjectDatabase.mdf

# Conéctate con:
Server: (localdb)\MSSQLLocalDB
Database: ProjectDatabase
# O adjunta el archivo .mdf en SSMS
```

### Paso 7: Ejecutar Pruebas Manuales

Puedes modificar `InitializeDb/Program.cs` para probar tus propios escenarios:

```csharp
// Ejemplo: Crear un nuevo usuario y loguearse
var nuevoUsuario = usuarioCEN.Crear("testuser", "test@email.com", "hash123");
unitOfWork.SaveChanges();

var usuarioLogueado = usuarioCEN.Login("test@email.com", "hash123");
Console.WriteLine($"Usuario logueado: {usuarioLogueado.Nick}");
```

### Solución de Problemas Comunes

**Error: "No se encuentra el SDK de .NET"**
```powershell
# Instala .NET 8.0 SDK desde:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

**Error: "No se puede conectar a SQL Server"**
- El proyecto usa **LocalDB automáticamente** como fallback
- No necesitas configurar nada adicional
- LocalDB viene incluido con Visual Studio

**Error: "NuGet package not found"**
```powershell
# Limpia y restaura:
dotnet clean
dotnet restore --force
dotnet build
```

**Error: "The type or namespace name 'NHibernate' could not be found"**
```powershell
# Verifica que los paquetes se hayan instalado:
dotnet list package
# Restaura explícitamente:
dotnet restore Solution.sln
```

**Error: "Database already exists" al ejecutar InitializeDb**
```powershell
# InitializeDb hace DROP + CREATE automáticamente
# Si persiste el error, elimina manualmente la BD:
# - En SQL Server Express: DROP DATABASE ProjectDatabase;
# - En LocalDB: Elimina el archivo InitializeDb\bin\Debug\net8.0\Data\ProjectDatabase.mdf
```
