# Funcionalidades Implementadas

## ✅ Operaciones CRUD Completas
- **21 CENs** con operaciones `Crear`, `Modificar`, `Eliminar`, `DamePorOID`, `DameTodos`
- Cobertura total del 100% de todas las entidades del dominio

### Lista Completa de CENs con CRUD:
1. UsuarioCEN
2. ComunidadCEN
3. EquipoCEN
4. MiembroComunidadCEN
5. MiembroEquipoCEN
6. JuegoCEN
7. PerfilCEN
8. TorneoCEN
9. PublicacionCEN
10. SolicitudIngresoCEN
11. InvitacionCEN ⭐
12. ChatEquipoCEN ⭐
13. MensajeChatCEN ⭐
14. ComentarioCEN ⭐
15. ReaccionCEN ⭐
16. NotificacionCEN ⭐
17. PropuestaTorneoCEN ⭐
18. VotoTorneoCEN ⭐
19. ParticipacionTorneoCEN ⭐
20. PerfilJuegoCEN ⭐
21. SesionCEN ⭐

**⭐ = CENs nuevos agregados con CRUD completo**

## ✅ Métodos Custom (8 implementados, mínimo 3 requeridos)
1. **`UsuarioCEN.Login(correo, password)`** - Autenticación de usuarios
   - Valida email + contraseña + cuenta ACTIVA
   - Retorna usuario o lanza UnauthorizedAccessException

2. **`MiembroComunidadCEN.PromoverAModerador(id)`** - Cambio de rol
   - Cambia rol de MIEMBRO → MODERADOR
   - Usado para promociones dentro de comunidades

3. **`MiembroComunidadCEN.ActualizarFechaAccion(id, fecha)`** - Gestión temporal
   - Actualiza UltimaAccion para tracking de actividad
   - Usado para estadísticas y gamificación

4. **`MiembroEquipoCEN.BanearMiembro(id)`** - Expulsión de miembros
   - Cambia Estado → EXPULSADA
   - Establece FechaBaja → DateTime.Now
   - Usado para moderación de equipos

5. **`SolicitudIngresoCEN.Aprobar(id)`** - Aprobación de solicitudes
   - Cambia Estado → ACEPTADA
   - Establece FechaRespuesta → DateTime.Now

6. **`SolicitudIngresoCEN.Rechazar(id)`** - Rechazo de solicitudes
   - Cambia Estado → RECHAZADA
   - Establece FechaRespuesta → DateTime.Now

7. **`NotificacionCEN.MarcarComoLeida(id)`** ⭐ - Marcar notificaciones
   - Establece Leida → true
   - Método de conveniencia para caso de uso común

8. **`SesionCEN.CerrarSesion(id)`** ⭐ - Logout de usuarios
   - Establece FechaFin → DateTime.Now
   - Método de conveniencia para finalizar sesión

## ✅ Filtros ReadFilter (12 implementados, mínimo 4 requeridos)

### Filtros Generales (8)
1. **`UsuarioCEN.DamePorFiltro(texto)`** - Búsqueda por Nick o Email (LIKE)
2. **`ComunidadCEN.DamePorFiltro(texto)`** - Búsqueda por Nombre o Descripción
3. **`EquipoCEN.DamePorFiltro(texto)`** - Búsqueda por Nombre o Descripción
4. **`TorneoCEN.DamePorFiltro(texto)`** - Búsqueda por Nombre o Estado
5. **`JuegoCEN.DamePorFiltro(texto)`** - Búsqueda por NombreJuego
6. **`PublicacionCEN.DamePorFiltro(texto)`** - Búsqueda por Contenido
7. **`NotificacionCEN.DamePorFiltro(texto)`** - Búsqueda por Mensaje
8. **`PerfilCEN.DamePorFiltro(texto)`** - Búsqueda por datos de perfil

### Filtros Específicos (4)
1. **`EquipoRepository.DamePorTorneo(idTorneo)`** 
   - Retorna equipos participando en un torneo específico
   - Usa LINQ: `e.Participaciones.Any(p => p.Torneo.IdTorneo == idTorneo)`

2. **`TorneoRepository.DamePorEquipo(idEquipo)`** 
   - Retorna torneos donde participa un equipo específico
   - Usa LINQ: `t.Participaciones.Any(p => p.Equipo.IdEquipo == idEquipo)`

3. **`UsuarioRepository.DamePorEquipo(idEquipo)`** 
   - Retorna usuarios miembros de un equipo específico
   - Usa LINQ: `u.MiembrosEquipo.Any(mem => mem.Equipo.IdEquipo == idEquipo)`

4. **`UsuarioRepository.DamePorComunidad(idComunidad)`** 
   - Retorna usuarios miembros de una comunidad específica
   - Usa LINQ: `u.MiembrosComunidad.Any(mem => mem.Comunidad.IdComunidad == idComunidad)`

## ✅ Custom Transactions - CPs (4 implementados, mínimo 2 requeridos)

### 1. **RegistroUsuarioCP**
**Operación**: Registro de usuario + creación de perfil (transaccional)

**Flujo**:
```
Usuario.Crear() → Perfil.Crear() → UnitOfWork.SaveChanges()
```

**Atomicidad**: Si falla cualquiera, ambos se revierten (ROLLBACK)

**Uso**:
```csharp
var idUsuario = registroUsuarioCP.RegistrarUsuarioConPerfil(
    "newuser", "newuser@test.com", "hashed_password", "123456789"
);
```

