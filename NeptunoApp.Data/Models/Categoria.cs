using CommunityToolkit.Mvvm.ComponentModel;

namespace NeptunoApp.Models;

public partial class Categoria : ObservableObject
{
    [ObservableProperty]
    private int categoriaID;

    [ObservableProperty]
    private string nombreCategoria = string.Empty;

    [ObservableProperty]
    private string? descripcion;

    /// <summary>Estado para la eliminacion logica: false cuando el registro fue dado de baja.</summary>
    [ObservableProperty]
    private bool activo = true;
}
