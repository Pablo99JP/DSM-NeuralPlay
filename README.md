# Proyecto Clean Architecture DDD - Monolito

Este proyecto implementa una arquitectura Clean DDD basada en el modelo de dominio generado desde `dominio.puml`.

## 📚 Documentación

La documentación completa del proyecto está organizada en archivos específicos dentro de la carpeta `docs/`:

1. **[Estructura del Proyecto](docs/01_ESTRUCTURA.md)**
   - Arquitectura y organización de carpetas
   - Archivos generados (EN, Enums, CENs, CPs, Repositories)
   - Convenciones aplicadas
   - Tecnologías y requisitos

2. **[Guía de Compilación y Pruebas](docs/02_COMPILACION_Y_PRUEBAS.md)**
   - Requisitos previos (.NET 8.0, SQL Server/LocalDB)
   - Pasos para compilar el proyecto
   - Cómo ejecutar InitializeDb
   - Verificación de base de datos
   - Solución de problemas comunes

3. **[Integración con Frontend](docs/03_INTEGRACION_FRONTEND.md)**
   - Cómo crear Web API
   - Configuración de Dependency Injection
   - Ejemplos de Controllers (UsuarioController)
   - Ejemplos de consumo desde React/TypeScript
   - Ventajas de la arquitectura

4. **[Flujo de Lógica de Negocio](docs/04_FLUJO_LOGICA_NEGOCIO.md)**
   - Diagrama de arquitectura en capas
   - Flujo completo de una operación (Registro de Usuario)
   - Flujo de métodos custom (Login)
   - Referencias entre archivos y líneas de código
   - SQL generado por NHibernate

5. **[Funcionalidades Implementadas](docs/05_FUNCIONALIDADES.md)**
   - CRUD completo (10 CENs)
   - 6 Métodos Custom (Login, PromoverAModerador, BanearMiembro, etc.)
   - 12 ReadFilters (generales + específicos)
   - 4 Custom Transactions (CPs transaccionales)
   - Reglas de negocio documentadas
   - InitializeDb completo

## 🚀 Inicio Rápido

### Compilar y Ejecutar

```powershell
# 1. Restaurar dependencias
dotnet restore Solution.sln

# 2. Compilar
dotnet build Solution.sln --configuration Release

# 3. Inicializar base de datos y seed
cd InitializeDb
dotnet run
```

### Resultado Esperado

```
✓ ApplicationCore realizado correctamente
✓ Infrastructure realizado correctamente
✓ InitializeDb realizado correctamente

=== Iniciando InitializeDb ===
✓ Conectado a SQL Server Express (o LocalDB)
✓ Esquema creado correctamente
✓ Usuarios creados: 4
✓ Comunidades creadas: 3
✓ Equipos creados: 1
✓✓✓ InitializeDb COMPLETADO EXITOSAMENTE ✓✓✓
```

## ⚡ Resumen del Proyecto

### Arquitectura
- **Clean Architecture + DDD**: Separación clara entre dominio e infraestructura
- **NHibernate ORM**: Persistencia con mappings XML
- **.NET 8.0**: Framework moderno y eficiente

### Componentes Principales
- **21 Entidades** (Usuario, Comunidad, Equipo, Torneo, Invitacion, ChatEquipo, MensajeChat, Comentario, Reaccion, Notificacion, PropuestaTorneo, VotoTorneo, ParticipacionTorneo, PerfilJuego, Sesion, etc.)
- **11 Enums** (RolComunidad, EstadoMembresia, TipoNotificacion, TipoInvitacion, EstadoSolicitud, TipoReaccion, etc.)
- **21 CENs** con CRUD completo + 6 métodos custom
- **4 CPs** transaccionales (RegistroUsuarioCP, CrearComunidadCP, AceptarInvitacionEquipoCP, AprobarPropuestaTorneoCP)
- **12 ReadFilters** (8 generales + 4 específicos)

Ver detalles completos en **[Estructura del Proyecto](docs/01_ESTRUCTURA.md)** y **[Funcionalidades](docs/05_FUNCIONALIDADES.md)**

## 📖 Documentación Adicional

- **[IMPLEMENTACIONES.md](IMPLEMENTACIONES.md)** - Detalle técnico completo de todas las implementaciones
- **[VERIFICACION_REQUISITOS.md](VERIFICACION_REQUISITOS.md)** - Verificación exhaustiva de requisitos cumplidos

## 🛠️ Tecnologías

- **.NET 8.0** - Framework principal
- **NHibernate 5.5.2** - ORM con mappings XML
- **SQL Server Express / LocalDB** - Base de datos
- **Clean Architecture + DDD** - Patrón arquitectónico

## 📝 Notas Importantes

- Las entidades NO tienen referencias a Entity Framework o NHibernate (POCOs puros)
- Los CENs solo exponen operaciones sobre UNA entidad
- Los CPs orquestan MÚLTIPLES CENs y aplican lógica transaccional
- Generador HiLo para IDs eficiente sin round-trips a BD
- Todas las operaciones son síncronas según especificación
- Validaciones de negocio centralizadas en CENs y CPs

---

**Para información detallada, consulta los archivos de documentación en la carpeta `docs/`**
