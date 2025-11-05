-- Migration: Añadir columna FechaAccion a MiembroComunidad y MiembroEquipo (SQL Server)
-- Ejecutar en el contexto de la base de datos correspondiente (LocalDB / SQL Server)

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'FechaAccion' AND Object_ID = Object_ID(N'dbo.MiembroComunidad'))
BEGIN
    ALTER TABLE dbo.MiembroComunidad ADD FechaAccion DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'FechaAccion' AND Object_ID = Object_ID(N'dbo.MiembroEquipo'))
BEGIN
    ALTER TABLE dbo.MiembroEquipo ADD FechaAccion DATETIME NULL;
END

-- Nota: si usas un esquema distinto a 'dbo', ajusta los nombres de tabla/schema.
