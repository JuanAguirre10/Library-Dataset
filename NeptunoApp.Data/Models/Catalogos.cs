namespace NeptunoApp.Models;

/* Entidades de solo lectura que alimentan los combos de la interfaz.
   No cambian mientras la ventana esta abierta, por eso no necesitan
   notificacion de cambios. */

public class Cliente
{
    public int ClienteID { get; init; }
    public string Empresa { get; init; } = string.Empty;
    public string? NombreContacto { get; init; }
    public string? Ciudad { get; init; }
    public string? Pais { get; init; }
    public string? Telefono { get; init; }
}

public class Empleado
{
    public int EmpleadoID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Cargo { get; init; }
    public string? Ciudad { get; init; }
    public string? Pais { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
}

public class Transportista
{
    public int TransportistaID { get; init; }
    public string CompaniaNombre { get; init; } = string.Empty;
    public string? Telefono { get; init; }
}
