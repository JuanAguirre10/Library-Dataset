/* ============================================================
   NeptunoDB_Lab06 - Procedimientos almacenados
   Laboratorio 06 - Class Library & DataSet
   ------------------------------------------------------------
   Requiere 00_NeptunoDB.sql y 01_NeptunoDB_EliminacionLogica.sql.
   Todos los procedimientos son CREATE OR ALTER, asi que este
   script se puede volver a ejecutar sin borrar la base de datos.

   Reglas que siguen todos los mantenimientos:
   - Listar / ObtenerPorId / Buscar devuelven solo Activo = 1.
   - Crear NO devuelve filas: el identificador generado sale por
     un parametro OUTPUT, para que la aplicacion ejecute el alta
     con ExecuteNonQuery y lea el valor del parametro.
   - Actualizar solo afecta registros activos.
   - Eliminar es LOGICO: UPDATE ... SET Activo = 0. Nunca DELETE.
   - Si no se afecta ninguna fila se lanza THROW, porque con
     SET NOCOUNT ON ExecuteNonQuery devuelve -1 y la aplicacion
     no puede usar el conteo de filas para detectarlo.
   ============================================================ */

USE NeptunoDB_Lab06;
GO

/* ============================================================
   CATEGORIAS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoriaID, NombreCategoria, Descripcion, Activo
    FROM dbo.Categorias
    WHERE Activo = 1
    ORDER BY NombreCategoria;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_ObtenerPorId
    @CategoriaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoriaID, NombreCategoria, Descripcion, Activo
    FROM dbo.Categorias
    WHERE CategoriaID = @CategoriaID
      AND Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Crear
    @NombreCategoria NVARCHAR(30),
    @Descripcion     NVARCHAR(200) = NULL,
    @CategoriaID     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Categorias (NombreCategoria, Descripcion)
        VALUES (@NombreCategoria, @Descripcion);

        SET @CategoriaID = CAST(SCOPE_IDENTITY() AS INT);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Actualizar
    @CategoriaID     INT,
    @NombreCategoria NVARCHAR(30),
    @Descripcion     NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Categorias
        SET NombreCategoria = @NombreCategoria,
            Descripcion     = @Descripcion
        WHERE CategoriaID = @CategoriaID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50101, 'La categoria no existe o fue eliminada.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Baja logica. Se bloquea mientras haya productos ACTIVOS en la
   categoria, para que ningun producto visible apunte a una
   categoria dada de baja. */
CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Eliminar
    @CategoriaID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Productos
                   WHERE CategoriaID = @CategoriaID AND Activo = 1)
            THROW 50102, 'No se puede eliminar: la categoria tiene productos activos asociados.', 1;

        UPDATE dbo.Categorias
        SET Activo = 0
        WHERE CategoriaID = @CategoriaID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50103, 'La categoria no existe o ya fue eliminada.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ============================================================
   PROVEEDORES
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProveedorID, CompaniaNombre, NombreContacto, CargoContacto, Direccion,
           Ciudad, CodigoPostal, Pais, Telefono, Fax, Activo
    FROM dbo.Proveedores
    WHERE Activo = 1
    ORDER BY CompaniaNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_ObtenerPorId
    @ProveedorID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProveedorID, CompaniaNombre, NombreContacto, CargoContacto, Direccion,
           Ciudad, CodigoPostal, Pais, Telefono, Fax, Activo
    FROM dbo.Proveedores
    WHERE ProveedorID = @ProveedorID
      AND Activo = 1;
END
GO

