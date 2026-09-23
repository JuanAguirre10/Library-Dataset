using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeptunoApp.Data;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class CategoriasViewModel : ViewModelBase
{
    private readonly ICategoriaRepository _repositorio;

    public ObservableCollection<Categoria> Categorias { get; } = new();

    [ObservableProperty]
    private Categoria? seleccionada;

    [ObservableProperty]
    private bool hayRegistros;

    /// <summary>La vista es la responsable de mostrar el dialogo de edicion.</summary>
    public event Action<CategoriaEditViewModel>? EdicionSolicitada;

    public CategoriasViewModel(ICategoriaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    [RelayCommand]
    private Task CargarAsync() => EjecutarAsync(async () =>
    {
        var resultado = await _repositorio.ListarAsync();

        Categorias.Clear();
        foreach (var categoria in resultado)
        {
            Categorias.Add(categoria);
        }
        HayRegistros = Categorias.Count > 0;
    }, "No se pudo cargar el listado de categorías");

    [RelayCommand]
    private void Nueva() => EdicionSolicitada?.Invoke(new CategoriaEditViewModel());

    [RelayCommand]
    private Task EditarAsync(Categoria? categoria) => EjecutarAsync(async () =>
    {
        if (categoria is null) return;

        var actual = await _repositorio.ObtenerPorIdAsync(categoria.CategoriaID);
        if (actual is null)
        {
            // El refresco limpia ErrorMessage, por eso el aviso se asigna despues.
            await CargarAsync();
            ErrorMessage = "La categoría ya no existe.";
            return;
        }

        EdicionSolicitada?.Invoke(new CategoriaEditViewModel(actual));
    }, "No se pudo abrir la categoría");

    [RelayCommand]
    private Task GuardarAsync(CategoriaEditViewModel? edicion) => EjecutarAsync(async () =>
    {
        if (edicion is null) return;

        var categoria = edicion.AModelo();
        if (edicion.EsNueva)
        {
            await _repositorio.CrearAsync(categoria);
        }
        else
        {
            await _repositorio.ActualizarAsync(categoria);
        }

        await CargarAsync();
    }, "No se pudo guardar la categoría");

    [RelayCommand]
    private Task EliminarAsync(Categoria? categoria) => EjecutarAsync(async () =>
    {
        if (categoria is null) return;
        if (!Confirmar($"¿Eliminar la categoría \"{categoria.NombreCategoria}\"?\n\nEs una eliminación lógica: el registro se marca como inactivo (Activo = 0), deja de aparecer en los listados y se conserva en la base de datos.")) return;

        await _repositorio.EliminarAsync(categoria.CategoriaID);
        Categorias.Remove(categoria);
        HayRegistros = Categorias.Count > 0;
    }, "No se pudo eliminar la categoría");
}
