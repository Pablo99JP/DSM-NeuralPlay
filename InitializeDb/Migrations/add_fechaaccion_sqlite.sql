-- Migration: Añadir columna FechaAccion a MiembroComunidad y MiembroEquipo (SQLite)
-- Ejecutar contra el archivo SQLite (ej: project.db) con sqlite3 o como parte de la migración.

PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

-- SQLite permite ADD COLUMN, la nueva columna será NULL por defecto.
ALTER TABLE MiembroComunidad ADD COLUMN FechaAccion TEXT NULL;
ALTER TABLE MiembroEquipo ADD COLUMN FechaAccion TEXT NULL;

COMMIT;
PRAGMA foreign_keys = ON;

-- Nota: el tipo TEXT se usa para almacenar DATETIME en SQLite; si necesitas otro formato, adapta las consultas.
