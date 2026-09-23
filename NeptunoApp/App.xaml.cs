using System.Windows;
using NeptunoApp.Configuracion;
using NeptunoApp.Data;
using NeptunoApp.ViewModels;

namespace NeptunoApp;

/// <summary>
/// Punto de composicion: crea los repositorios, los inyecta en los ViewModels
/// y abre la ventana principal.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Unico punto donde se resuelve la cadena: se lee de App.config y se
        // inyecta en los repositorios, que no saben de donde viene.
        var cadena = DbConfig.ConnectionString;

        ICategoriaRepository categoriaRepositorio = new CategoriaRepository(cadena);
        IProveedorRepository proveedorRepositorio = new ProveedorRepository(cadena);
        IProductoRepository productoRepositorio = new ProductoRepository(cadena);
        IPedidoRepository pedidoRepositorio = new PedidoRepository(cadena);
        ICatalogoRepository catalogoRepositorio = new CatalogoRepository(cadena);

        var principal = new MainViewModel(
            new ProductosViewModel(productoRepositorio, categoriaRepositorio, proveedorRepositorio),
            new CategoriasViewModel(categoriaRepositorio),
            new ProveedoresViewModel(proveedorRepositorio),
            new PedidosViewModel(pedidoRepositorio, catalogoRepositorio, productoRepositorio),
            new ReportesViewModel(pedidoRepositorio));

        var ventana = new MainWindow { DataContext = principal };
        ventana.Show();
    }
}
