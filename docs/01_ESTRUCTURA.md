# Estructura del Proyecto

## Arquitectura del Proyecto

```
Solution.sln
├─ ApplicationCore/
│  └─ Domain/
│     ├─ EN/                    # Entidades de dominio (POCO)
│     ├─ Enums/                 # Enums del dominio
│     ├─ Repositories/          # Interfaces de repositorio y IUnitOfWork
│     ├─ CEN/                   # Componentes Entidad de Negocio
│     └─ CP/                    # Casos de Proceso (use cases)
├─ Infrastructure/
│  ├─ NHibernate/
│  │  ├─ Mappings/             # Archivos .hbm.xml
│  │  ├─ Repositories/         # Implementaciones concretas de repositorios
│  │  └─ NHibernateHelper.cs   # Helper para configuración
│  └─ UnitOfWork/              # Implementación de UnitOfWork
└─ InitializeDb/               # Proyecto ejecutable de inicialización
```

## Archivos Generados

### 1. domain.model.json
Modelo de dominio generado desde `dominio.puml` con todas las entidades, enums, propiedades y relaciones.

### 2. ApplicationCore
- **EN (Entidades)**: 21 clases POCO con propiedades virtuales para NHibernate (Usuario, Comunidad, Equipo, etc.)
- **Enums**: 11 enums (RolComunidad, EstadoMembresia, TipoNotificacion, etc.)
- **Repositories**: Interfaces de repositorio con métodos síncronos (damePorOID, dameTodos, New, Modify, Destroy, ModifyAll)
  - Filtros adicionales implementados:
    - `IEquipoRepository.DamePorTorneo(idTorneo)`: Equipos participando en un torneo
    - `ITorneoRepository.DamePorEquipo(idEquipo)`: Torneos donde participa un equipo
    - `IUsuarioRepository.DamePorEquipo(idEquipo)`: Usuarios miembros de un equipo
    - `IUsuarioRepository.DamePorComunidad(idComunidad)`: Usuarios miembros de una comunidad
- **CEN**: 21 Componentes de negocio por entidad con CRUD completo:
  - UsuarioCEN, ComunidadCEN, EquipoCEN, MiembroComunidadCEN, MiembroEquipoCEN
  - JuegoCEN, PerfilCEN, TorneoCEN, PublicacionCEN, SolicitudIngresoCEN
  - InvitacionCEN, ChatEquipoCEN, MensajeChatCEN, ComentarioCEN, ReaccionCEN
  - NotificacionCEN, PropuestaTorneoCEN, VotoTorneoCEN, ParticipacionTorneoCEN
  - PerfilJuegoCEN, SesionCEN
  - Métodos custom implementados:
    - `MiembroComunidadCEN.PromoverAModerador(id)`: Promociona miembro a colaborador
    - `MiembroComunidadCEN.ActualizarFechaAccion(id, fecha)`: Actualiza fecha de alta
    - `MiembroEquipoCEN.BanearMiembro(id)`: Expulsa miembro de equipo
  - Reglas de negocio:
    - Usuario siempre se crea con `EstadoCuenta.ACTIVA`
    - MiembroComunidad y MiembroEquipo se crean sin `FechaBaja`
    - SolicitudIngreso valida que usuario no esté ya en equipo de la comunidad
- **CP**: Casos de proceso que orquestan múltiples CENs (RegistroUsuarioCP, CrearComunidadCP, AceptarInvitacionEquipoCP, AprobarPropuestaTorneoCP)

### 3. Infrastructure
- **Mappings XML**: Archivos `.hbm.xml` para NHibernate con generador HiLo
- **Repositorios NHibernate**: Implementaciones concretas usando ISession
- **UnitOfWork**: Implementación NHibernate para gestión transaccional
- **NHibernateHelper**: Carga configuración y SessionFactory

### 4. InitializeDb
Proyecto ejecutable que:
- Crea la base de datos (LocalDB o SQL Express)
- Ejecuta SchemaExport para crear tablas
- Incluye fallback automático a LocalDB si SQL Express no está disponible

## Convenciones Aplicadas

- **IDs**: Tipo `long` con generador HiLo de NHibernate
- **Propiedades**: Virtuales para lazy loading de NHibernate
- **Repositorios**: Métodos síncronos según especificación
- **CENs**: Métodos crear/modificar con solo parámetros obligatorios
- **Mappings**: XML sin duplicación de FKs (solo asociaciones many-to-one)

## Tecnologías

- .NET 8.0
- NHibernate 5.5.2
- SQL Server / LocalDB
- Clean Architecture + DDD

## Requisitos

- .NET 8.0 SDK
- SQL Server Express (localhost\SQLEXPRESS) o LocalDB
- SQL Server Browser en ejecución (para SQL Express)

## Cadenas de Conexión

### SQL Server Express (por defecto)
```
Server=localhost\SQLEXPRESS;Database=ProjectDatabase;Integrated Security=True;
```

### LocalDB (fallback automático)
```
Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectDatabase;Integrated Security=True;AttachDBFilename=|DataDirectory|\ProjectDatabase.mdf
```
