using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeptunoApp.Data;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class ProveedoresViewModel : ViewModelBase
{
    private readonly IProveedorRepository _repositorio;

    public ObservableCollection<Proveedor> Proveedores { get; } = new();

    [ObservableProperty]
    private Proveedor? seleccionado;

    [ObservableProperty]
    private bool hayRegistros;

    // Filtros de busqueda pedidos por el laboratorio.
    [ObservableProperty]
    private string? filtroNombreContacto;

    [ObservableProperty]
    private string? filtroCiudad;

    [ObservableProperty]
    private bool filtrosAplicados;

    public event Action<ProveedorEditViewModel>? EdicionSolicitada;

    public ProveedoresViewModel(IProveedorRepository repositorio)
    {
        _repositorio = repositorio;
    }

    [RelayCommand]
    private Task CargarAsync() => EjecutarAsync(async () =>
    {
        // Se usa siempre el procedimiento de busqueda: con los filtros vacios
        // devuelve el listado completo.
        var resultado = await _repositorio.BuscarAsync(FiltroNombreContacto, FiltroCiudad);

        Proveedores.Clear();
        foreach (var proveedor in resultado)
        {
            Proveedores.Add(proveedor);
        }
        HayRegistros = Proveedores.Count > 0;
        FiltrosAplicados = !string.IsNullOrWhiteSpace(FiltroNombreContacto)
                           || !string.IsNullOrWhiteSpace(FiltroCiudad);
    }, "No se pudo cargar el listado de proveedores");

    [RelayCommand]
    private Task BuscarAsync() => CargarAsync();

    [RelayCommand]
    private Task LimpiarFiltrosAsync()
    {
        FiltroNombreContacto = null;
        FiltroCiudad = null;
        return CargarAsync();
    }

    [RelayCommand]
    private void Nuevo() => EdicionSolicitada?.Invoke(new ProveedorEditViewModel());

    [RelayCommand]
    private Task EditarAsync(Proveedor? proveedor) => EjecutarAsync(async () =>
    {
        if (proveedor is null) return;

        var actual = await _repositorio.ObtenerPorIdAsync(proveedor.ProveedorID);
        if (actual is null)
        {
            await CargarAsync();
            ErrorMessage = "El proveedor ya no existe.";
            return;
        }

        EdicionSolicitada?.Invoke(new ProveedorEditViewModel(actual));
    }, "No se pudo abrir el proveedor");

    [RelayCommand]
    private Task GuardarAsync(ProveedorEditViewModel? edicion) => EjecutarAsync(async () =>
    {
        if (edicion is null) return;

        var proveedor = edicion.AModelo();
        if (edicion.EsNuevo)
        {
            await _repositorio.CrearAsync(proveedor);
        }
        else
        {
            await _repositorio.ActualizarAsync(proveedor);
        }

        await CargarAsync();
    }, "No se pudo guardar el proveedor");

    [RelayCommand]
    private Task EliminarAsync(Proveedor? proveedor) => EjecutarAsync(async () =>
    {
        if (proveedor is null) return;
        if (!Confirmar($"¿Eliminar el proveedor \"{proveedor.CompaniaNombre}\"?\n\nEs una eliminación lógica: el registro se marca como inactivo (Activo = 0), deja de aparecer en los listados y se conserva en la base de datos.")) return;

        await _repositorio.EliminarAsync(proveedor.ProveedorID);
        Proveedores.Remove(proveedor);
        HayRegistros = Proveedores.Count > 0;
    }, "No se pudo eliminar el proveedor");
}
