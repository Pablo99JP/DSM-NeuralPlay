if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_98C3C5C0]') and parent_object_id = OBJECT_ID(N'Comentario'))
alter table Comentario  drop constraint FK_98C3C5C0

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_4956C47E]') and parent_object_id = OBJECT_ID(N'Comentario'))
alter table Comentario  drop constraint FK_4956C47E

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_8E78311A]') and parent_object_id = OBJECT_ID(N'Equipo'))
alter table Equipo  drop constraint FK_8E78311A

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_293234A2]') and parent_object_id = OBJECT_ID(N'Invitacion'))
alter table Invitacion  drop constraint FK_293234A2

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_565EAFEA]') and parent_object_id = OBJECT_ID(N'Invitacion'))
alter table Invitacion  drop constraint FK_565EAFEA

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_47B7F244]') and parent_object_id = OBJECT_ID(N'Invitacion'))
alter table Invitacion  drop constraint FK_47B7F244

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_BD9226F9]') and parent_object_id = OBJECT_ID(N'Invitacion'))
alter table Invitacion  drop constraint FK_BD9226F9

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_3EC3241E]') and parent_object_id = OBJECT_ID(N'MensajeChat'))
alter table MensajeChat  drop constraint FK_3EC3241E

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_F6E66199]') and parent_object_id = OBJECT_ID(N'MensajeChat'))
alter table MensajeChat  drop constraint FK_F6E66199

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_F0CE6A4F]') and parent_object_id = OBJECT_ID(N'MiembroComunidad'))
alter table MiembroComunidad  drop constraint FK_F0CE6A4F

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_648E734F]') and parent_object_id = OBJECT_ID(N'MiembroComunidad'))
alter table MiembroComunidad  drop constraint FK_648E734F

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_92569281]') and parent_object_id = OBJECT_ID(N'MiembroEquipo'))
alter table MiembroEquipo  drop constraint FK_92569281

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_327CEBF3]') and parent_object_id = OBJECT_ID(N'MiembroEquipo'))
alter table MiembroEquipo  drop constraint FK_327CEBF3

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_9BA3D3D4]') and parent_object_id = OBJECT_ID(N'Notificacion'))
alter table Notificacion  drop constraint FK_9BA3D3D4

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_CB5ABC08]') and parent_object_id = OBJECT_ID(N'ParticipacionTorneo'))
alter table ParticipacionTorneo  drop constraint FK_CB5ABC08

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_D61144E]') and parent_object_id = OBJECT_ID(N'ParticipacionTorneo'))
alter table ParticipacionTorneo  drop constraint FK_D61144E

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Perfil_Usuario]') and parent_object_id = OBJECT_ID(N'Perfil'))
alter table Perfil  drop constraint FK_Perfil_Usuario

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_C590C1B4]') and parent_object_id = OBJECT_ID(N'PerfilJuego'))
alter table PerfilJuego  drop constraint FK_C590C1B4

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_3A792974]') and parent_object_id = OBJECT_ID(N'PerfilJuego'))
alter table PerfilJuego  drop constraint FK_3A792974

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_249F0BAF]') and parent_object_id = OBJECT_ID(N'PropuestaTorneo'))
alter table PropuestaTorneo  drop constraint FK_249F0BAF

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_98D56DAC]') and parent_object_id = OBJECT_ID(N'PropuestaTorneo'))
alter table PropuestaTorneo  drop constraint FK_98D56DAC

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_8CA6CB43]') and parent_object_id = OBJECT_ID(N'PropuestaTorneo'))
alter table PropuestaTorneo  drop constraint FK_8CA6CB43

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_878B07D4]') and parent_object_id = OBJECT_ID(N'Publicacion'))
alter table Publicacion  drop constraint FK_878B07D4

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_289AFC11]') and parent_object_id = OBJECT_ID(N'Publicacion'))
alter table Publicacion  drop constraint FK_289AFC11

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_379F6F47]') and parent_object_id = OBJECT_ID(N'Reaccion'))
alter table Reaccion  drop constraint FK_379F6F47

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_12066B6D]') and parent_object_id = OBJECT_ID(N'Reaccion'))
alter table Reaccion  drop constraint FK_12066B6D

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_DAB67978]') and parent_object_id = OBJECT_ID(N'Reaccion'))
alter table Reaccion  drop constraint FK_DAB67978

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_5902CC39]') and parent_object_id = OBJECT_ID(N'Sesion'))
alter table Sesion  drop constraint FK_5902CC39

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_2E439B98]') and parent_object_id = OBJECT_ID(N'SolicitudIngreso'))
alter table SolicitudIngreso  drop constraint FK_2E439B98

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_F95BA7F0]') and parent_object_id = OBJECT_ID(N'SolicitudIngreso'))
alter table SolicitudIngreso  drop constraint FK_F95BA7F0

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_13422D2A]') and parent_object_id = OBJECT_ID(N'SolicitudIngreso'))
alter table SolicitudIngreso  drop constraint FK_13422D2A

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_F939C77]') and parent_object_id = OBJECT_ID(N'Torneo'))
alter table Torneo  drop constraint FK_F939C77

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_E3777048]') and parent_object_id = OBJECT_ID(N'VotoTorneo'))
alter table VotoTorneo  drop constraint FK_E3777048

