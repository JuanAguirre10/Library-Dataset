using System.Data;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class CatalogoRepository : RepositoryBase, ICatalogoRepository
{
    public CatalogoRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Cliente>> ListarClientesAsync()
        => ListarAsync("dbo.usp_Cliente_Listar", fila => new Cliente
        {
            ClienteID = fila.Entero("ClienteID"),
            Empresa = fila.Texto("Empresa"),
            NombreContacto = fila.TextoNulo("NombreContacto"),
            Ciudad = fila.TextoNulo("Ciudad"),
            Pais = fila.TextoNulo("Pais"),
            Telefono = fila.TextoNulo("Telefono")
        });

    public Task<List<Empleado>> ListarEmpleadosAsync()
        => ListarAsync("dbo.usp_Empleado_Listar", fila => new Empleado
        {
            EmpleadoID = fila.Entero("EmpleadoID"),
            Nombre = fila.Texto("Nombre"),
            Apellidos = fila.Texto("Apellidos"),
            Cargo = fila.TextoNulo("Cargo"),
            Ciudad = fila.TextoNulo("Ciudad"),
            Pais = fila.TextoNulo("Pais"),
            NombreCompleto = fila.Texto("NombreCompleto")
        });

    public Task<List<Transportista>> ListarTransportistasAsync()
        => ListarAsync("dbo.usp_Transportista_Listar", fila => new Transportista
        {
            TransportistaID = fila.Entero("TransportistaID"),
            CompaniaNombre = fila.Texto("CompaniaNombre"),
            Telefono = fila.TextoNulo("Telefono")
        });
}
