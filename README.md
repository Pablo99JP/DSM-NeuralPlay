# NeuralPlay

Aplicación web ASP.NET Core con vistas Razor que gestiona comunidades, equipos, torneos y chat. Usa NHibernate para persistencia y un inicializador de BD para crear y poblar el esquema en LocalDB/SQL Server.

## Arquitectura de Proyectos
- `ApplicationCore`: Entidades (EN), enums y lógica de negocio (CEN, CP futuros) sobre interfaces de repositorio.
- `Infrastructure`: Implementaciones de repositorios (NHibernate), mapeos `*.hbm.xml` y `NHibernate.cfg.xml`.
- `InitializeDb`: Inicialización de base de datos (export de esquema y seed de datos).
- `NeuralPlay`: Aplicación web con controladores y vistas Razor.

## Entidades y funcionamiento general
- `Usuario`: Datos de cuenta y perfil. Permite autenticación, edición del perfil y relación con comunidades/equipos.
- `Comunidad`: Agrupa usuarios por intereses. Se puede crear, listar, unirse/salir y gestionar miembros y roles.
- `Equipo`: Grupo dentro de una comunidad orientado a juego/competición. Crear/editar, invitar/gestionar miembros y acceder a su chat.
- `Torneo`: Competición entre equipos/usuarios. Crear torneos, inscribir equipos, gestionar fases y consultar resultados.
- `MensajeChat` (Chat de Equipo): Mensajes por equipo persistidos con NHibernate; vistas Razor para leer/enviar.
- `Juego`: Catálogo/afinidad de juegos asociados a usuarios y equipos. Añadir/quitar juegos del perfil o del equipo.
- `Perfil`: Información pública del usuario (nick, bio, juegos, imagen). Edición desde la sección de perfil.
- `Membresías/Roles`: Relaciones `Usuario↔Comunidad` y `Usuario↔Equipo` con roles (líder, admin, miembro) que condicionan permisos.

## Secciones que requieren inicio de sesión
- `Equipo/Chat`: ver y enviar mensajes requiere sesión y pertenecer al equipo.
- Gestión de `Equipos`: crear/editar, invitar/expulsar, cambiar roles.
- Gestión de `Comunidades`: crear/editar, unirse/salir, administrar miembros.
- `Torneos`: crear torneo e inscribir equipos; consulta pública puede ser anónima.
- `Perfil` de usuario: ver propio perfil completo y editar.
- `Perfiles/AnadirJuego`: añadir juegos al perfil.

Acceso anónimo (lectura) según configuración:
- Listados públicos de comunidades, equipos y torneos.
- Páginas de inicio y contenido informativo.

## Persistencia y configuración
- ORM: NHibernate con mapeos en `Infrastructure/NHibernate/Mappings` y configuración en `Infrastructure/NHibernate/NHibernate.cfg.xml`.
- Helper: `Infrastructure/NHibernate/NHibernateHelper.cs` construye `ISessionFactory`, detecta `cfg.xml`, añade mapeos y puede crear la BD si no existe.
- BD por defecto: SQL Server LocalDB (`ProjectDatabase`). Fallback opcional a SQLite con `NP_USE_SQLITE=1`.

## Inicialización de Base de Datos
Usa `InitializeDb` para crear el esquema y seed:
- `dotnet run` desde `InitializeDb` crea la BD y datos de ejemplo.
- Opciones: `--force-drop --confirm` para recreación; más en `InitializeDb/README.md`.

## Seguridad y roles
- Autenticación para acciones de escritura: chat, creación/edición, inscripciones.
- Autorización por rol en equipo/comunidad: líderes/admins administran miembros y roles.

## Puntos de entrada
- Controladores y vistas: `NeuralPlay/Controllers` y `NeuralPlay/Views` (p.ej., `EquipoController`, `Views/Equipo/Chat.cshtml`).
- Lógica de negocio: `ApplicationCore/Domain/CEN` (p.ej., `TorneoCEN`, `MensajeChatCEN`).

## Desarrollo
- Configuración: `NeuralPlay/appsettings.json` y `Infrastructure/NHibernate/NHibernate.cfg.xml`.
- Para ver SQL: `show_sql=true` en NHibernate.

## Requisitos
- .NET8/9 SDK
- SQL Server LocalDB (o SQL Server)

## Licencia
Proyecto académico.
