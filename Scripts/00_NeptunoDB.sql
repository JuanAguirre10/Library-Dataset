/* ============================================================
   NeptunoDB_Lab06 (base de datos del Laboratorio 06)
   ------------------------------------------------------------
   Motor: SQL Server (T-SQL)
   ============================================================ */

--  Crear la BD 
IF DB_ID(N'NeptunoDB_Lab06') IS NULL
BEGIN
    CREATE DATABASE NeptunoDB_Lab06;
END
GO

USE NeptunoDB_Lab06;
GO

 

/* ------------------------------------------------------------
  Tablas principales
   ------------------------------------------------------------ */

CREATE TABLE dbo.Categorias (
    CategoriaID     INT IDENTITY(1,1) PRIMARY KEY,
    NombreCategoria NVARCHAR(30)  NOT NULL,
    Descripcion     NVARCHAR(200) NULL
);
GO

CREATE TABLE dbo.Proveedores (
    ProveedorID     INT IDENTITY(1,1) PRIMARY KEY,
    CompaniaNombre  NVARCHAR(60)  NOT NULL,
    NombreContacto  NVARCHAR(40)  NULL,
    CargoContacto   NVARCHAR(40)  NULL,
    Direccion       NVARCHAR(80)  NULL,
    Ciudad          NVARCHAR(30)  NULL,
    CodigoPostal    NVARCHAR(10)  NULL,
    Pais            NVARCHAR(30)  NULL,
    Telefono        NVARCHAR(24)  NULL,
    Fax             NVARCHAR(24)  NULL
);
GO

CREATE TABLE dbo.Clientes (
    ClienteID       INT IDENTITY(1,1) PRIMARY KEY,
    Empresa         NVARCHAR(60) NOT NULL,
    NombreContacto  NVARCHAR(40) NULL,
    Ciudad          NVARCHAR(30) NULL,
    Pais            NVARCHAR(30) NULL,
    Telefono        NVARCHAR(24) NULL
);
GO

CREATE TABLE dbo.Empleados (
    EmpleadoID       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre           NVARCHAR(20) NOT NULL,
    Apellidos        NVARCHAR(30) NOT NULL,
    Cargo            NVARCHAR(40) NULL,
    FechaNacimiento  DATE NULL,
    FechaContratacion DATE NULL,
    Ciudad           NVARCHAR(30) NULL,
    Pais             NVARCHAR(30) NULL
);
GO

CREATE TABLE dbo.Transportistas (
    TransportistaID INT IDENTITY(1,1) PRIMARY KEY,
    CompaniaNombre  NVARCHAR(60) NOT NULL,
    Telefono        NVARCHAR(24) NULL
);
GO

CREATE TABLE dbo.Productos (
    ProductoID          INT IDENTITY(1,1) PRIMARY KEY,
    NombreProducto      NVARCHAR(60)   NOT NULL,
    ProveedorID         INT            NULL,
    CategoriaID         INT            NULL,
    CantidadPorUnidad   NVARCHAR(30)   NULL,
    PrecioUnidad        DECIMAL(10,2)  NOT NULL DEFAULT 0,
    UnidadesEnExistencia SMALLINT      NOT NULL DEFAULT 0,
    UnidadesEnPedido    SMALLINT       NOT NULL DEFAULT 0,
    NivelDeReorden      SMALLINT       NOT NULL DEFAULT 0,
    Descontinuado       BIT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_Productos_Proveedores FOREIGN KEY (ProveedorID) REFERENCES dbo.Proveedores(ProveedorID),
    CONSTRAINT FK_Productos_Categorias  FOREIGN KEY (CategoriaID) REFERENCES dbo.Categorias(CategoriaID)
);
GO

CREATE TABLE dbo.Pedidos (
    PedidoID        INT IDENTITY(1,1) PRIMARY KEY,
    ClienteID       INT NULL,
    EmpleadoID      INT NULL,
    FechaPedido     DATE NOT NULL,
    FechaRequerida  DATE NULL,
    FechaEnvio      DATE NULL,
    TransportistaID INT NULL,
    Destinatario    NVARCHAR(60) NULL,
    CiudadDestino   NVARCHAR(30) NULL,
    PaisDestino     NVARCHAR(30) NULL,
    CONSTRAINT FK_Pedidos_Clientes       FOREIGN KEY (ClienteID)       REFERENCES dbo.Clientes(ClienteID),
    CONSTRAINT FK_Pedidos_Empleados      FOREIGN KEY (EmpleadoID)      REFERENCES dbo.Empleados(EmpleadoID),
    CONSTRAINT FK_Pedidos_Transportistas FOREIGN KEY (TransportistaID) REFERENCES dbo.Transportistas(TransportistaID)
);
GO

