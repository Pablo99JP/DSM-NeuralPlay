# Flujo de Lógica de Negocio

## 🔄 Flujo de Lógica de Negocio y Aplicación

### Arquitectura en Capas (Clean Architecture + DDD)

```
┌─────────────────────────────────────────────────────────┐
│                      FRONTEND                           │
│            (React, Angular, Vue, etc.)                  │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP REST / GraphQL
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    WEB API LAYER                        │
│              (Controllers + DTOs)                       │
│  • Recibe peticiones HTTP                              │
│  • Valida entrada                                       │
│  • Llama a CPs o CENs                                   │
│  • Retorna respuestas JSON                              │
└────────────────────┬────────────────────────────────────┘
                     │ Dependency Injection
                     ▼
┌─────────────────────────────────────────────────────────┐
│              APPLICATION CORE (Dominio)                 │
│  ┌───────────────────────────────────────────────────┐ │
│  │  CP (Casos de Proceso)                            │ │
│  │  • RegistroUsuarioCP                              │ │
│  │  • CrearComunidadCP                               │ │
│  │  • AceptarInvitacionEquipoCP                      │ │
│  │  • AprobarPropuestaTorneoCP                       │ │
│  │  ───────────────────────────────────────────────  │ │
│  │  Orquestan múltiples CENs + UnitOfWork           │ │
│  │  Garantizan transaccionalidad                     │ │
│  └───────────────────┬───────────────────────────────┘ │
│                      │ Llama a                          │
│                      ▼                                   │
│  ┌───────────────────────────────────────────────────┐ │
│  │  CEN (Componentes Entidad Negocio)               │ │
│  │  • UsuarioCEN, ComunidadCEN, EquipoCEN, etc.     │ │
│  │  ───────────────────────────────────────────────  │ │
│  │  CRUD + Métodos Custom por entidad               │ │
│  │  Validaciones de negocio                          │ │
│  │  Reglas de dominio                                │ │
│  └───────────────────┬───────────────────────────────┘ │
│                      │ Usa                              │
│                      ▼                                   │
│  ┌───────────────────────────────────────────────────┐ │
│  │  Interfaces de Repositorio (IRepository)         │ │
│  │  • IUsuarioRepository                             │ │
│  │  • IComunidadRepository                           │ │
│  │  • IEquipoRepository, etc.                        │ │
│  │  ───────────────────────────────────────────────  │ │
│  │  Abstracción de persistencia                      │ │
│  │  SIN implementación concreta                      │ │
│  └───────────────────┬───────────────────────────────┘ │
│                      │                                   │
│  ┌───────────────────┴───────────────────────────────┐ │
│  │  Entidades de Dominio (EN)                        │ │
│  │  • Usuario, Comunidad, Equipo, etc.              │ │
│  │  ───────────────────────────────────────────────  │ │
│  │  POCOs sin dependencias de infraestructura       │ │
│  │  Propiedades virtuales para lazy loading         │ │
│  └───────────────────────────────────────────────────┘ │
└────────────────────┬────────────────────────────────────┘
                     │ Implementa
                     ▼
┌─────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE LAYER                       │
│  ┌───────────────────────────────────────────────────┐ │
│  │  Repositorios Concretos (NHibernate)             │ │
│  │  • UsuarioRepository                              │ │
│  │  • ComunidadRepository                            │ │
│  │  • EquipoRepository, etc.                         │ │
│  │  ───────────────────────────────────────────────  │ │
│  │  Implementación con ISession                      │ │
│  │  LINQ to NHibernate                               │ │
│  └───────────────────┬───────────────────────────────┘ │
│                      │                                   │
│  ┌───────────────────┴───────────────────────────────┐ │
│  │  NHibernate Configuration                         │ │
│  │  • Mappings XML (.hbm.xml)                        │ │
│  │  • NHibernateHelper                               │ │
│  │  • SessionFactory (Singleton)                     │ │
│  └───────────────────┬───────────────────────────────┘ │
│                      │                                   │
│  ┌───────────────────┴───────────────────────────────┐ │
│  │  UnitOfWork (NHibernateUnitOfWork)               │ │
│  │  • ITransaction                                    │ │
│  │  • SaveChanges() → Commit                         │ │
│  │  • Rollback en caso de error                      │ │
│  └───────────────────────────────────────────────────┘ │
└────────────────────┬────────────────────────────────────┘
                     │ SQL
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  DATABASE LAYER                         │
│              SQL Server / LocalDB                       │
│  • Tablas generadas por SchemaExport                   │
│  • IDs con generador HiLo                              │
│  • Relaciones FK configuradas en mappings              │
└─────────────────────────────────────────────────────────┘
```

