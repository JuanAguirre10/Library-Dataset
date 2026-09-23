using CommunityToolkit.Mvvm.ComponentModel;

namespace NeptunoApp.Models;

public partial class Proveedor : ObservableObject
{
    [ObservableProperty]
    private int proveedorID;

    [ObservableProperty]
    private string companiaNombre = string.Empty;

    [ObservableProperty]
    private string? nombreContacto;

    [ObservableProperty]
    private string? cargoContacto;

    [ObservableProperty]
    private string? direccion;

    [ObservableProperty]
    private string? ciudad;

    [ObservableProperty]
    private string? codigoPostal;

    [ObservableProperty]
    private string? pais;

    [ObservableProperty]
    private string? telefono;

    [ObservableProperty]
    private string? fax;

    /// <summary>Estado para la eliminacion logica: false cuando el registro fue dado de baja.</summary>
    [ObservableProperty]
    private bool activo = true;
}
