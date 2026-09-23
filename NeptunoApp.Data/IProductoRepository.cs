using NeptunoApp.Models;

namespace NeptunoApp.Data;

public interface IProductoRepository
{
    Task<List<Producto>> ListarAsync();
    Task<Producto?> ObtenerPorIdAsync(int productoId);
    Task<int> CrearAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    /// <summary>Eliminacion logica: ejecuta con ExecuteNonQuery un procedimiento que pone Activo = 0.</summary>
    Task EliminarAsync(int productoId);
}
