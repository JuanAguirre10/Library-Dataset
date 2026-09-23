using NeptunoApp.Models;

namespace NeptunoApp.Data;

public interface ICategoriaRepository
{
    Task<List<Categoria>> ListarAsync();
    Task<Categoria?> ObtenerPorIdAsync(int categoriaId);
    Task<int> CrearAsync(Categoria categoria);
    Task ActualizarAsync(Categoria categoria);
    /// <summary>Eliminacion logica: ejecuta con ExecuteNonQuery un procedimiento que pone Activo = 0.</summary>
    Task EliminarAsync(int categoriaId);
}
