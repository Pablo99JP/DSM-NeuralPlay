# NeuralPlay

Aplicación web ASP.NET Core con vistas Razor que gestiona comunidades, equipos, torneos y chat. Usa NHibernate para persistencia y un inicializador de BD para crear y poblar el esquema en LocalDB/SQL Server.

## Arquitectura de Proyectos
- `ApplicationCore`: Entidades (EN), enums y lógica de negocio (CEN, CP futuros) sobre interfaces de repositorio.
- `Infrastructure`: Implementaciones de repositorios (NHibernate), mapeos `*.hbm.xml` y `NHibernate.cfg.xml`.
- `InitializeDb`: Inicialización de base de datos (export de esquema y seed de datos).
- `NeuralPlay`: Aplicación web con controladores y vistas Razor.

## Funcionamiento general
- `Usuario`: Datos de cuenta y perfil. Relación con comunidades/equipos. Se puede hacer CRUD desde la url "/usuario" de cualquier usuario.
- `Comunidad`: Agrupa usuarios por intereses. Se puede crear, listar, unirse/salir y gestionar miembros y roles. Los miembros se representan desde la clase MiembroComunidad que permite CRUD completo.
- `Equipo`: Grupo orientado para competir. Crear/editar, invitar/gestionar miembros y acceder a su chat. Los miembros se representan desde la clase MiembroEquipo que permite CRUD completo.
- `Torneo`: Competición entre equipos/usuarios. Crear torneos, inscribir equipos, gestionar fases y consultar resultados. Los torneos lo crean líderes o moderadores de comunidades. Luego un miembro de un equipo puede solicitar a su equipo unirse a ese torneo. Los participantes del equipo deberán votar si unen, se aprueba por unaminidad. Para que el estado del torneo se abra, deben estar al menos dos equipos dentro (cuyos miembros han votado estarlo). En las reglas (o descripción) del torneo es donde especificarán sus creadores donde y como se jugará el torneo (esto pasaría fuera de nuestra aplicación). Todo esto, para poder probarlo (CRUD completa), debes estar iniciado con el respectivo usuario en cada situación.
- `Chat` (Chat de Equipo): Espacio donde los miembros del equipo (en esta entrega, cualquier usuario loggeado) puede escribir y publicar mensajes para que se queden en la vista del chat de cada equipo. No se ha implementado un controlador CRUD para los mensajes como entidad porque realmente no tiene sentido en nuestra app poder editar y borrar mensajes de texto que se mandan a un grupo. Se podría implementar de cara a la entrega final si se nos indicase que es un requisito obligatorio más.
- `Juego`: Catálogo/afinidad de juegos asociados a usuarios y equipos. Añadir/quitar juegos del perfil o del equipo. CRUD disponible para cualquier usuario desde home.
- `Perfil`: Información pública del usuario (nick, bio, juegos, imagen). Edición desde la sección de perfil. En el caso de estar iniciado puedes editar tu biografia y nombre, se pueden editar los juegos asociados al perfil de cualquier usuario.
- `Notificacion`: Sistema de avisos para usuarios (reacciones, comentarios, propuestas de torneo, mensajes). Incluye tipo, mensaje, estado de lectura y fecha. Permite marcar como leída y filtrar por usuario. Debes estar iniciado para poder acceder a las notificaciones de ese usuario.
- `Invitacion`: Permite a usuarios invitar a otros a unirse a comunidades o equipos. Incluye emisor, destinatario, tipo (comunidad/equipo), estado (pendiente/aceptada/rechazada) y fechas de envío/respuesta. Permite crearla, eliminarla o ver detalle, hace falta estar iniciado para verlas pero puedes ver la de cualquier usuario.
- `SolicitudIngreso`: Solicitud de un usuario para unirse a una comunidad o equipo. Incluye solicitante, entidad destino, estado y fechas. Puede ser aprobada o rechazada de momento por cualquier usuario iniciado.
- `Publicacion`: Contenido compartido en comunidades por usuarios. Incluye autor, contenido, fechas de creación/edición y puede recibir comentarios y reacciones. Permite hacer CRUD completo y sin estar iniciado.
- `Comentario`: Respuesta a una publicación. Incluye autor, contenido, fechas y puede recibir reacciones. Permite conversaciones en publicaciones. Puedes hacer CRUD completo de tus propios comentarios.
- `Reaccion`: "Me gusta" se puede dar a publicaciones o comentarios. Hace falta estar iniciado y por cada usuario que da "me gusta", el total es acumulativo.
- `Membresías/Roles`: Relaciones `Usuario↔Comunidad` y `Usuario↔Equipo` con roles (líder, admin, miembro) que condicionan permisos.

## Login
La aplicación utiliza autenticación basada en sesiones. El usuario introduce correo y contraseña en `/Login`, y si son válidas, se almacena su identificador en la sesión. Un servicio de autenticación recupera el usuario actual para controlar permisos. Muchas funcionalidades (chat, gestión de miembros, votaciones) requieren estar autenticado; otras (ver comunidades, torneos) son públicas. Para cerrar sesión se accede a `/Logout`.

## Elementos select
Varias entidades utilizan listas desplegables (select) para relacionar información con otras entidades:
- **MiembroComunidad**: Selecciona usuarios de la lista completa, comunidades disponibles, y roles (LIDER, COLABORADOR, MODERADOR, MIEMBRO). También permite elegir estados de membresía (PENDIENTE, ACTIVA, EXPULSADA, ABANDONADA).
- **MiembroEquipo**: Selecciona usuarios, equipos existentes, y roles de equipo (ADMIN, MIEMBRO). Incluye estados de membresía.
- **Invitacion**: Desplegables para elegir emisor y destinatario (usuarios), así como comunidad o equipo destino según el tipo de invitación.
- **Perfil**: Permite seleccionar juegos de un catálogo completo para asociarlos al perfil del usuario o establecer uno como favorito.

Estos selects facilitan la gestión de relaciones entre entidades, evitando ingresar IDs manualmente y mejorando la experiencia de usuario.

## Persistencia y configuración
- ORM: NHibernate con mapeos en `Infrastructure/NHibernate/Mappings` y configuración en `Infrastructure/NHibernate/NHibernate.cfg.xml`.
- Helper: `Infrastructure/NHibernate/NHibernateHelper.cs` construye `ISessionFactory`, detecta `cfg.xml`, añade mapeos y puede crear la BD si no existe.
- BD por defecto: SQL Server LocalDB (`ProjectDatabase`). Fallback opcional a SQLite con `NP_USE_SQLITE=1`.

## Inicialización de Base de Datos
Usa `InitializeDb` para crear el esquema y seed:
- `dotnet run` desde `InitializeDb` crea la BD y datos de ejemplo.
- Opciones: `--force-drop --confirm` para recreación; más en `InitializeDb/README.md`.

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
