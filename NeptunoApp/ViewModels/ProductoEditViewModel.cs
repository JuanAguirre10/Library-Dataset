using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class ProductoEditViewModel : ObservableObject
{
    public int ProductoID { get; }

    /// <summary>Categorias disponibles para el combo del formulario.</summary>
    public ObservableCollection<Categoria> Categorias { get; }

    /// <summary>Proveedores disponibles para el combo del formulario.</summary>
    public ObservableCollection<Proveedor> Proveedores { get; }

    [ObservableProperty]
    private string nombreProducto;

    [ObservableProperty]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    private Proveedor? proveedorSeleccionado;

    [ObservableProperty]
    private string? cantidadPorUnidad;

    [ObservableProperty]
    private decimal precioUnidad;

    [ObservableProperty]
    private short unidadesEnExistencia;

    [ObservableProperty]
    private short unidadesEnPedido;

    [ObservableProperty]
    private short nivelDeReorden;

    [ObservableProperty]
    private bool descontinuado;

    public bool EsNuevo => ProductoID == 0;
    public string TituloVentana => EsNuevo ? "Nuevo producto" : "Editar producto";

    public ProductoEditViewModel(
        IEnumerable<Categoria> categorias,
        IEnumerable<Proveedor> proveedores,
        Producto? producto = null)
    {
        Categorias = new ObservableCollection<Categoria>(categorias);
        Proveedores = new ObservableCollection<Proveedor>(proveedores);

        ProductoID = producto?.ProductoID ?? 0;
        nombreProducto = producto?.NombreProducto ?? string.Empty;
        cantidadPorUnidad = producto?.CantidadPorUnidad;
        precioUnidad = producto?.PrecioUnidad ?? 0m;
        unidadesEnExistencia = producto?.UnidadesEnExistencia ?? 0;
        unidadesEnPedido = producto?.UnidadesEnPedido ?? 0;
        nivelDeReorden = producto?.NivelDeReorden ?? 0;
        descontinuado = producto?.Descontinuado ?? false;

        categoriaSeleccionada = Categorias.FirstOrDefault(c => c.CategoriaID == producto?.CategoriaID);
        proveedorSeleccionado = Proveedores.FirstOrDefault(p => p.ProveedorID == producto?.ProveedorID);
    }

    public string? Validar()
    {
        if (string.IsNullOrWhiteSpace(NombreProducto))
            return "El nombre del producto es obligatorio.";

        if (NombreProducto.Trim().Length > 60)
            return "El nombre no puede superar los 60 caracteres.";

        if (PrecioUnidad < 0)
            return "El precio unitario no puede ser negativo.";

        if (UnidadesEnExistencia < 0 || UnidadesEnPedido < 0 || NivelDeReorden < 0)
            return "Las unidades no pueden ser negativas.";

        return null;
    }

    public Producto AModelo() => new()
    {
        ProductoID = ProductoID,
        NombreProducto = NombreProducto.Trim(),
        CategoriaID = CategoriaSeleccionada?.CategoriaID,
        ProveedorID = ProveedorSeleccionado?.ProveedorID,
        CantidadPorUnidad = string.IsNullOrWhiteSpace(CantidadPorUnidad) ? null : CantidadPorUnidad.Trim(),
        PrecioUnidad = PrecioUnidad,
        UnidadesEnExistencia = UnidadesEnExistencia,
        UnidadesEnPedido = UnidadesEnPedido,
        NivelDeReorden = NivelDeReorden,
        Descontinuado = Descontinuado
    };
}
