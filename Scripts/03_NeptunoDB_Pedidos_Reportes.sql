/* ============================================================
   NeptunoDB_Lab06 - Pedidos, detalle de pedidos y reportes
   Laboratorio 06 - Class Library & DataSet
   ------------------------------------------------------------
   Ejecutar despues de 02_NeptunoDB_Procedimientos.sql
   Mismas reglas que el script 02: listados con Activo = 1,
   altas con parametro OUTPUT y bajas logicas (Activo = 0).
   ============================================================ */

USE NeptunoDB_Lab06;
GO

/* ============================================================
   CATALOGOS DE APOYO (combos de la interfaz)
   Clientes, Empleados y Transportistas no forman parte de la
   eliminacion logica del laboratorio, por eso no filtran Activo.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ClienteID, Empresa, NombreContacto, Ciudad, Pais, Telefono
    FROM dbo.Clientes
    ORDER BY Empresa;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Empleado_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmpleadoID, Nombre, Apellidos, Cargo, Ciudad, Pais,
           Nombre + ' ' + Apellidos AS NombreCompleto
    FROM dbo.Empleados
    ORDER BY Apellidos, Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transportista_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TransportistaID, CompaniaNombre, Telefono
    FROM dbo.Transportistas
    ORDER BY CompaniaNombre;
END
GO

/* ============================================================
   PEDIDOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ped.PedidoID, ped.ClienteID, ped.EmpleadoID, ped.FechaPedido,
           ped.FechaRequerida, ped.FechaEnvio, ped.TransportistaID,
           ped.Destinatario, ped.CiudadDestino, ped.PaisDestino, ped.Activo,
           cli.Empresa AS NombreCliente,
           emp.Nombre + ' ' + emp.Apellidos AS NombreEmpleado,
           tra.CompaniaNombre AS NombreTransportista,
           ISNULL((SELECT SUM(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento))
                   FROM dbo.DetallePedidos det
                   WHERE det.PedidoID = ped.PedidoID), 0) AS Total
    FROM dbo.Pedidos ped
    LEFT JOIN dbo.Clientes        cli ON cli.ClienteID       = ped.ClienteID
    LEFT JOIN dbo.Empleados       emp ON emp.EmpleadoID      = ped.EmpleadoID
    LEFT JOIN dbo.Transportistas  tra ON tra.TransportistaID = ped.TransportistaID
    WHERE ped.Activo = 1
    ORDER BY ped.FechaPedido DESC, ped.PedidoID DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedido_ObtenerPorId
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ped.PedidoID, ped.ClienteID, ped.EmpleadoID, ped.FechaPedido,
           ped.FechaRequerida, ped.FechaEnvio, ped.TransportistaID,
           ped.Destinatario, ped.CiudadDestino, ped.PaisDestino, ped.Activo,
           cli.Empresa AS NombreCliente,
           emp.Nombre + ' ' + emp.Apellidos AS NombreEmpleado,
           tra.CompaniaNombre AS NombreTransportista,
           ISNULL((SELECT SUM(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento))
                   FROM dbo.DetallePedidos det
                   WHERE det.PedidoID = ped.PedidoID), 0) AS Total
    FROM dbo.Pedidos ped
    LEFT JOIN dbo.Clientes        cli ON cli.ClienteID       = ped.ClienteID
    LEFT JOIN dbo.Empleados       emp ON emp.EmpleadoID      = ped.EmpleadoID
    LEFT JOIN dbo.Transportistas  tra ON tra.TransportistaID = ped.TransportistaID
    WHERE ped.PedidoID = @PedidoID
      AND ped.Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Crear
    @ClienteID       INT = NULL,
    @EmpleadoID      INT = NULL,
    @FechaPedido     DATE,
    @FechaRequerida  DATE = NULL,
    @FechaEnvio      DATE = NULL,
    @TransportistaID INT = NULL,
    @Destinatario    NVARCHAR(60) = NULL,
    @CiudadDestino   NVARCHAR(30) = NULL,
    @PaisDestino     NVARCHAR(30) = NULL,
    @PedidoID        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @FechaRequerida IS NOT NULL AND @FechaRequerida < @FechaPedido
            THROW 50401, 'La fecha requerida no puede ser anterior a la fecha del pedido.', 1;

        INSERT INTO dbo.Pedidos
            (ClienteID, EmpleadoID, FechaPedido, FechaRequerida, FechaEnvio,
             TransportistaID, Destinatario, CiudadDestino, PaisDestino)
        VALUES
            (@ClienteID, @EmpleadoID, @FechaPedido, @FechaRequerida, @FechaEnvio,
             @TransportistaID, @Destinatario, @CiudadDestino, @PaisDestino);

        SET @PedidoID = CAST(SCOPE_IDENTITY() AS INT);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Actualizar
    @PedidoID        INT,
    @ClienteID       INT = NULL,
    @EmpleadoID      INT = NULL,
    @FechaPedido     DATE,
    @FechaRequerida  DATE = NULL,
    @FechaEnvio      DATE = NULL,
    @TransportistaID INT = NULL,
    @Destinatario    NVARCHAR(60) = NULL,
    @CiudadDestino   NVARCHAR(30) = NULL,
    @PaisDestino     NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @FechaRequerida IS NOT NULL AND @FechaRequerida < @FechaPedido
            THROW 50402, 'La fecha requerida no puede ser anterior a la fecha del pedido.', 1;

        UPDATE dbo.Pedidos
        SET ClienteID       = @ClienteID,
            EmpleadoID      = @EmpleadoID,
            FechaPedido     = @FechaPedido,
            FechaRequerida  = @FechaRequerida,
            FechaEnvio      = @FechaEnvio,
            TransportistaID = @TransportistaID,
            Destinatario    = @Destinatario,
            CiudadDestino   = @CiudadDestino,
            PaisDestino     = @PaisDestino
        WHERE PedidoID = @PedidoID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50403, 'El pedido no existe o fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Baja logica del pedido. En el laboratorio 04 se borraban las
   lineas y la cabecera en una transaccion; ahora solo se marca la
   cabecera con Activo = 0 y el detalle se conserva intacto, por
   lo que el pedido puede auditarse o reactivarse despues. */
CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Eliminar
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Pedidos
        SET Activo = 0
        WHERE PedidoID = @PedidoID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50404, 'El pedido no existe o ya fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ============================================================
   DETALLE DE PEDIDOS
   La clave primaria es compuesta (PedidoID, ProductoID), por eso
   no hay SCOPE_IDENTITY y las operaciones piden las dos claves.
   DetallePedidos no lleva columna Activo: sus lineas pertenecen
   al pedido, y la baja logica se aplica sobre la cabecera. Solo
   se permite modificar lineas de pedidos activos.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedido_ListarPorPedido
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    /* Sin filtro sobre pro.Activo: una linea historica sigue
       mostrando su producto aunque este se haya dado de baja. */
    SELECT det.PedidoID, det.ProductoID, det.PrecioUnidad, det.Cantidad, det.Descuento,
           pro.NombreProducto,
           det.PrecioUnidad * det.Cantidad * (1 - det.Descuento) AS Subtotal
    FROM dbo.DetallePedidos det
    INNER JOIN dbo.Pedidos   ped ON ped.PedidoID   = det.PedidoID
    INNER JOIN dbo.Productos pro ON pro.ProductoID = det.ProductoID
    WHERE det.PedidoID = @PedidoID
      AND ped.Activo = 1
    ORDER BY pro.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedido_Agregar
    @PedidoID     INT,
    @ProductoID   INT,
    @PrecioUnidad DECIMAL(10,2),
    @Cantidad     SMALLINT,
    @Descuento    DECIMAL(4,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Cantidad <= 0
            THROW 50501, 'La cantidad debe ser mayor que cero.', 1;

        IF @Descuento < 0 OR @Descuento > 1
            THROW 50502, 'El descuento debe estar entre 0 y 1.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID AND Activo = 1)
            THROW 50508, 'El pedido no existe o fue eliminado.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoID = @ProductoID AND Activo = 1)
            THROW 50509, 'El producto no existe o fue eliminado.', 1;

        IF EXISTS (SELECT 1 FROM dbo.DetallePedidos
                   WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID)
            THROW 50503, 'El producto ya esta registrado en este pedido.', 1;

        INSERT INTO dbo.DetallePedidos (PedidoID, ProductoID, PrecioUnidad, Cantidad, Descuento)
        VALUES (@PedidoID, @ProductoID, @PrecioUnidad, @Cantidad, @Descuento);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedido_Actualizar
    @PedidoID     INT,
    @ProductoID   INT,
    @PrecioUnidad DECIMAL(10,2),
    @Cantidad     SMALLINT,
    @Descuento    DECIMAL(4,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Cantidad <= 0
            THROW 50504, 'La cantidad debe ser mayor que cero.', 1;

        IF @Descuento < 0 OR @Descuento > 1
            THROW 50505, 'El descuento debe estar entre 0 y 1.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID AND Activo = 1)
            THROW 50508, 'El pedido no existe o fue eliminado.', 1;

        UPDATE dbo.DetallePedidos
        SET PrecioUnidad = @PrecioUnidad,
            Cantidad     = @Cantidad,
            Descuento    = @Descuento
        WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID;

        IF @@ROWCOUNT = 0
            THROW 50506, 'El detalle no existe.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Quita una linea de un pedido activo. Es la unica operacion con
   DELETE: DetallePedidos no tiene campo Activo (el enunciado lo pide
   solo para Productos, Categorias, Proveedores y Pedidos) y quitar
   una linea es editar el contenido del pedido, no darlo de baja. */
CREATE OR ALTER PROCEDURE dbo.usp_DetallePedido_Eliminar
    @PedidoID   INT,
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID AND Activo = 1)
            THROW 50508, 'El pedido no existe o fue eliminado.', 1;

        DELETE FROM dbo.DetallePedidos
        WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID;

        IF @@ROWCOUNT = 0
            THROW 50507, 'El detalle no existe.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ============================================================
   REPORTE
   Listado de detalles de pedidos con INNER JOIN a pedidos,
   filtrando por un intervalo de fechas y excluyendo los pedidos
   dados de baja (Activo = 0).
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedido_ListarPorRangoFechas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio > @FechaFin
        THROW 50601, 'La fecha inicial no puede ser mayor que la fecha final.', 1;

    SELECT ped.PedidoID,
           ped.FechaPedido,
           cli.Empresa AS NombreCliente,
           pro.ProductoID,
           pro.NombreProducto,
           det.PrecioUnidad,
           det.Cantidad,
           det.Descuento,
           det.PrecioUnidad * det.Cantidad * (1 - det.Descuento) AS Subtotal
    FROM dbo.DetallePedidos det
    INNER JOIN dbo.Pedidos   ped ON ped.PedidoID   = det.PedidoID
    INNER JOIN dbo.Productos pro ON pro.ProductoID = det.ProductoID
    LEFT  JOIN dbo.Clientes  cli ON cli.ClienteID  = ped.ClienteID
    WHERE ped.Activo = 1
      AND ped.FechaPedido BETWEEN @FechaInicio AND @FechaFin
    ORDER BY ped.FechaPedido, ped.PedidoID, pro.NombreProducto;
END
GO
