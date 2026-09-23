/* ============================================================
   NeptunoDB_Lab06 - Correccion de acentos en los datos de prueba
   Laboratorio 06 - Class Library & DataSet
   ------------------------------------------------------------
   OPCIONAL. El archivo NeptunoDB.sql entregado trae los datos de
   ejemplo con las tildes reemplazadas por el caracter U+FFFD
   ("Lacteos" aparece como L?cteos, "Peru" como Per?), por lo que
   la corrupcion se ve en cualquier herramienta, no solo en esta
   aplicacion. Este script deja los textos correctos.

   Ejecutar despues de 00_NeptunoDB.sql. Se puede repetir sin problema.
   El archivo esta guardado en UTF-8 con BOM para que sqlcmd y SSMS
   respeten las tildes.
   ============================================================ */

USE NeptunoDB_Lab06;
GO

SET NOCOUNT ON;

/* ---------- Categorias ---------- */
UPDATE dbo.Categorias SET Descripcion = N'Refrescos, cafés, tés, cervezas y otras bebidas'
WHERE NombreCategoria = N'Bebidas';

UPDATE dbo.Categorias SET NombreCategoria = N'Lácteos', Descripcion = N'Quesos y otros productos lácteos'
WHERE NombreCategoria LIKE N'L%cteos';

/* ---------- Proveedores ---------- */
UPDATE dbo.Proveedores SET CompaniaNombre = N'Lácteos García S.A.', NombreContacto = N'Ana García',
       Direccion = N'Av. Los Álamos 245', Pais = N'Perú'
WHERE ProveedorID = 1;

UPDATE dbo.Proveedores SET NombreContacto = N'Carlos Ramírez', Pais = N'Perú'
WHERE ProveedorID = 2;

UPDATE dbo.Proveedores SET NombreContacto = N'María Torres', CargoContacto = N'Coordinadora de Distribución',
       Pais = N'Perú'
WHERE ProveedorID = 3;

UPDATE dbo.Proveedores SET Pais = N'Perú'
WHERE ProveedorID = 4;

UPDATE dbo.Proveedores SET NombreContacto = N'Lucía Fernández', Direccion = N'Jr. San Martín 77', Pais = N'Perú'
WHERE ProveedorID = 5;

/* ---------- Clientes ---------- */
UPDATE dbo.Clientes SET Pais = N'Perú' WHERE ClienteID IN (1, 2, 5);

UPDATE dbo.Clientes SET Empresa = N'Distribuidora Sureña EIRL', NombreContacto = N'Luis Chávez', Pais = N'Perú'
WHERE ClienteID = 3;

UPDATE dbo.Clientes SET Pais = N'Perú' WHERE ClienteID = 4;

UPDATE dbo.Clientes SET NombreContacto = N'Miguel Ángel Paredes' WHERE ClienteID = 5;

/* ---------- Empleados ---------- */
UPDATE dbo.Empleados SET Apellidos = N'Pérez Gómez', Pais = N'Perú' WHERE EmpleadoID = 1;
UPDATE dbo.Empleados SET Nombre = N'María', Apellidos = N'López Díaz', Pais = N'Perú' WHERE EmpleadoID = 2;
UPDATE dbo.Empleados SET Pais = N'Perú' WHERE EmpleadoID = 3;
UPDATE dbo.Empleados SET Nombre = N'Sofía', Pais = N'Perú' WHERE EmpleadoID = 4;
UPDATE dbo.Empleados SET Apellidos = N'Fernández Ríos', Pais = N'Perú' WHERE EmpleadoID = 5;

/* ---------- Transportistas ---------- */
UPDATE dbo.Transportistas SET CompaniaNombre = N'Transportes Rápido SAC' WHERE TransportistaID = 1;
UPDATE dbo.Transportistas SET CompaniaNombre = N'Envíos Seguros EIRL' WHERE TransportistaID = 2;
UPDATE dbo.Transportistas SET CompaniaNombre = N'Logística del Pacífico' WHERE TransportistaID = 3;

/* ---------- Productos ---------- */
UPDATE dbo.Productos SET NombreProducto = N'Café Andino Premium' WHERE ProductoID = 1;
UPDATE dbo.Productos SET NombreProducto = N'Salsa de Ají Amarillo' WHERE ProductoID = 2;

/* ---------- Pedidos ---------- */
UPDATE dbo.Pedidos SET PaisDestino = N'Perú';
UPDATE dbo.Pedidos SET Destinatario = N'Distribuidora Sureña EIRL' WHERE PedidoID = 3;
GO

/* Verificacion: no debe quedar ningun caracter de reemplazo.
   Se compara con collation binaria porque, con la collation por defecto,
   NCHAR(0xFFFD) es un caracter ignorable y LIKE terminaria aceptando todo. */
DECLARE @Reemplazo NVARCHAR(1) = NCHAR(0xFFFD);

SELECT 'Categorias' AS Tabla, COUNT(*) AS PendientesDeCorregir
FROM dbo.Categorias
WHERE NombreCategoria COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Descripcion COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Proveedores', COUNT(*)
FROM dbo.Proveedores
WHERE CompaniaNombre COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR NombreContacto COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR CargoContacto COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Direccion COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Pais COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Clientes', COUNT(*)
FROM dbo.Clientes
WHERE Empresa COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR NombreContacto COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Pais COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Empleados', COUNT(*)
FROM dbo.Empleados
WHERE Nombre COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Apellidos COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR Pais COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Transportistas', COUNT(*)
FROM dbo.Transportistas
WHERE CompaniaNombre COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Productos', COUNT(*)
FROM dbo.Productos
WHERE NombreProducto COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
UNION ALL
SELECT 'Pedidos', COUNT(*)
FROM dbo.Pedidos
WHERE Destinatario COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%'
   OR PaisDestino COLLATE Latin1_General_BIN2 LIKE N'%' + @Reemplazo + N'%';
GO