## Flujo de una Operación Completa

### Ejemplo: Registro de Usuario

#### 1️⃣ **Petición desde Frontend**
```typescript
// Usuario hace clic en "Registrarse"
const response = await fetch('http://localhost:5000/api/Usuario/registro', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    nick: 'newuser',
    correoElectronico: 'newuser@test.com',
    contrasenaHash: 'hashed_password',
    telefono: '123456789'
  })
});
```

#### 2️⃣ **Controlador Web API recibe la petición**
```csharp
// WebAPI/Controllers/UsuarioController.cs (línea ~50)
[HttpPost("registro")]
public IActionResult Registrar([FromBody] RegistroUsuarioDto dto)
{
    // Valida el modelo
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    try
    {
        // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CP/RegistroUsuarioCP.cs
        var idUsuario = _registroUsuarioCP.RegistrarUsuarioConPerfil(
            dto.Nick,
            dto.CorreoElectronico,
            dto.ContrasenaHash,
            dto.Telefono
        );
        
        return CreatedAtAction(nameof(GetPorId), new { id = idUsuario }, 
            new { id = idUsuario, mensaje = "Usuario registrado exitosamente" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

#### 3️⃣ **CP (Caso de Proceso) orquesta la operación**
```csharp
// ApplicationCore/Domain/CP/RegistroUsuarioCP.cs (línea ~35)
public long RegistrarUsuarioConPerfil(string nick, string correoElectronico, 
    string contrasenaHash, string telefono = null)
{
    // PASO 1: Crear Usuario
    // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~35)
    var idUsuario = _usuarioCEN.Crear(
        nick: nick,
        correoElectronico: correoElectronico,
        contrasenaHash: contrasenaHash,
        telefono: telefono
    );

    // PASO 2: Crear Perfil asociado
    // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/PerfilCEN.cs
    _perfilCEN.Crear(
        visibilidadPerfil: Visibilidad.PUBLICO,
        visibilidadActividad: Visibilidad.PUBLICO
    );

    // PASO 3: Guardar TODO en una transacción
    // FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
    _unitOfWork.SaveChanges(); // ← Si falla, se hace ROLLBACK automático

    return idUsuario;
}
```

#### 4️⃣ **CEN (Componente Entidad Negocio) ejecuta lógica de dominio**
```csharp
// ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~35)
public long Crear(string nick, string correoElectronico, string contrasenaHash, 
    string telefono = null)
{
    // Construye la entidad de dominio con reglas de negocio
    var usuario = new Usuario
    {
        Nick = nick,
        CorreoElectronico = correoElectronico,
        ContrasenaHash = contrasenaHash,
        FechaRegistro = DateTime.Now,           // ← Regla: Siempre fecha actual
        EstadoCuenta = EstadoCuenta.ACTIVA,     // ← Regla: Siempre ACTIVA al crear
        Telefono = telefono
    };

    // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs
    // Que extiende de GenericRepository<Usuario, long>
    _usuarioRepository.New(usuario);
    
    return usuario.IdUsuario;
}
```

#### 5️⃣ **Repositorio persiste en base de datos**
```csharp
// Infrastructure/NHibernate/Repositories/GenericRepository.cs (línea ~28)
public virtual void New(T entity)
{
    // ISession es la abstracción de NHibernate para la conexión BD
    // FLUJO SE DESPLAZA A: NHibernate (biblioteca externa)
    // Save() marca la entidad para inserción (no ejecuta SQL todavía)
    _session.Save(entity);
}
```

#### 6️⃣ **UnitOfWork confirma la transacción**
```csharp
// Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
public void SaveChanges()
{
    if (_transaction != null && _transaction.IsActive)
    {
        // AQUÍ se ejecutan TODOS los SQL INSERT/UPDATE/DELETE pendientes
        // FLUJO SE DESPLAZA A: NHibernate (biblioteca externa)
        _transaction.Commit(); // ← Confirma cambios en BD
        
        // Inicia nueva transacción para siguientes operaciones
        _transaction = _session.BeginTransaction();
    }
}
```

#### 7️⃣ **NHibernate ejecuta SQL**
```sql
-- NHibernate genera y ejecuta SQL automáticamente:

