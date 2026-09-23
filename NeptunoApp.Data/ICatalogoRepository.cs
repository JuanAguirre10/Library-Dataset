using NeptunoApp.Models;

namespace NeptunoApp.Data;

/// <summary>Listados de solo lectura que llenan los combos de la interfaz.</summary>
public interface ICatalogoRepository
{
    Task<List<Cliente>> ListarClientesAsync();
    Task<List<Empleado>> ListarEmpleadosAsync();
    Task<List<Transportista>> ListarTransportistasAsync();
}