if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_A853BA31]') and parent_object_id = OBJECT_ID(N'VotoTorneo'))
alter table VotoTorneo  drop constraint FK_A853BA31

if exists (select * from dbo.sysobjects where id = object_id(N'ChatEquipo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table ChatEquipo
if exists (select * from dbo.sysobjects where id = object_id(N'Comentario') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Comentario
if exists (select * from dbo.sysobjects where id = object_id(N'Comunidad') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Comunidad
if exists (select * from dbo.sysobjects where id = object_id(N'Equipo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Equipo
if exists (select * from dbo.sysobjects where id = object_id(N'Invitacion') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Invitacion
if exists (select * from dbo.sysobjects where id = object_id(N'Juego') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Juego
if exists (select * from dbo.sysobjects where id = object_id(N'MensajeChat') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table MensajeChat
if exists (select * from dbo.sysobjects where id = object_id(N'MiembroComunidad') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table MiembroComunidad
if exists (select * from dbo.sysobjects where id = object_id(N'MiembroEquipo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table MiembroEquipo
if exists (select * from dbo.sysobjects where id = object_id(N'Notificacion') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Notificacion
if exists (select * from dbo.sysobjects where id = object_id(N'ParticipacionTorneo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table ParticipacionTorneo
if exists (select * from dbo.sysobjects where id = object_id(N'Perfil') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Perfil
if exists (select * from dbo.sysobjects where id = object_id(N'PerfilJuego') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PerfilJuego
if exists (select * from dbo.sysobjects where id = object_id(N'PropuestaTorneo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PropuestaTorneo
if exists (select * from dbo.sysobjects where id = object_id(N'Publicacion') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Publicacion
if exists (select * from dbo.sysobjects where id = object_id(N'Reaccion') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Reaccion
if exists (select * from dbo.sysobjects where id = object_id(N'Sesion') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Sesion
if exists (select * from dbo.sysobjects where id = object_id(N'SolicitudIngreso') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table SolicitudIngreso
if exists (select * from dbo.sysobjects where id = object_id(N'Torneo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Torneo
if exists (select * from dbo.sysobjects where id = object_id(N'Usuario') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Usuario
if exists (select * from dbo.sysobjects where id = object_id(N'VotoTorneo') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table VotoTorneo
if exists (select * from dbo.sysobjects where id = object_id(N'NHibernateUniqueKey') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table NHibernateUniqueKey
create table ChatEquipo (IdChatEquipo BIGINT IDENTITY NOT NULL, primary key (IdChatEquipo))
create table Comentario (IdComentario BIGINT IDENTITY NOT NULL, Contenido NVARCHAR(255) null, FechaCreacion DATETIME2 null, FechaEdicion DATETIME2 null, AutorId BIGINT null, PublicacionId BIGINT null, primary key (IdComentario))
create table Comunidad (IdComunidad BIGINT IDENTITY NOT NULL, Nombre NVARCHAR(255) null, Descripcion NVARCHAR(255) null, FechaCreacion DATETIME2 null, primary key (IdComunidad))
create table Equipo (IdEquipo BIGINT IDENTITY NOT NULL, Nombre NVARCHAR(255) null, Descripcion NVARCHAR(255) null, FechaCreacion DATETIME2 null, ComunidadId BIGINT null, primary key (IdEquipo))
create table Invitacion (IdInvitacion BIGINT IDENTITY NOT NULL, Tipo INT null, Estado INT null, FechaEnvio DATETIME2 null, FechaRespuesta DATETIME2 null, EmisorId BIGINT null, DestinatarioId BIGINT null, ComunidadId BIGINT null, EquipoId BIGINT null, primary key (IdInvitacion))
create table Juego (IdJuego BIGINT IDENTITY NOT NULL, NombreJuego NVARCHAR(255) null, Genero INT null, primary key (IdJuego))
create table MensajeChat (IdMensajeChat BIGINT IDENTITY NOT NULL, Contenido NVARCHAR(255) null, FechaEnvio DATETIME2 null, ChatId BIGINT null, AutorId BIGINT null, primary key (IdMensajeChat))
create table MiembroComunidad (IdMiembroComunidad BIGINT IDENTITY NOT NULL, Rol INT null, Estado INT null, FechaAlta DATETIME2 null, FechaAccion DATETIME2 null, FechaBaja DATETIME2 null, UsuarioId BIGINT null, ComunidadId BIGINT null, primary key (IdMiembroComunidad))
create table MiembroEquipo (IdMiembroEquipo BIGINT IDENTITY NOT NULL, Rol INT null, Estado INT null, FechaAlta DATETIME2 null, FechaAccion DATETIME2 null, FechaBaja DATETIME2 null, UsuarioId BIGINT null, EquipoId BIGINT null, primary key (IdMiembroEquipo))
create table Notificacion (IdNotificacion BIGINT IDENTITY NOT NULL, Tipo INT null, Mensaje NVARCHAR(255) null, Leida BIT null, FechaCreacion DATETIME2 null, DestinatarioId BIGINT null, primary key (IdNotificacion))
create table ParticipacionTorneo (IdParticipacion BIGINT IDENTITY NOT NULL, FechaAlta DATETIME2 null, Estado NVARCHAR(255) null, EquipoId BIGINT null, TorneoId BIGINT null, primary key (IdParticipacion))
create table Perfil (IdPerfil BIGINT NOT NULL, FotoPerfilUrl NVARCHAR(255) null, Descripcion NVARCHAR(255) null, VisibilidadPerfil INT null, VisibilidadActividad INT null, JuegoFavoritoId BIGINT null, primary key (IdPerfil))
create table PerfilJuego (IdPerfilJuego BIGINT IDENTITY NOT NULL, FechaAdicion DATETIME2 null, PerfilId BIGINT null, JuegoId BIGINT null, primary key (IdPerfilJuego))
create table PropuestaTorneo (IdPropuesta BIGINT IDENTITY NOT NULL, FechaPropuesta DATETIME2 null, Estado INT null, EquipoId BIGINT null, TorneoId BIGINT null, PropuestoPorId BIGINT null, primary key (IdPropuesta))
create table Publicacion (IdPublicacion BIGINT IDENTITY NOT NULL, Contenido NVARCHAR(255) null, FechaCreacion DATETIME2 null, FechaEdicion DATETIME2 null, ComunidadId BIGINT null, AutorId BIGINT null, primary key (IdPublicacion))
create table Reaccion (IdReaccion BIGINT IDENTITY NOT NULL, Tipo INT null, FechaCreacion DATETIME2 null, AutorId BIGINT null, PublicacionId BIGINT null, ComentarioId BIGINT null, primary key (IdReaccion))
create table Sesion (IdSesion BIGINT IDENTITY NOT NULL, FechaInicio DATETIME2 null, FechaFin DATETIME2 null, Token NVARCHAR(255) null, UsuarioId BIGINT null, primary key (IdSesion))
create table SolicitudIngreso (IdSolicitud BIGINT IDENTITY NOT NULL, Tipo INT null, Estado INT null, FechaSolicitud DATETIME2 null, FechaResolucion DATETIME2 null, SolicitanteId BIGINT null, ComunidadId BIGINT null, EquipoId BIGINT null, primary key (IdSolicitud))
create table Torneo (IdTorneo BIGINT IDENTITY NOT NULL, Nombre NVARCHAR(255) null, FechaInicio DATETIME2 null, Reglas NVARCHAR(255) null, Estado NVARCHAR(255) null, ComunidadId BIGINT null, primary key (IdTorneo))
create table Usuario (IdUsuario BIGINT IDENTITY NOT NULL, Nick NVARCHAR(255) null, CorreoElectronico NVARCHAR(255) null, ContrasenaHash NVARCHAR(255) null, Telefono NVARCHAR(255) null, FechaRegistro DATETIME2 null, EstadoCuenta INT null, primary key (IdUsuario))
create table VotoTorneo (IdVoto BIGINT IDENTITY NOT NULL, Valor BIT null, FechaVoto DATETIME2 null, UsuarioId BIGINT null, PropuestaId BIGINT null, primary key (IdVoto))
alter table Comentario add constraint FK_98C3C5C0 foreign key (AutorId) references Usuario
alter table Comentario add constraint FK_4956C47E foreign key (PublicacionId) references Publicacion
alter table Equipo add constraint FK_8E78311A foreign key (ComunidadId) references Comunidad
alter table Invitacion add constraint FK_293234A2 foreign key (EmisorId) references Usuario
alter table Invitacion add constraint FK_565EAFEA foreign key (DestinatarioId) references Usuario
alter table Invitacion add constraint FK_47B7F244 foreign key (ComunidadId) references Comunidad
alter table Invitacion add constraint FK_BD9226F9 foreign key (EquipoId) references Equipo
alter table MensajeChat add constraint FK_3EC3241E foreign key (ChatId) references ChatEquipo
alter table MensajeChat add constraint FK_F6E66199 foreign key (AutorId) references Usuario
alter table MensajeChat add constraint FK_F6E66199 foreign key (AutorId) references Usuario
alter table MiembroComunidad add constraint FK_F0CE6A4F foreign key (UsuarioId) references Usuario
alter table MiembroComunidad add constraint FK_648E734F foreign key (ComunidadId) references Comunidad
alter table MiembroEquipo add constraint FK_92569281 foreign key (UsuarioId) references Usuario
alter table MiembroEquipo add constraint FK_327CEBF3 foreign key (EquipoId) references Equipo
alter table Notificacion add constraint FK_9BA3D3D4 foreign key (DestinatarioId) references Usuario
alter table ParticipacionTorneo add constraint FK_CB5ABC08 foreign key (EquipoId) references Equipo
alter table ParticipacionTorneo add constraint FK_D61144E foreign key (TorneoId) references Torneo
alter table Perfil add constraint FK_1503F34C foreign key (IdPerfil) references Usuario
alter table PerfilJuego add constraint FK_C590C1B4 foreign key (PerfilId) references Perfil
alter table PerfilJuego add constraint FK_3A792974 foreign key (JuegoId) references Juego
alter table PropuestaTorneo add constraint FK_249F0BAF foreign key (EquipoId) references Equipo
alter table PropuestaTorneo add constraint FK_98D56DAC foreign key (TorneoId) references Torneo
alter table PropuestaTorneo add constraint FK_8CA6CB43 foreign key (PropuestoPorId) references Usuario
alter table Publicacion add constraint FK_878B07D4 foreign key (ComunidadId) references Comunidad
alter table Publicacion add constraint FK_289AFC11 foreign key (AutorId) references Usuario
alter table Reaccion add constraint FK_379F6F47 foreign key (AutorId) references Usuario
alter table Reaccion add constraint FK_12066B6D foreign key (PublicacionId) references Publicacion
alter table Reaccion add constraint FK_DAB67978 foreign key (ComentarioId) references Comentario
alter table Sesion add constraint FK_5902CC39 foreign key (UsuarioId) references Usuario
alter table SolicitudIngreso add constraint FK_2E439B98 foreign key (SolicitanteId) references Usuario
alter table SolicitudIngreso add constraint FK_F95BA7F0 foreign key (ComunidadId) references Comunidad
alter table SolicitudIngreso add constraint FK_13422D2A foreign key (EquipoId) references Equipo
alter table Torneo add constraint FK_F939C77 foreign key (ComunidadId) references Comunidad
alter table VotoTorneo add constraint FK_E3777048 foreign key (UsuarioId) references Usuario
alter table VotoTorneo add constraint FK_A853BA31 foreign key (PropuestaId) references PropuestaTorneo
create table NHibernateUniqueKey ( NextHigh BIGINT )
insert into NHibernateUniqueKey values ( 1 )
