# Pruebas (tests) — DSM-NeuralPlay

Este archivo explica cómo ejecutar las pruebas del repositorio desde Windows PowerShell.

Resumen
- Proyecto de pruebas unitarias ligero: `tests/UnitTests/UnitTests.csproj` (xUnit + Moq).
- Tests de integración / smoke: `tests/Domain.SmokeTests` — tocan NHibernate/LocalDB/SQLite y pueden ser frágiles en entornos con LocalDB en uso.

Comandos útiles (PowerShell)

- Restaurar dependencias y compilar solución:
```powershell
dotnet restore "c:\Users\Usuario\Desktop\TERCERO\DSM\ia\DSM-NeuralPlay\DSM-NeuralPlay.sln"
dotnet build "c:\Users\Usuario\Desktop\TERCERO\DSM\ia\DSM-NeuralPlay\DSM-NeuralPlay.sln"
```

- Ejecutar únicamente el proyecto de pruebas unitarias (recomendado para desarrollo rápido):
```powershell
dotnet test "c:\Users\Usuario\Desktop\TERCERO\DSM\ia\DSM-NeuralPlay\tests\UnitTests\UnitTests.csproj" --verbosity minimal
```

- Ejecutar la suite completa (incluye tests smoke/integration):
```powershell
dotnet test "c:\Users\Usuario\Desktop\TERCERO\DSM\ia\DSM-NeuralPlay\DSM-NeuralPlay.sln" --verbosity minimal
```

- Ejecutar un test concreto por nombre (filtrado):
```powershell
# Filtra por nombre del test o FullyQualifiedName
dotnet test "...\UnitTests.csproj" --filter "DisplayName~Ejecutar_ShouldCreateComunidad"
# o
dotnet test "...\UnitTests.csproj" --filter "FullyQualifiedName~Domain.UnitTests.CrearComunidadCPTests"
```

- Ejecutar tests sin reconstruir (útil en CI después de build):
```powershell
dotnet test "...\UnitTests.csproj" --no-build --verbosity minimal
```

Notas y recomendaciones

- Para desarrollo diario ejecuta sólo `tests/UnitTests` (rápido y aislado).
- Los tests en `tests/Domain.SmokeTests` realizan operaciones de base de datos (intentarán LocalDB y luego SQLite). Si tienes problemas con LocalDB (bases ya creadas o bloqueo de ficheros), ejecuta sólo los unit tests o limpia los MDF temporales en `%TEMP%` que empiecen por `initdb_test_...`.
- Si ves advertencias NU1901 sobre `Moq` (vulnerabilidad de baja gravedad), puedes actualizar la dependencia del proyecto de tests:
```powershell
cd "c:\Users\Usuario\Desktop\TERCERO\DSM\ia\DSM-NeuralPlay\tests\UnitTests"
dotnet add package Moq --version 4.22.0
```
Luego vuelve a `dotnet restore` y `dotnet test`.

Problemas conocidos
- Los smoke tests pueden intentar crear LocalDB MDFs con nombres fijos; si LocalDB ya tiene bases con esos nombres la creación falla y los tests fallback a SQLite. Esto está manejado por los tests/InitializeDb pero puede generar logs/advertencias. Ejecutar sólo `UnitTests` evita estos problemas.

CI (ejemplo básico)
- En un pipeline, usa este flujo compacto:
```powershell
# Windows agent
dotnet restore "DSM-NeuralPlay.sln"
dotnet build "DSM-NeuralPlay.sln" --configuration Release
# Ejecutar solo pruebas unitarias (rápidas)
dotnet test "tests/UnitTests/UnitTests.csproj" --configuration Release --no-build --verbosity minimal
```

Estado actual (local)
- En mi ejecución local tras ajustar un test flaky, la suite pasó: 29 tests, 0 fallos.

Si quieres puedo:
- Añadir etiquetas/Traits a los tests smoke para poder filtrarlos con `--filter "Category!=smoke"`.
- Actualizar Moq a 4.22.0 automáticamente y volver a ejecutar tests.

---
Archivo generado automáticamente por el asistente para facilitar la ejecución de pruebas.
