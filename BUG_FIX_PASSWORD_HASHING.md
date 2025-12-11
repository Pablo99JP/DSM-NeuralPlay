# 🔐 FIX: Double Password Hashing en InitializeDb

## Problema Identificado

Al ejecutar `InitializeDb`, las contraseñas de los usuarios de prueba (alice, bob, charlie, etc.) **no funcionaban para iniciar sesión**, aunque al cambiar la contraseña desde la web, sí funcionaba.

### Causa Raíz

**Double Hashing (Hashing doble):** La contraseña se estaba hasheando **dos veces** consecutivas:

```csharp
// InitializeDbService.cs (línea 659)
var user = usuarioCEN.NewUsuario(nick, email, PasswordHasher.Hash(password)); 
                                                    // ↓ Ya hasheada aquí

// UsuarioCEN.cs (línea 27 - ANTES)
ContrasenaHash = PasswordHasher.Hash(password)  // ↓ Se hashea OTRA VEZ
```

### Resultado del Bug

1. **Primera vez**: `"password1"` → Hash válido (PBKDF2)
2. **Segunda vez**: `Hash("password1")` → `"100000.salt.hash"` → Se hashea como si fuera contraseña en claro
3. **En Login**: Al verificar, intenta hacer `Verify("password1", doubleHash)` → ❌ FALLA

Cuando cambias desde la web, solo se hashea UNA vez → ✅ FUNCIONA

## Solución Implementada

### Cambio 1: UsuarioCEN.cs (línea 21-27)

**Antes:**
```csharp
public Usuario NewUsuario(string nick, string correo, string password)
{
    var u = new Usuario
    {
        Nick = nick,
        CorreoElectronico = correo,
        ContrasenaHash = PasswordHasher.Hash(password),  // ❌ SIEMPRE hashea
```

**Después:**
```csharp
public Usuario NewUsuario(string nick, string correo, string password)
{
    // Si ya está hasheada (formato PBKDF2), usarla directamente
    // Si no, hashearla primero
    var passwordHash = LooksLikeHashed(password) ? password : PasswordHasher.Hash(password);
    
    var u = new Usuario
    {
        Nick = nick,
        CorreoElectronico = correo,
        ContrasenaHash = passwordHash,  // ✅ Inteligente, detecta si ya está hasheada
```

### Cambio 2: InitializeDbService.cs (línea 654-659)

**Antes:**
```csharp
var user = usuarioCEN.NewUsuario(nick, email, PasswordHasher.Hash(password)); // ❌ Pre-hashea
```

**Después:**
```csharp
var user = usuarioCEN.NewUsuario(nick, email, password);  // ✅ Pasa en claro
```

## Cómo Funciona Ahora

### Creación de Usuario en InitializeDb

```
password = "password1"
    ↓
NewUsuario(nick, email, "password1")
    ↓ LooksLikeHashed("password1") → false (no contiene 3 partes separadas por punto)
    ↓
PasswordHasher.Hash("password1") → "100000.salt.hash"  ← Una sola vez ✅
    ↓
BD almacena: ContrasenaHash = "100000.salt.hash"
```

### Login con Contraseña

```
Usuario ingresa: "password1"
    ↓
PasswordHasher.Verify("password1", "100000.salt.hash")
    ↓ Recalcula el hash con el salt almacenado
    ↓
Hashes coinciden → ✅ LOGIN EXITOSO
```

### Si se pasa un hash pre-calculado (edge case)

```
password = "100000.salt.hash" (formato PBKDF2)
    ↓
NewUsuario(nick, email, "100000.salt.hash")
    ↓ LooksLikeHashed("100000.salt.hash") → true (contiene 3 partes numéricas)
    ↓
Se usa directamente, sin hashear → "100000.salt.hash"  ← No se hashea ✅
    ↓
BD almacena: ContrasenaHash = "100000.salt.hash"
```

## Beneficios de Esta Solución

1. **Robustez**: Detecta automáticamente si la contraseña ya está hasheada
2. **Consistencia**: Igual lógica que `ModifyUsuario()` (línea 80)
3. **Compatibilidad**: Funciona con InitializeDb y con creación de usuarios desde la web
4. **Idempotencia**: No importa cuántas veces se llame, el resultado es correcto

## Verificación

### Usuarios de Prueba que Ahora Funcionan

- **alice** / **password1** ✅
- **bob** / **password2** ✅
- **charlie** / **password3** ✅
- **user1** / **password1** ✅
- **user2** / **password2** ✅
- **user3** / **password3** ✅

### Cómo Probar

```bash
cd c:\Users\SantinoCampessiLojo\Documents\UNI\DSM-NeuralPlay
dotnet build  # Compilar (ya hecho)

# Ejecutar InitializeDb
cd InitializeDb
dotnet run -- --force-drop --confirm --seed

# Luego intentar login en la web con:
# Nick: alice
# Contraseña: password1
```

## Archivos Modificados

1. `/ApplicationCore/Domain/CEN/UsuarioCEN.cs` (líneas 21-27)
2. `/InitializeDb/InitializeDbService.cs` (líneas 654-659)

## Cambios Relacionados

La función `LooksLikeHashed()` ya existía y se usaba en `ModifyUsuario()` (línea 80) para evitar hashear dos veces cuando se modifica un usuario. Ahora se usa el mismo patrón en `NewUsuario()` para mantener consistencia.
