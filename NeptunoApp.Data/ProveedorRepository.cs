using System.Data;
using Microsoft.Data.SqlClient;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class ProveedorRepository : RepositoryBase, IProveedorRepository
{
    public ProveedorRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Proveedor>> ListarAsync()
        => ListarAsync("dbo.usp_Proveedor_Listar", Mapear);

    public Task<List<Proveedor>> BuscarAsync(string? nombreContacto, string? ciudad)
        => ListarAsync("dbo.usp_Proveedor_Buscar", Mapear, p =>
        {
            p.Add("@NombreContacto", SqlDbType.NVarChar, 40).Value = Valor(nombreContacto);
            p.Add("@Ciudad", SqlDbType.NVarChar, 30).Value = Valor(ciudad);
        });

    public Task<Proveedor?> ObtenerPorIdAsync(int proveedorId)
        => ObtenerAsync("dbo.usp_Proveedor_ObtenerPorId", Mapear, p =>
            p.Add("@ProveedorID", SqlDbType.Int).Value = proveedorId);

    public Task<int> CrearAsync(Proveedor proveedor)
        => InsertarAsync("dbo.usp_Proveedor_Crear", "@ProveedorID", p => AgregarDatos(p, proveedor));

    public Task ActualizarAsync(Proveedor proveedor)
        => EjecutarAsync("dbo.usp_Proveedor_Actualizar", p =>
        {
            p.Add("@ProveedorID", SqlDbType.Int).Value = proveedor.ProveedorID;
            AgregarDatos(p, proveedor);
        });

    public Task EliminarAsync(int proveedorId)
        => EjecutarAsync("dbo.usp_Proveedor_Eliminar", p =>
            p.Add("@ProveedorID", SqlDbType.Int).Value = proveedorId);

    /// <summary>Parametros comunes al alta y a la actualizacion.</summary>
    private static void AgregarDatos(SqlParameterCollection p, Proveedor proveedor)
    {
        p.Add("@CompaniaNombre", SqlDbType.NVarChar, 60).Value = proveedor.CompaniaNombre;
        p.Add("@NombreContacto", SqlDbType.NVarChar, 40).Value = Valor(proveedor.NombreContacto);
        p.Add("@CargoContacto", SqlDbType.NVarChar, 40).Value = Valor(proveedor.CargoContacto);
        p.Add("@Direccion", SqlDbType.NVarChar, 80).Value = Valor(proveedor.Direccion);
        p.Add("@Ciudad", SqlDbType.NVarChar, 30).Value = Valor(proveedor.Ciudad);
        p.Add("@CodigoPostal", SqlDbType.NVarChar, 10).Value = Valor(proveedor.CodigoPostal);
        p.Add("@Pais", SqlDbType.NVarChar, 30).Value = Valor(proveedor.Pais);
        p.Add("@Telefono", SqlDbType.NVarChar, 24).Value = Valor(proveedor.Telefono);
        p.Add("@Fax", SqlDbType.NVarChar, 24).Value = Valor(proveedor.Fax);
    }

    private static Proveedor Mapear(DataRow fila) => new()
    {
        ProveedorID = fila.Entero("ProveedorID"),
        CompaniaNombre = fila.Texto("CompaniaNombre"),
        NombreContacto = fila.TextoNulo("NombreContacto"),
        CargoContacto = fila.TextoNulo("CargoContacto"),
        Direccion = fila.TextoNulo("Direccion"),
        Ciudad = fila.TextoNulo("Ciudad"),
        CodigoPostal = fila.TextoNulo("CodigoPostal"),
        Pais = fila.TextoNulo("Pais"),
        Telefono = fila.TextoNulo("Telefono"),
        Fax = fila.TextoNulo("Fax"),
        Activo = fila.Booleano("Activo")
    };
}
