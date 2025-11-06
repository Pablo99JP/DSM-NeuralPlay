# InitializeDb

Este proyecto contiene el inicializador de base de datos que crea el esquema completo y ejecuta un seed con datos de prueba en **SQL Server LocalDB**.

## Descripción General

El sistema está configurado por defecto para trabajar con **LocalDB** y ejecutar automáticamente el seed de datos. Utiliza **NHibernate** como ORM con soporte para:
- **SQL Server LocalDB** (configuración por defecto)
- **SQL Server** (vía conexión personalizada)
- **SQLite** (fallback automático si LocalDB no está disponible)

## Uso Básico

### Ejecutar con configuración por defecto (LocalDB + Seed)

Desde el directorio `InitializeDb`:

```powershell
dotnet run
```

Esto creará automáticamente:
- Base de datos `ProjectDatabase` en LocalDB
- Esquema completo (22 tablas)
- Datos de prueba iniciales (usuarios, comunidades, equipos, etc.)

### Recrear la base de datos desde cero

Para eliminar y recrear completamente la base de datos:

```powershell
dotnet run -- --force-drop --confirm
```

## Opciones de Línea de Comandos

### Modos de Operación
- `--mode=schemaexport` (por defecto): Exporta el esquema a LocalDB/SQL Server y ejecuta el seed
- `--mode=inmemory`: Validación en memoria sin persistencia (útil para pruebas rápidas)

### Control de Base de Datos
- `--db-name=<nombre>`: Nombre de la base de datos (por defecto: `ProjectDatabase`)
- `--data-dir=<ruta>`: Carpeta para archivos MDF/LDF (por defecto: `InitializeDb/Data`)
- `--seed`: Ejecutar seed de datos (activado por defecto)
- `--force-drop`: Permite eliminar una base de datos existente (⚠️ destructivo)
- `--confirm`: Confirmación requerida junto a `--force-drop`

### Conexión Personalizada
- `--target-connection=<cadena>`: Cadena de conexión personalizada a SQL Server
- `--dialect=<dialecto>`: Dialecto NHibernate (ej: `NHibernate.Dialect.MsSql2012Dialect`)

### Logging
- `--log-file=<ruta>`: Archivo de log de Serilog
- `--verbose` o `-v`: Activa logging detallado (nivel Debug)

## Variables de Entorno

- `LOG_FILE`: Ruta del archivo de log (alternativa a `--log-file`)
- `LOG_VERBOSE`: Si es `true`, habilita nivel Debug
- `LOG_LEVEL`: Nivel explícito de Serilog (`Debug`, `Information`, `Warning`, `Error`)
- `LOCALDB_AVAILABLE`: Indicador para CI/CD

## Configuración de NHibernate

El archivo `NHibernate.cfg.xml` está configurado para usar:
- **Driver**: `NHibernate.Driver.MicrosoftDataSqlClientDriver`
- **Dialecto**: `NHibernate.Dialect.MsSql2012Dialect`
- **Conexión**: `Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Database=ProjectDatabase;TrustServerCertificate=true;`

## Ejemplos de Uso

### Crear base de datos con nombre personalizado

```powershell
dotnet run -- --db-name=MiBaseDeDatos
```

### Conectar a SQL Server remoto

```powershell
dotnet run -- --target-connection="Server=mi-servidor;Database=MiDB;User Id=usuario;Password=clave;" --dialect="NHibernate.Dialect.MsSql2012Dialect"
```

### Modo desarrollo: validación rápida en memoria

```powershell
dotnet run -- --mode=inmemory
```

### Recrear con logs detallados

```powershell
dotnet run -- --force-drop --confirm --verbose --log-file=./Data/init.log
```

## Datos de Seed

El seed crea automáticamente:

### Usuarios
- `alice` (alice@example.com) - Contraseña hasheada
- `bob` (bob@example.com) - Contraseña hasheada

### Comunidades
- `Gamers` - Comunidad de gaming

### Equipos
- `TeamA` - Equipo de ejemplo

### Relaciones
- Membresías de comunidad y equipo
- Roles asignados (líder, admin, miembro)

El seed es **idempotente**: verifica la existencia de datos antes de insertarlos, por lo que se puede ejecutar múltiples veces sin duplicar registros.

## Estructura de Archivos Generados

Después de ejecutar, en la carpeta `Data/` encontrarás:

```
Data/
├── ProjectDatabase.mdf          # Archivo de base de datos SQL Server
├── ProjectDatabase_log.ldf      # Log de transacciones SQL Server
├── ProjectDatabase_schema.sql   # Script SQL generado (si se usa sqlcmd)
└── init.log                      # Log de Serilog (si se especifica --log-file)
```

## Verificación de Resultados

Para verificar que la base de datos se creó correctamente:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProjectDatabase -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
```

Para ver los datos del seed:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProjectDatabase -Q "SELECT IdUsuario, Nick, CorreoElectronico FROM Usuario"
```

## Optimizaciones

### Filtrado en SQL
Todos los métodos `ReadFilter` de los repositorios NHibernate utilizan **LINQ to NHibernate**, lo que garantiza que:
- ✅ El filtrado se ejecuta directamente en SQL Server
- ✅ Se minimizan las transferencias de datos
- ✅ Se aprovechan los índices de la base de datos
- ✅ No se cargan datos innecesarios en memoria del backend

Ejemplo: `ReadByEmail("usuario@test.com")` genera:
```sql
SELECT TOP 1 * FROM Usuario WHERE LOWER(CorreoElectronico) = 'usuario@test.com'
```

## Troubleshooting

### Error: "Database already exists"
Usa `--force-drop --confirm` para eliminar y recrear

### LocalDB no disponible
El sistema intentará automáticamente usar SQLite como fallback

### Problemas de permisos
Asegúrate de ejecutar con permisos suficientes para crear archivos en `Data/`

### Ver consultas SQL generadas
Edita `NHibernate.cfg.xml` y cambia `show_sql` a `true`

## Notas para CI/CD

- El modo `--mode=schemaexport` es ideal para CI/CD
- Exporta las variables de entorno `LOG_FILE`, `LOG_VERBOSE` y `LOCALDB_AVAILABLE`
- Los artefactos (logs, MDF) se pueden recopilar desde `--data-dir`
- Para tests de integración, usa bases de datos temporales con `--db-name=test_db_${BUILD_ID}`

## Arquitectura

El proyecto utiliza:
- **ApplicationCore**: Lógica de negocio (CEN), entidades (EN) y repositorios
- **Infrastructure.NHibernate**: Implementación de repositorios con NHibernate
- **Infrastructure.Memory**: Repositorios en memoria para el modo `inmemory`
- **Serilog**: Logging estructurado con sinks de consola y archivo
