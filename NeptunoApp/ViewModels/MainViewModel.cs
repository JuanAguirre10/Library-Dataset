using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

/// <summary>
/// ViewModel de la ventana principal: conserva los modulos y decide cual se
/// muestra en el area de contenido.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public ProductosViewModel Productos { get; }
    public CategoriasViewModel Categorias { get; }
    public ProveedoresViewModel Proveedores { get; }
    public PedidosViewModel Pedidos { get; }
    public ReportesViewModel Reportes { get; }

    [ObservableProperty]
    private Seccion seccionActual;

    [ObservableProperty]
    private ObservableObject? contenidoActual;

    public MainViewModel(
        ProductosViewModel productos,
        CategoriasViewModel categorias,
        ProveedoresViewModel proveedores,
        PedidosViewModel pedidos,
        ReportesViewModel reportes)
    {
        Productos = productos;
        Categorias = categorias;
        Proveedores = proveedores;
        Pedidos = pedidos;
        Reportes = reportes;

        // Se llama directamente porque asignar SeccionActual con el mismo valor
        // que ya tiene el campo no dispararia OnSeccionActualChanged.
        MostrarSeccion(SeccionActual);
    }

    partial void OnSeccionActualChanged(Seccion value) => MostrarSeccion(value);

    private void MostrarSeccion(Seccion seccion)
    {
        ContenidoActual = seccion switch
        {
            Seccion.Productos => Productos,
            Seccion.Categorias => Categorias,
            Seccion.Proveedores => Proveedores,
            Seccion.Pedidos => Pedidos,
            Seccion.Reportes => Reportes,
            _ => Productos
        };

        // Los listados se piden al entrar al modulo; el reporte espera a que el
        // usuario elija el intervalo de fechas.
        switch (seccion)
        {
            case Seccion.Productos:
                _ = Productos.CargarCommand.ExecuteAsync(null);
                break;
            case Seccion.Categorias:
                _ = Categorias.CargarCommand.ExecuteAsync(null);
                break;
            case Seccion.Proveedores:
                _ = Proveedores.CargarCommand.ExecuteAsync(null);
                break;
            case Seccion.Pedidos:
                _ = Pedidos.CargarCommand.ExecuteAsync(null);
                break;
        }
    }
}