/* Listado de proveedores buscando por nombreContacto y ciudad.
   Los dos filtros son opcionales: si llegan nulos o vacios no restringen.
   Solo devuelve proveedores con Activo = 1. */
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Buscar
    @NombreContacto NVARCHAR(40) = NULL,
    @Ciudad         NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF LTRIM(RTRIM(ISNULL(@NombreContacto, ''))) = '' SET @NombreContacto = NULL;
    IF LTRIM(RTRIM(ISNULL(@Ciudad, ''))) = '' SET @Ciudad = NULL;

    SELECT ProveedorID, CompaniaNombre, NombreContacto, CargoContacto, Direccion,
           Ciudad, CodigoPostal, Pais, Telefono, Fax, Activo
    FROM dbo.Proveedores
    WHERE Activo = 1
      AND (@NombreContacto IS NULL OR NombreContacto LIKE '%' + @NombreContacto + '%')
      AND (@Ciudad IS NULL OR Ciudad LIKE '%' + @Ciudad + '%')
    ORDER BY CompaniaNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Crear
    @CompaniaNombre NVARCHAR(60),
    @NombreContacto NVARCHAR(40) = NULL,
    @CargoContacto  NVARCHAR(40) = NULL,
    @Direccion      NVARCHAR(80) = NULL,
    @Ciudad         NVARCHAR(30) = NULL,
    @CodigoPostal   NVARCHAR(10) = NULL,
    @Pais           NVARCHAR(30) = NULL,
    @Telefono       NVARCHAR(24) = NULL,
    @Fax            NVARCHAR(24) = NULL,
    @ProveedorID    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Proveedores
            (CompaniaNombre, NombreContacto, CargoContacto, Direccion, Ciudad,
             CodigoPostal, Pais, Telefono, Fax)
        VALUES
            (@CompaniaNombre, @NombreContacto, @CargoContacto, @Direccion, @Ciudad,
             @CodigoPostal, @Pais, @Telefono, @Fax);

        SET @ProveedorID = CAST(SCOPE_IDENTITY() AS INT);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Actualizar
    @ProveedorID    INT,
    @CompaniaNombre NVARCHAR(60),
    @NombreContacto NVARCHAR(40) = NULL,
    @CargoContacto  NVARCHAR(40) = NULL,
    @Direccion      NVARCHAR(80) = NULL,
    @Ciudad         NVARCHAR(30) = NULL,
    @CodigoPostal   NVARCHAR(10) = NULL,
    @Pais           NVARCHAR(30) = NULL,
    @Telefono       NVARCHAR(24) = NULL,
    @Fax            NVARCHAR(24) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Proveedores
        SET CompaniaNombre = @CompaniaNombre,
            NombreContacto = @NombreContacto,
            CargoContacto  = @CargoContacto,
            Direccion      = @Direccion,
            Ciudad         = @Ciudad,
            CodigoPostal   = @CodigoPostal,
            Pais           = @Pais,
            Telefono       = @Telefono,
            Fax            = @Fax
        WHERE ProveedorID = @ProveedorID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50201, 'El proveedor no existe o fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Baja logica. Igual que en categorias, se bloquea mientras el
   proveedor tenga productos activos. */
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Eliminar
    @ProveedorID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Productos
                   WHERE ProveedorID = @ProveedorID AND Activo = 1)
            THROW 50202, 'No se puede eliminar: el proveedor tiene productos activos asociados.', 1;

        UPDATE dbo.Proveedores
        SET Activo = 0
        WHERE ProveedorID = @ProveedorID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50203, 'El proveedor no existe o ya fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ============================================================
   PRODUCTOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProductoID, p.NombreProducto, p.ProveedorID, p.CategoriaID,
           p.CantidadPorUnidad, p.PrecioUnidad, p.UnidadesEnExistencia,
           p.UnidadesEnPedido, p.NivelDeReorden, p.Descontinuado, p.Activo,
           c.NombreCategoria,
           pr.CompaniaNombre AS NombreProveedor
    FROM dbo.Productos p
    LEFT JOIN dbo.Categorias  c  ON c.CategoriaID  = p.CategoriaID
    LEFT JOIN dbo.Proveedores pr ON pr.ProveedorID = p.ProveedorID
    WHERE p.Activo = 1
    ORDER BY p.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_ObtenerPorId
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProductoID, p.NombreProducto, p.ProveedorID, p.CategoriaID,
           p.CantidadPorUnidad, p.PrecioUnidad, p.UnidadesEnExistencia,
           p.UnidadesEnPedido, p.NivelDeReorden, p.Descontinuado, p.Activo,
           c.NombreCategoria,
           pr.CompaniaNombre AS NombreProveedor
    FROM dbo.Productos p
    LEFT JOIN dbo.Categorias  c  ON c.CategoriaID  = p.CategoriaID
    LEFT JOIN dbo.Proveedores pr ON pr.ProveedorID = p.ProveedorID
    WHERE p.ProductoID = @ProductoID
      AND p.Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Crear
    @NombreProducto       NVARCHAR(60),
    @ProveedorID          INT = NULL,
    @CategoriaID          INT = NULL,
    @CantidadPorUnidad    NVARCHAR(30) = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT = 0,
    @UnidadesEnPedido     SMALLINT = 0,
    @NivelDeReorden       SMALLINT = 0,
    @Descontinuado        BIT = 0,
    @ProductoID           INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @PrecioUnidad < 0
            THROW 50301, 'El precio unitario no puede ser negativo.', 1;

        IF @CategoriaID IS NOT NULL AND NOT EXISTS
           (SELECT 1 FROM dbo.Categorias WHERE CategoriaID = @CategoriaID AND Activo = 1)
            THROW 50306, 'La categoria seleccionada no existe o fue eliminada.', 1;

        IF @ProveedorID IS NOT NULL AND NOT EXISTS
           (SELECT 1 FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID AND Activo = 1)
            THROW 50307, 'El proveedor seleccionado no existe o fue eliminado.', 1;

        INSERT INTO dbo.Productos
            (NombreProducto, ProveedorID, CategoriaID, CantidadPorUnidad, PrecioUnidad,
             UnidadesEnExistencia, UnidadesEnPedido, NivelDeReorden, Descontinuado)
        VALUES
            (@NombreProducto, @ProveedorID, @CategoriaID, @CantidadPorUnidad, @PrecioUnidad,
             @UnidadesEnExistencia, @UnidadesEnPedido, @NivelDeReorden, @Descontinuado);

        SET @ProductoID = CAST(SCOPE_IDENTITY() AS INT);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Actualizar
    @ProductoID           INT,
    @NombreProducto       NVARCHAR(60),
    @ProveedorID          INT = NULL,
    @CategoriaID          INT = NULL,
    @CantidadPorUnidad    NVARCHAR(30) = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT = 0,
    @UnidadesEnPedido     SMALLINT = 0,
    @NivelDeReorden       SMALLINT = 0,
    @Descontinuado        BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @PrecioUnidad < 0
            THROW 50302, 'El precio unitario no puede ser negativo.', 1;

        IF @CategoriaID IS NOT NULL AND NOT EXISTS
           (SELECT 1 FROM dbo.Categorias WHERE CategoriaID = @CategoriaID AND Activo = 1)
            THROW 50306, 'La categoria seleccionada no existe o fue eliminada.', 1;

        IF @ProveedorID IS NOT NULL AND NOT EXISTS
           (SELECT 1 FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID AND Activo = 1)
            THROW 50307, 'El proveedor seleccionado no existe o fue eliminado.', 1;

        UPDATE dbo.Productos
        SET NombreProducto       = @NombreProducto,
            ProveedorID          = @ProveedorID,
            CategoriaID          = @CategoriaID,
            CantidadPorUnidad    = @CantidadPorUnidad,
            PrecioUnidad         = @PrecioUnidad,
            UnidadesEnExistencia = @UnidadesEnExistencia,
            UnidadesEnPedido     = @UnidadesEnPedido,
            NivelDeReorden       = @NivelDeReorden,
            Descontinuado        = @Descontinuado
        WHERE ProductoID = @ProductoID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50303, 'El producto no existe o fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Baja logica. A diferencia del laboratorio 04 ya no se bloquea
   cuando el producto figura en pedidos: la fila se conserva, asi
   que las lineas de pedido y los reportes historicos siguen
   mostrando el nombre del producto. */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Eliminar
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Productos
        SET Activo = 0
        WHERE ProductoID = @ProductoID
          AND Activo = 1;

        IF @@ROWCOUNT = 0
            THROW 50305, 'El producto no existe o ya fue eliminado.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
