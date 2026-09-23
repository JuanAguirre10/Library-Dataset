using System.Data;
using Microsoft.Data.SqlClient;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class ProductoRepository : RepositoryBase, IProductoRepository
{
    public ProductoRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Producto>> ListarAsync()
        => ListarDesconectadoAsync("dbo.usp_Producto_Listar", Mapear);

    public Task<Producto?> ObtenerPorIdAsync(int productoId)
        => ObtenerConectadoAsync("dbo.usp_Producto_ObtenerPorId", Mapear, p =>
            p.Add("@ProductoID", SqlDbType.Int).Value = productoId);

    public Task<int> CrearAsync(Producto producto)
        => InsertarAsync("dbo.usp_Producto_Crear", "@ProductoID", p => AgregarDatos(p, producto));

    public Task ActualizarAsync(Producto producto)
        => EjecutarAsync("dbo.usp_Producto_Actualizar", p =>
        {
            p.Add("@ProductoID", SqlDbType.Int).Value = producto.ProductoID;
            AgregarDatos(p, producto);
        });

    public Task EliminarAsync(int productoId)
        => EjecutarAsync("dbo.usp_Producto_Eliminar", p =>
            p.Add("@ProductoID", SqlDbType.Int).Value = productoId);

    private static void AgregarDatos(SqlParameterCollection p, Producto producto)
    {
        p.Add("@NombreProducto", SqlDbType.NVarChar, 60).Value = producto.NombreProducto;
        p.Add("@ProveedorID", SqlDbType.Int).Value = Valor(producto.ProveedorID);
        p.Add("@CategoriaID", SqlDbType.Int).Value = Valor(producto.CategoriaID);
        p.Add("@CantidadPorUnidad", SqlDbType.NVarChar, 30).Value = Valor(producto.CantidadPorUnidad);
        p.Add("@PrecioUnidad", SqlDbType.Decimal).Value = producto.PrecioUnidad;
        p.Add("@UnidadesEnExistencia", SqlDbType.SmallInt).Value = producto.UnidadesEnExistencia;
        p.Add("@UnidadesEnPedido", SqlDbType.SmallInt).Value = producto.UnidadesEnPedido;
        p.Add("@NivelDeReorden", SqlDbType.SmallInt).Value = producto.NivelDeReorden;
        p.Add("@Descontinuado", SqlDbType.Bit).Value = producto.Descontinuado;
    }

    private static Producto Mapear(IDataRecord registro) => new()
    {
        ProductoID = registro.Entero("ProductoID"),
        NombreProducto = registro.Texto("NombreProducto"),
        ProveedorID = registro.EnteroNulo("ProveedorID"),
        CategoriaID = registro.EnteroNulo("CategoriaID"),
        CantidadPorUnidad = registro.TextoNulo("CantidadPorUnidad"),
        PrecioUnidad = registro.Decimal("PrecioUnidad"),
        UnidadesEnExistencia = registro.Corto("UnidadesEnExistencia"),
        UnidadesEnPedido = registro.Corto("UnidadesEnPedido"),
        NivelDeReorden = registro.Corto("NivelDeReorden"),
        Descontinuado = registro.Booleano("Descontinuado"),
        Activo = registro.Booleano("Activo"),
        NombreCategoria = registro.TextoNulo("NombreCategoria"),
        NombreProveedor = registro.TextoNulo("NombreProveedor")
    };
}