CREATE TABLE dbo.DetallePedidos (
    PedidoID     INT NOT NULL,
    ProductoID   INT NOT NULL,
    PrecioUnidad DECIMAL(10,2) NOT NULL,
    Cantidad     SMALLINT NOT NULL DEFAULT 1,
    Descuento    DECIMAL(4,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_DetallePedidos PRIMARY KEY (PedidoID, ProductoID),
    CONSTRAINT FK_DetallePedidos_Pedidos   FOREIGN KEY (PedidoID)   REFERENCES dbo.Pedidos(PedidoID),
    CONSTRAINT FK_DetallePedidos_Productos FOREIGN KEY (ProductoID) REFERENCES dbo.Productos(ProductoID)
);
GO

/* ------------------------------------------------------------
   Información Base
   ------------------------------------------------------------ */

-- Categorias
INSERT INTO dbo.Categorias (NombreCategoria, Descripcion) VALUES
(N'Bebidas',            N'Refrescos, caf�s, t�s, cervezas y otras bebidas'),
(N'Condimentos',        N'Salsas, especias y aderezos'),
(N'Confituras',         N'Mermeladas, dulces y postres'),
(N'L�cteos',            N'Quesos y otros productos l�cteos'),
(N'Carnes y Embutidos', N'Carnes preparadas y embutidos');
GO

-- Proveedores
INSERT INTO dbo.Proveedores (CompaniaNombre, NombreContacto, CargoContacto, Direccion, Ciudad, CodigoPostal, Pais, Telefono, Fax) VALUES
(N'L�cteos Garc�a S.A.',       N'Ana Garc�a',       N'Gerente de Ventas',            N'Av. Los �lamos 245', N'Lima',      N'15024', N'Per�', N'511-4567890', N'511-4567891'),
(N'Bebidas del Sur Ltda.',     N'Carlos Ram�rez',   N'Jefe Comercial',               N'Jr. Comercio 890',   N'Arequipa',  N'04001', N'Per�', N'054-223344', N'054-223345'),
(N'Embutidos La Preferida',    N'Mar�a Torres',     N'Coordinadora de Distribuci�n', N'Calle Las Flores 120', N'Trujillo', N'13001', N'Per�', N'044-556677', N'044-556678'),
(N'Condimentos Andinos SAC',   N'Jorge Quispe',     N'Gerente General',              N'Av. Industrial 500', N'Cusco',     N'08001', N'Per�', N'084-778899', N'084-778900'),
(N'Dulces del Valle E.I.R.L.', N'Luc�a Fern�ndez',  N'Encargada de Ventas',          N'Jr. San Mart�n 77',  N'Chiclayo',  N'14001', N'Per�', N'074-991122', N'074-991123');
GO

-- Clientes
INSERT INTO dbo.Clientes (Empresa, NombreContacto, Ciudad, Pais, Telefono) VALUES
(N'Comercial Andina SAC',        N'Pedro Salazar',           N'Lima',     N'Per�', N'511-2345678'),
(N'Supermercados del Norte',     N'Rosa Medina',             N'Trujillo', N'Per�', N'044-334455'),
(N'Distribuidora Sure�a EIRL',   N'Luis Ch�vez',             N'Arequipa', N'Per�', N'054-667788'),
(N'Minimarket Central',          N'Elena Rojas',             N'Cusco',    N'Per�', N'084-112233'),
(N'Tiendas Express SAC',         N'Miguel �ngel Paredes',    N'Chiclayo', N'Per�', N'074-445566');
GO

-- Empleados
INSERT INTO dbo.Empleados (Nombre, Apellidos, Cargo, FechaNacimiento, FechaContratacion, Ciudad, Pais) VALUES
(N'Juan',   N'P�rez G�mez',     N'Vendedor',           '1990-05-12', '2020-01-15', N'Lima',     N'Per�'),
(N'Mar�a',  N'L�pez D�az',      N'Supervisora de Ventas', '1988-09-23', '2018-03-01', N'Lima',   N'Per�'),
(N'Carlos', N'Ruiz Mendoza',    N'Vendedor',           '1992-02-17', '2021-06-10', N'Arequipa', N'Per�'),
(N'Sof�a',  N'Vargas Castro',   N'Gerente Regional',   '1985-11-30', '2015-08-20', N'Trujillo', N'Per�'),
(N'Diego',  N'Fern�ndez R�os',  N'Vendedor',           '1995-07-08', '2022-02-01', N'Cusco',    N'Per�');
GO

-- Transportistas
INSERT INTO dbo.Transportistas (CompaniaNombre, Telefono) VALUES
(N'Transportes R�pido SAC',   N'511-8889900'),
(N'Env�os Seguros EIRL',      N'511-7776655'),
(N'Log�stica del Pac�fico',   N'054-990011'),
(N'Courier Nacional SA',      N'044-223344'),
(N'TransAndino Express',      N'084-556677');
GO

-- Productos
INSERT INTO dbo.Productos (NombreProducto, ProveedorID, CategoriaID, CantidadPorUnidad, PrecioUnidad, UnidadesEnExistencia, UnidadesEnPedido, NivelDeReorden, Descontinuado) VALUES
(N'Caf� Andino Premium',    2, 1, N'500 g',  45.90, 120, 30, 20, 0),
(N'Salsa de Aj� Amarillo',  4, 2, N'300 ml', 12.50, 200, 50, 30, 0),
(N'Mermelada de Aguaymanto',5, 3, N'250 g',  15.00,  80, 20, 15, 0),
(N'Queso Fresco Andino',    1, 4, N'1 kg',   22.00,  60, 10, 10, 0),
(N'Chorizo Ahumado',        3, 5, N'500 g',  18.75,  90, 25, 20, 0);
GO

-- Pedidos
INSERT INTO dbo.Pedidos (ClienteID, EmpleadoID, FechaPedido, FechaRequerida, FechaEnvio, TransportistaID, Destinatario, CiudadDestino, PaisDestino) VALUES
(1, 1, '2026-08-10', '2026-08-20', '2026-08-15', 1, N'Comercial Andina SAC',      N'Lima',     N'Per�'),
(2, 3, '2026-08-12', '2026-08-22', '2026-08-18', 3, N'Supermercados del Norte',   N'Trujillo', N'Per�'),
(3, 2, '2026-08-15', '2026-08-25', NULL,         4, N'Distribuidora Sure�a EIRL', N'Arequipa', N'Per�'),
(4, 4, '2026-08-20', '2026-08-30', '2026-08-26', 5, N'Minimarket Central',        N'Cusco',    N'Per�'),
(5, 5, '2026-08-22', '2026-09-01', '2026-08-28', 2, N'Tiendas Express SAC',       N'Chiclayo', N'Per�');
GO

-- DetallePedidos
INSERT INTO dbo.DetallePedidos (PedidoID, ProductoID, PrecioUnidad, Cantidad, Descuento) VALUES
(1, 1, 45.90, 10, 0.00),
(2, 2, 12.50, 25, 0.05),
(3, 3, 15.00, 15, 0.00),
(4, 4, 22.00,  8, 0.10),
(5, 5, 18.75, 12, 0.00);
GO

/* ------------------------------------------------------------
  Validar tablas
   ------------------------------------------------------------ */
SELECT 'Categorias' AS Tabla, COUNT(*) AS Registros FROM dbo.Categorias
UNION ALL SELECT 'Proveedores', COUNT(*) FROM dbo.Proveedores
UNION ALL SELECT 'Clientes', COUNT(*) FROM dbo.Clientes
UNION ALL SELECT 'Empleados', COUNT(*) FROM dbo.Empleados
UNION ALL SELECT 'Transportistas', COUNT(*) FROM dbo.Transportistas
UNION ALL SELECT 'Productos', COUNT(*) FROM dbo.Productos
UNION ALL SELECT 'Pedidos', COUNT(*) FROM dbo.Pedidos
UNION ALL SELECT 'DetallePedidos', COUNT(*) FROM dbo.DetallePedidos;
GO