using System.Data;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class CatalogoRepository : RepositoryBase, ICatalogoRepository
{
    public CatalogoRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Cliente>> ListarClientesAsync()
        => ListarDesconectadoAsync("dbo.usp_Cliente_Listar", registro => new Cliente
        {
            ClienteID = registro.Entero("ClienteID"),
            Empresa = registro.Texto("Empresa"),
            NombreContacto = registro.TextoNulo("NombreContacto"),
            Ciudad = registro.TextoNulo("Ciudad"),
            Pais = registro.TextoNulo("Pais"),
            Telefono = registro.TextoNulo("Telefono")
        });

    public Task<List<Empleado>> ListarEmpleadosAsync()
        => ListarDesconectadoAsync("dbo.usp_Empleado_Listar", registro => new Empleado
        {
            EmpleadoID = registro.Entero("EmpleadoID"),
            Nombre = registro.Texto("Nombre"),
            Apellidos = registro.Texto("Apellidos"),
            Cargo = registro.TextoNulo("Cargo"),
            Ciudad = registro.TextoNulo("Ciudad"),
            Pais = registro.TextoNulo("Pais"),
            NombreCompleto = registro.Texto("NombreCompleto")
        });

    public Task<List<Transportista>> ListarTransportistasAsync()
        => ListarDesconectadoAsync("dbo.usp_Transportista_Listar", registro => new Transportista
        {
            TransportistaID = registro.Entero("TransportistaID"),
            CompaniaNombre = registro.Texto("CompaniaNombre"),
            Telefono = registro.TextoNulo("Telefono")
        });
}
