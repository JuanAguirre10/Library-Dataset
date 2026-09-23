using System.Data;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class CategoriaRepository : RepositoryBase, ICategoriaRepository
{
    public CategoriaRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Categoria>> ListarAsync()
        => ListarAsync("dbo.usp_Categoria_Listar", Mapear);

    public Task<Categoria?> ObtenerPorIdAsync(int categoriaId)
        => ObtenerAsync("dbo.usp_Categoria_ObtenerPorId", Mapear, p =>
            p.Add("@CategoriaID", SqlDbType.Int).Value = categoriaId);

    public Task<int> CrearAsync(Categoria categoria)
        => InsertarAsync("dbo.usp_Categoria_Crear", "@CategoriaID", p =>
        {
            p.Add("@NombreCategoria", SqlDbType.NVarChar, 30).Value = categoria.NombreCategoria;
            p.Add("@Descripcion", SqlDbType.NVarChar, 200).Value = Valor(categoria.Descripcion);
        });

    public Task ActualizarAsync(Categoria categoria)
        => EjecutarAsync("dbo.usp_Categoria_Actualizar", p =>
        {
            p.Add("@CategoriaID", SqlDbType.Int).Value = categoria.CategoriaID;
            p.Add("@NombreCategoria", SqlDbType.NVarChar, 30).Value = categoria.NombreCategoria;
            p.Add("@Descripcion", SqlDbType.NVarChar, 200).Value = Valor(categoria.Descripcion);
        });

    public Task EliminarAsync(int categoriaId)
        => EjecutarAsync("dbo.usp_Categoria_Eliminar", p =>
            p.Add("@CategoriaID", SqlDbType.Int).Value = categoriaId);

    private static Categoria Mapear(DataRow fila) => new()
    {
        CategoriaID = fila.Entero("CategoriaID"),
        NombreCategoria = fila.Texto("NombreCategoria"),
        Descripcion = fila.TextoNulo("Descripcion"),
        Activo = fila.Booleano("Activo")
    };
}
