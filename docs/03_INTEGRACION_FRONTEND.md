# Integración con Frontend

## 🎨 Cómo Integrar con un Frontend

Este proyecto está diseñado siguiendo Clean Architecture, lo que facilita la creación de una capa de API REST para consumir desde cualquier frontend (React, Angular, Vue, etc.).

### Arquitectura para API REST

#### Paso 1: Crear Proyecto Web API

```powershell
# Crear nuevo proyecto Web API en la solución
dotnet new webapi -n WebAPI
dotnet sln Solution.sln add WebAPI/WebAPI.csproj

# Agregar referencias a los proyectos existentes
cd WebAPI
dotnet add reference ../ApplicationCore/ApplicationCore.csproj
dotnet add reference ../Infrastructure/Infrastructure.csproj
```

#### Paso 2: Configurar Dependency Injection (Program.cs o Startup.cs)

```csharp
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;

var builder = WebApplication.CreateBuilder(args);

// Configurar NHibernate SessionFactory
builder.Services.AddSingleton<ISessionFactory>(provider =>
{
    return NHibernateHelper.SessionFactory; // Singleton global
});

// Registrar ISession como Scoped (una por request)
builder.Services.AddScoped<ISession>(provider =>
{
    var sessionFactory = provider.GetRequiredService<ISessionFactory>();
    return sessionFactory.OpenSession();
});

// Registrar UnitOfWork
builder.Services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();

// Registrar Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IComunidadRepository, ComunidadRepository>();
builder.Services.AddScoped<IEquipoRepository, EquipoRepository>();
builder.Services.AddScoped<IMiembroComunidadRepository, MiembroComunidadRepository>();
builder.Services.AddScoped<IMiembroEquipoRepository, MiembroEquipoRepository>();
builder.Services.AddScoped<IJuegoRepository, JuegoRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<ITorneoRepository, TorneoRepository>();
builder.Services.AddScoped<IInvitacionRepository, InvitacionRepository>();
builder.Services.AddScoped<ISolicitudIngresoRepository, SolicitudIngresoRepository>();
builder.Services.AddScoped<IPropuestaTorneoRepository, PropuestaTorneoRepository>();
builder.Services.AddScoped<IParticipacionTorneoRepository, ParticipacionTorneoRepository>();
builder.Services.AddScoped<IPublicacionRepository, PublicacionRepository>();

// Registrar CENs
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<ComunidadCEN>();
builder.Services.AddScoped<EquipoCEN>();
builder.Services.AddScoped<MiembroComunidadCEN>();
builder.Services.AddScoped<MiembroEquipoCEN>();
builder.Services.AddScoped<JuegoCEN>();
builder.Services.AddScoped<PerfilCEN>();
builder.Services.AddScoped<TorneoCEN>();
builder.Services.AddScoped<PublicacionCEN>();
builder.Services.AddScoped<SolicitudIngresoCEN>();

// Registrar CPs
builder.Services.AddScoped<RegistroUsuarioCP>();
builder.Services.AddScoped<CrearComunidadCP>();
builder.Services.AddScoped<AceptarInvitacionEquipoCP>();
builder.Services.AddScoped<AprobarPropuestaTorneoCP>();

// Configurar CORS para frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200") // React, Angular
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### Paso 3: Crear Controladores (Ejemplo: UsuarioController.cs)

```csharp
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly RegistroUsuarioCP _registroUsuarioCP;

        // Constructor: Inyección de dependencias
        // El framework ASP.NET Core resuelve automáticamente los CENs y CPs
        public UsuarioController(UsuarioCEN usuarioCEN, RegistroUsuarioCP registroUsuarioCP)
        {
            _usuarioCEN = usuarioCEN;
            _registroUsuarioCP = registroUsuarioCP;
        }

        // GET: api/Usuario
        [HttpGet]
        public IActionResult GetTodos()
        {
            var usuarios = _usuarioCEN.DameTodos();
            return Ok(usuarios);
        }

        // GET: api/Usuario/5
        [HttpGet("{id}")]
        public IActionResult GetPorId(long id)
        {
            var usuario = _usuarioCEN.DamePorOID(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });
            
            return Ok(usuario);
        }

        // POST: api/Usuario/registro
        [HttpPost("registro")]
        public IActionResult Registrar([FromBody] RegistroUsuarioDto dto)
        {
            try
            {
                // Usa el CP transaccional que crea Usuario + Perfil
                var idUsuario = _registroUsuarioCP.RegistrarUsuarioConPerfil(
                    dto.Nick,
                    dto.CorreoElectronico,
                    dto.ContrasenaHash,
                    dto.Telefono
                );

                return CreatedAtAction(
                    nameof(GetPorId),
                    new { id = idUsuario },
                    new { id = idUsuario, mensaje = "Usuario registrado exitosamente" }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/Usuario/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            try
            {
                // Usa el método custom del CEN
                var usuario = _usuarioCEN.Login(dto.CorreoElectronico, dto.ContrasenaHash);
                
                return Ok(new
                {
                    id = usuario.IdUsuario,
                    nick = usuario.Nick,
                    correo = usuario.CorreoElectronico,
                    mensaje = "Login exitoso"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        // GET: api/Usuario/filtro/{texto}
        [HttpGet("filtro/{texto}")]
        public IActionResult Filtrar(string texto)
        {
            var usuarios = _usuarioCEN.DamePorFiltro(texto);
            return Ok(usuarios);
        }

        // PUT: api/Usuario/5
        [HttpPut("{id}")]
        public IActionResult Modificar(long id, [FromBody] ModificarUsuarioDto dto)
        {
            try
            {
                _usuarioCEN.Modificar(
                    id,
                    dto.Nick,
                    dto.CorreoElectronico,
                    dto.ContrasenaHash,
                    dto.FechaRegistro,
                    dto.EstadoCuenta,
                    dto.Telefono
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/Usuario/5
        [HttpDelete("{id}")]
        public IActionResult Eliminar(long id)
        {
            try
            {
                _usuarioCEN.Eliminar(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // DTOs (Data Transfer Objects) - Evitan exponer entidades directamente
    public class RegistroUsuarioDto
    {
        public string Nick { get; set; }
        public string CorreoElectronico { get; set; }
        public string ContrasenaHash { get; set; }
        public string? Telefono { get; set; }
    }

    public class LoginDto
    {
        public string CorreoElectronico { get; set; }
        public string ContrasenaHash { get; set; }
    }

    public class ModificarUsuarioDto
    {
        public string Nick { get; set; }
        public string CorreoElectronico { get; set; }
        public string ContrasenaHash { get; set; }
        public DateTime FechaRegistro { get; set; }
        public EstadoCuenta EstadoCuenta { get; set; }
        public string? Telefono { get; set; }
    }
}
```

#### Paso 4: Consumir desde Frontend (Ejemplo React/TypeScript)

```typescript
// services/usuarioService.ts
const API_BASE_URL = 'http://localhost:5000/api';

export interface Usuario {
  idUsuario: number;
  nick: string;
  correoElectronico: string;
  telefono?: string;
  fechaRegistro: string;
  estadoCuenta: string;
}

export interface RegistroUsuario {
  nick: string;
  correoElectronico: string;
  contrasenaHash: string;
  telefono?: string;
}

export interface Login {
  correoElectronico: string;
  contrasenaHash: string;
}

// Obtener todos los usuarios
export async function obtenerUsuarios(): Promise<Usuario[]> {
  const response = await fetch(`${API_BASE_URL}/Usuario`);
  if (!response.ok) throw new Error('Error al obtener usuarios');
  return await response.json();
}

// Registrar nuevo usuario
export async function registrarUsuario(datos: RegistroUsuario): Promise<{ id: number }> {
  const response = await fetch(`${API_BASE_URL}/Usuario/registro`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(datos)
  });
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Error al registrar');
  }
  return await response.json();
}

// Login
export async function loginUsuario(datos: Login): Promise<Usuario> {
  const response = await fetch(`${API_BASE_URL}/Usuario/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(datos)
  });
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Credenciales inválidas');
  }
  return await response.json();
}

// Filtrar usuarios
export async function filtrarUsuarios(texto: string): Promise<Usuario[]> {
  const response = await fetch(`${API_BASE_URL}/Usuario/filtro/${encodeURIComponent(texto)}`);
  if (!response.ok) throw new Error('Error al filtrar');
  return await response.json();
}
```

```tsx
// components/LoginForm.tsx
import React, { useState } from 'react';
import { loginUsuario } from '../services/usuarioService';

export function LoginForm() {
  const [correo, setCorreo] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const usuario = await loginUsuario({
        correoElectronico: correo,
        contrasenaHash: password // En producción, hashear antes de enviar
      });
      console.log('Login exitoso:', usuario);
      // Guardar en contexto/Redux/localStorage
      localStorage.setItem('usuario', JSON.stringify(usuario));
      window.location.href = '/dashboard';
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="email"
        value={correo}
        onChange={(e) => setCorreo(e.target.value)}
        placeholder="Correo electrónico"
        required
      />
      <input
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        placeholder="Contraseña"
        required
      />
      {error && <p className="error">{error}</p>}
      <button type="submit">Iniciar Sesión</button>
    </form>
  );
}
```

### Ventajas de esta Arquitectura para Frontend

✅ **Separación de Responsabilidades**: El frontend solo consume APIs REST, no conoce la base de datos ni NHibernate

✅ **Transaccionalidad**: Los CPs garantizan que operaciones complejas sean atómicas (todo o nada)

✅ **Validaciones Centralizadas**: Todas las reglas de negocio están en CENs/CPs, no en el frontend

✅ **Reutilización**: Los mismos CENs/CPs pueden usarse desde Web API, gRPC, GraphQL, etc.

✅ **Testabilidad**: Fácil escribir tests unitarios para CENs y CPs sin base de datos real

✅ **Escalabilidad**: Puedes tener múltiples frontends (web, móvil) consumiendo la misma API
