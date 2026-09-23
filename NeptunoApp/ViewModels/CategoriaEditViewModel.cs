using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class CategoriaEditViewModel : ObservableObject
{
    public int CategoriaID { get; }

    [ObservableProperty]
    private string nombreCategoria;

    [ObservableProperty]
    private string? descripcion;

    public bool EsNueva => CategoriaID == 0;
    public string TituloVentana => EsNueva ? "Nueva categoría" : "Editar categoría";

    public CategoriaEditViewModel(Categoria? categoria = null)
    {
        CategoriaID = categoria?.CategoriaID ?? 0;
        nombreCategoria = categoria?.NombreCategoria ?? string.Empty;
        descripcion = categoria?.Descripcion;
    }

    /// <summary>Devuelve null si los datos son validos, o el motivo del rechazo.</summary>
    public string? Validar()
    {
        if (string.IsNullOrWhiteSpace(NombreCategoria))
            return "El nombre de la categoría es obligatorio.";

        if (NombreCategoria.Trim().Length > 30)
            return "El nombre no puede superar los 30 caracteres.";

        return null;
    }

    public Categoria AModelo() => new()
    {
        CategoriaID = CategoriaID,
        NombreCategoria = NombreCategoria.Trim(),
        Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim()
    };
}
