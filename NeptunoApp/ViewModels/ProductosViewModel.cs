using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeptunoApp.Data;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class ProductosViewModel : ViewModelBase
{
    private readonly IProductoRepository _repositorio;
    private readonly ICategoriaRepository _categoriaRepositorio;
    private readonly IProveedorRepository _proveedorRepositorio;

    public ObservableCollection<Producto> Productos { get; } = new();

    [ObservableProperty]
    private Producto? seleccionado;

    [ObservableProperty]
    private bool hayRegistros;

    public event Action<ProductoEditViewModel>? EdicionSolicitada;

    public ProductosViewModel(
        IProductoRepository repositorio,
        ICategoriaRepository categoriaRepositorio,
        IProveedorRepository proveedorRepositorio)
    {
        _repositorio = repositorio;
        _categoriaRepositorio = categoriaRepositorio;
        _proveedorRepositorio = proveedorRepositorio;
    }

    [RelayCommand]
    private Task CargarAsync() => EjecutarAsync(async () =>
    {
        var resultado = await _repositorio.ListarAsync();

        Productos.Clear();
        foreach (var producto in resultado)
        {
            Productos.Add(producto);
        }
        HayRegistros = Productos.Count > 0;
    }, "No se pudo cargar el listado de productos");

    [RelayCommand]
    private Task NuevoAsync() => EjecutarAsync(async () =>
    {
        var (categorias, proveedores) = await CargarCombosAsync();
        EdicionSolicitada?.Invoke(new ProductoEditViewModel(categorias, proveedores));
    }, "No se pudo abrir el formulario");

    [RelayCommand]
    private Task EditarAsync(Producto? producto) => EjecutarAsync(async () =>
    {
        if (producto is null) return;

        var actual = await _repositorio.ObtenerPorIdAsync(producto.ProductoID);
        if (actual is null)
        {
            await CargarAsync();
            ErrorMessage = "El producto ya no existe.";
            return;
        }

        var (categorias, proveedores) = await CargarCombosAsync();
        EdicionSolicitada?.Invoke(new ProductoEditViewModel(categorias, proveedores, actual));
    }, "No se pudo abrir el producto");

    [RelayCommand]
    private Task GuardarAsync(ProductoEditViewModel? edicion) => EjecutarAsync(async () =>
    {
        if (edicion is null) return;

        var producto = edicion.AModelo();
        if (edicion.EsNuevo)
        {
            await _repositorio.CrearAsync(producto);
        }
        else
        {
            await _repositorio.ActualizarAsync(producto);
        }

        await CargarAsync();
    }, "No se pudo guardar el producto");

    [RelayCommand]
    private Task EliminarAsync(Producto? producto) => EjecutarAsync(async () =>
    {
        if (producto is null) return;
        if (!Confirmar($"¿Eliminar el producto \"{producto.NombreProducto}\"?\n\nEs una eliminación lógica: el registro se marca como inactivo (Activo = 0), deja de aparecer en los listados y se conserva en la base de datos.")) return;

        await _repositorio.EliminarAsync(producto.ProductoID);
        Productos.Remove(producto);
        HayRegistros = Productos.Count > 0;
    }, "No se pudo eliminar el producto");

    private async Task<(List<Categoria> Categorias, List<Proveedor> Proveedores)> CargarCombosAsync()
    {
        var categorias = await _categoriaRepositorio.ListarAsync();
        var proveedores = await _proveedorRepositorio.ListarAsync();
        return (categorias, proveedores);
    }
}