-- 1. Obtener siguiente ID del generador HiLo
SELECT NextHigh FROM NHibernateUniqueKey WHERE TableKey = 'Usuario';
UPDATE NHibernateUniqueKey SET NextHigh = NextHigh + 1 WHERE TableKey = 'Usuario';

-- 2. Insertar Usuario (ID calculado localmente con HiLo: eficiente)
INSERT INTO Usuario (IdUsuario, Nick, CorreoElectronico, ContrasenaHash, 
    Telefono, FechaRegistro, EstadoCuenta)
VALUES (1, 'newuser', 'newuser@test.com', 'hashed_password', 
    '123456789', '2025-11-06 10:30:00', 0);

-- 3. Insertar Perfil asociado
INSERT INTO Perfil (IdPerfil, VisibilidadPerfil, VisibilidadActividad, ...)
VALUES (1, 0, 0, ...);

-- 4. COMMIT de la transacción
COMMIT;
```

#### 8️⃣ **Respuesta al Frontend**
```json
{
  "id": 1,
  "mensaje": "Usuario registrado exitosamente"
}
```

## Flujo de Métodos Custom

### Ejemplo: Login de Usuario

#### 1️⃣ Frontend envía credenciales
```typescript
const response = await fetch('http://localhost:5000/api/Usuario/login', {
  method: 'POST',
  body: JSON.stringify({
    correoElectronico: 'player1@test.com',
    contrasenaHash: 'hash123'
  })
});
```

#### 2️⃣ Controlador llama al método custom del CEN
```csharp
// WebAPI/Controllers/UsuarioController.cs
[HttpPost("login")]
public IActionResult Login([FromBody] LoginDto dto)
{
    try
    {
        // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~95)
        var usuario = _usuarioCEN.Login(dto.CorreoElectronico, dto.ContrasenaHash);
        
        return Ok(new { id = usuario.IdUsuario, nick = usuario.Nick });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Unauthorized(new { error = ex.Message });
    }
}
```

#### 3️⃣ CEN ejecuta lógica de autenticación
```csharp
// ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~95)
public Usuario Login(string correoElectronico, string contrasenaHash)
{
    // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs
    var usuarios = _usuarioRepository.DameTodos();
    
    // LINQ to Objects: filtra en memoria (o usa DamePorFiltro para filtrar en BD)
    var usuario = usuarios.FirstOrDefault(u => 
        u.CorreoElectronico == correoElectronico && 
        u.ContrasenaHash == contrasenaHash &&
        u.EstadoCuenta == EstadoCuenta.ACTIVA);

    if (usuario == null)
    {
        throw new UnauthorizedAccessException("Credenciales inválidas o cuenta inactiva.");
    }

    return usuario;
}
```

#### 4️⃣ Repositorio consulta base de datos
```csharp
// Infrastructure/NHibernate/Repositories/GenericRepository.cs (línea ~40)
public virtual IList<T> DameTodos()
{
    // LINQ to NHibernate: genera SELECT * FROM Usuario
    return _session.Query<T>().ToList();
}
```

#### 5️⃣ SQL ejecutado
```sql
SELECT IdUsuario, Nick, CorreoElectronico, ContrasenaHash, 
       Telefono, FechaRegistro, EstadoCuenta
FROM Usuario;
```

#### 6️⃣ Respuesta al Frontend
```json
{
  "id": 1,
  "nick": "player1",
  "correo": "player1@test.com",
  "mensaje": "Login exitoso"
}
```

## Puntos Clave del Flujo

✅ **Separación de Responsabilidades**: Cada capa tiene su función específica

✅ **Dependency Inversion**: ApplicationCore NO conoce Infrastructure (solo interfaces)

✅ **Transaccionalidad**: UnitOfWork garantiza atomicidad (todo o nada)

✅ **Lazy Loading**: NHibernate carga relaciones bajo demanda (propiedades `virtual`)

✅ **Generador HiLo**: IDs eficientes sin round-trips a BD por cada INSERT

✅ **LINQ to NHibernate**: Consultas tipadas que se traducen a SQL

✅ **Validaciones Centralizadas**: Reglas de negocio en CENs, no dispersas
