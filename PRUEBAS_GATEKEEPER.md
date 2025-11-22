# PRUEBAS_GATEKEEPER.md

## Caso A: Registro, Hashing (PBKDF2) y Persistencia

**Pasos:**
1. Navegar a `/Usuario/Create` y crear una cuenta nueva.
2. Confirmar que el `ModelState` es válido y que la cuenta aparece listada en `/Usuario`.

**Verificación:**
- La cuenta creada debe aparecer en el listado.
- El password debe estar hasheado (PBKDF2) en la base de datos/memoria.

---

## Caso B: Login, Seguridad y Edición de Contraseña

**Pasos:**
1. Iniciar sesión con la cuenta creada.
2. Verificar que el menú muestra el enlace **"Logout"**.
3. Navegar a `/Usuario/Edit` y cambiar la contraseña (Ej: de `123456` a `NuevaPass789`).
4. Cerrar sesión.
5. Intentar iniciar sesión con la **contraseña antigua**.
6. Intentar iniciar sesión con la **contraseña nueva**.

**Verificación:**
- El paso (5) debe **FALLAR** (prueba que la edición hasheó correctamente).
- El paso (6) debe **FUNCIONAR**.

---

## Caso C: Notificaciones (CRUD y Filtrado)

**Precondición:** Usuario logueado.

**Pasos:**
1. Navegar a `/Notificacion/Create` y crear un mensaje simple.
2. Navegar a `/Notificacion`.
3. Desde el `Index`, verificar que existe el enlace **"Delete"** para esa notificación.

**Verificación:**
- El mensaje creado debe aparecer **SÍ o SÍ** en el listado de `/Notificacion`.
- El enlace "Delete" debe estar presente, validando la relación con el usuario logueado.
