/* ============================================================
   NeptunoDB_Lab06 - Campo de estado para eliminacion logica
   Laboratorio 06 - Class Library & DataSet
   ------------------------------------------------------------
   Ejecutar despues de 00_NeptunoDB.sql.

   Agrega la columna Activo BIT NOT NULL DEFAULT 1 a Productos,
   Categorias, Proveedores y Pedidos. Con WITH VALUES las filas
   que ya existen quedan con Activo = 1.

   "Eliminar" un registro ya no lo borra: los procedimientos
   *_Eliminar ponen Activo = 0 y todos los listados filtran
   Activo = 1.

   Cada ALTER esta protegido con COL_LENGTH, asi que el script se
   puede volver a ejecutar sin error.
   ============================================================ */

USE NeptunoDB_Lab06;
GO

IF COL_LENGTH(N'dbo.Categorias', N'Activo') IS NULL
    ALTER TABLE dbo.Categorias
        ADD Activo BIT NOT NULL
            CONSTRAINT DF_Categorias_Activo DEFAULT (1) WITH VALUES;
GO

IF COL_LENGTH(N'dbo.Proveedores', N'Activo') IS NULL
    ALTER TABLE dbo.Proveedores
        ADD Activo BIT NOT NULL
            CONSTRAINT DF_Proveedores_Activo DEFAULT (1) WITH VALUES;
GO

IF COL_LENGTH(N'dbo.Productos', N'Activo') IS NULL
    ALTER TABLE dbo.Productos
        ADD Activo BIT NOT NULL
            CONSTRAINT DF_Productos_Activo DEFAULT (1) WITH VALUES;
GO

IF COL_LENGTH(N'dbo.Pedidos', N'Activo') IS NULL
    ALTER TABLE dbo.Pedidos
        ADD Activo BIT NOT NULL
            CONSTRAINT DF_Pedidos_Activo DEFAULT (1) WITH VALUES;
GO

/* Verificacion: las cuatro tablas deben mostrar la columna Activo. */
SELECT t.name AS Tabla, c.name AS Columna, ty.name AS Tipo,
       c.is_nullable AS AdmiteNulos, dc.definition AS ValorPorDefecto
FROM sys.columns c
INNER JOIN sys.tables t  ON t.object_id = c.object_id
INNER JOIN sys.types  ty ON ty.user_type_id = c.user_type_id
LEFT  JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
WHERE c.name = N'Activo'
  AND t.name IN (N'Categorias', N'Proveedores', N'Productos', N'Pedidos')
ORDER BY t.name;
GO
