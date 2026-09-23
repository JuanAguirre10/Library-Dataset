using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class ProveedorEditViewModel : ObservableObject
{
    public int ProveedorID { get; }

    [ObservableProperty]
    private string companiaNombre;

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

    public bool EsNuevo => ProveedorID == 0;
    public string TituloVentana => EsNuevo ? "Nuevo proveedor" : "Editar proveedor";

    public ProveedorEditViewModel(Proveedor? proveedor = null)
    {
        ProveedorID = proveedor?.ProveedorID ?? 0;
        companiaNombre = proveedor?.CompaniaNombre ?? string.Empty;
        nombreContacto = proveedor?.NombreContacto;
        cargoContacto = proveedor?.CargoContacto;
        direccion = proveedor?.Direccion;
        ciudad = proveedor?.Ciudad;
        codigoPostal = proveedor?.CodigoPostal;
        pais = proveedor?.Pais;
        telefono = proveedor?.Telefono;
        fax = proveedor?.Fax;
    }

    public string? Validar()
    {
        if (string.IsNullOrWhiteSpace(CompaniaNombre))
            return "El nombre de la compañía es obligatorio.";

        if (CompaniaNombre.Trim().Length > 60)
            return "El nombre de la compañía no puede superar los 60 caracteres.";

        return null;
    }

    public Proveedor AModelo() => new()
    {
        ProveedorID = ProveedorID,
        CompaniaNombre = CompaniaNombre.Trim(),
        NombreContacto = Limpiar(NombreContacto),
        CargoContacto = Limpiar(CargoContacto),
        Direccion = Limpiar(Direccion),
        Ciudad = Limpiar(Ciudad),
        CodigoPostal = Limpiar(CodigoPostal),
        Pais = Limpiar(Pais),
        Telefono = Limpiar(Telefono),
        Fax = Limpiar(Fax)
    };

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