### 2. **CrearComunidadCP**
**Operación**: Creación de comunidad + líder (transaccional)

**Flujo**:
```
Comunidad.Crear() → MiembroComunidad.Crear(LIDER) → UnitOfWork.SaveChanges()
```

**Regla de Negocio**: Toda comunidad DEBE tener al menos un LÍDER

**Uso**:
```csharp
var idComunidad = crearComunidadCP.CrearComunidadConLider(
    "Elite Squad", "Solo los mejores", idUsuarioLider
);
```

### 3. **AceptarInvitacionEquipoCP**
**Operación**: Aceptación de invitación + membresía (transaccional)

**Flujo**:
```
1. Validar invitación (existe + tipo EQUIPO + estado PENDIENTE)
2. Actualizar invitación (Estado → ACEPTADA, FechaRespuesta → NOW)
3. Crear membresía (MiembroEquipo con Rol → MIEMBRO, Estado → ACTIVA)
4. UnitOfWork.SaveChanges()
```

**Atomicidad**: Si falla cualquiera, invitación NO cambia y membresía NO se crea

**Uso**:
```csharp
var idMiembro = aceptarInvitacionEquipoCP.Ejecutar(idInvitacion);
```

### 4. **AprobarPropuestaTorneoCP**
**Operación**: Aprobación de propuesta mediante votación unánime + participación (transaccional)

**Flujo**:
```
1. Validar propuesta (existe + estado PENDIENTE)
2. Verificar UNANIMIDAD (todos los votos son true)
3. Si NO hay unanimidad → Retorna false (NO se hace transacción)
4. Si SÍ hay unanimidad:
   - Aprobar propuesta (Estado → ACEPTADA)
   - Crear participación (ParticipacionTorneo)
   - UnitOfWork.SaveChanges()
   - Retorna true
```

**Regla de Negocio Crítica**: Se requiere UNANIMIDAD para aprobar

**Uso**:
```csharp
bool aprobada = aprobarPropuestaTorneoCP.Ejecutar(idPropuesta);
if (aprobada) {
    Console.WriteLine("Propuesta aprobada con unanimidad");
} else {
    Console.WriteLine("Falta unanimidad o no hay votos");
}
```

## ✅ InitializeDB Completo

El programa `InitializeDb` incluye:
- ✅ Creación automática del esquema de base de datos (SchemaExport)
- ✅ Seed de datos de prueba:
  - 4 Usuarios (player1, player2, player3, newplayer)
  - 3 Comunidades (Gamers Pro, Casual Players, Elite Squad)
  - 1 Equipo (Team Alpha)
  - 2 Juegos (League of Legends, FIFA 24)
  - 1 Torneo (Copa de Verano 2025)
  - Membresías, publicaciones, etc.
- ✅ Validación de métodos CRUD (Crear, Modificar, Eliminar, DamePorOID, DameTodos)
- ✅ Validación de métodos Custom (Login, PromoverAModerador, BanearMiembro, etc.)
- ✅ Validación de CustomTransactions (todos los CPs)
- ✅ Validación de Filtros (DamePorFiltro, DamePorTorneo, etc.)
- ✅ Resumen completo de la inicialización (conteo de entidades)

## Reglas de Negocio Implementadas

### Usuario
- ✅ Siempre se crea con `EstadoCuenta.ACTIVA` (no puede ser SUSPENDIDA o BANEADA al crear)
- ✅ FechaRegistro siempre es `DateTime.Now` (no se puede especificar)
- ✅ ContrasenaHash debe venir hasheada (el sistema NO hashea, es responsabilidad del llamador)

### MiembroComunidad
- ✅ Siempre se crea sin `FechaBaja` (null)
- ✅ FechaAlta siempre es `DateTime.Now`
- ✅ PromoverAModerador cambia Rol de MIEMBRO → MODERADOR (no LIDER)

### MiembroEquipo
- ✅ Siempre se crea sin `FechaBaja` (null)
- ✅ FechaAlta siempre es `DateTime.Now`
- ✅ BanearMiembro establece Estado → EXPULSADA y FechaBaja → DateTime.Now

### SolicitudIngreso
- ✅ Valida que usuario NO esté ya en la comunidad
- ✅ Valida que usuario NO esté en ningún equipo de la comunidad
- ✅ Solo se puede crear si cumple ambas condiciones

### PropuestaTorneo
- ✅ Requiere UNANIMIDAD (todos los votos = true) para aprobarse
- ✅ Si no hay votos, NO se puede aprobar
- ✅ Una vez aprobada, crea automáticamente ParticipacionTorneo

## Notas Técnicas

- ✅ Las entidades NO tienen referencias a Entity Framework o NHibernate
- ✅ Los CENs solo exponen operaciones sobre UNA entidad
- ✅ Los CPs orquestan MÚLTIPLES CENs y aplican lógica transaccional
- ✅ Generador HiLo para IDs eficiente y sin round-trips a BD
- ✅ Todas las operaciones son síncronas según especificación
- ✅ Validaciones de negocio implementadas en CENs y CPs
- ✅ LINQ to NHibernate para consultas tipadas traducidas a SQL
- ✅ UnitOfWork garantiza transaccionalidad (todo o nada)
- ✅ Lazy Loading de relaciones mediante propiedades `virtual`

## Documentos Adicionales

- **IMPLEMENTACIONES.md** - Detalle técnico de todas las implementaciones
- **VERIFICACION_REQUISITOS.md** - Verificación completa de requisitos cumplidos
