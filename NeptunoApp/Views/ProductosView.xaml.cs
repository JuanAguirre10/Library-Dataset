using System.Windows;
using System.Windows.Controls;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class ProductosView : UserControl
{
    private ProductosViewModel? _viewModel;

    public ProductosView()
    {
        InitializeComponent();
        Loaded += (_, _) => Suscribir();
        Unloaded += (_, _) => Desuscribir();
    }

    private void Suscribir()
    {
        Desuscribir();
        _viewModel = DataContext as ProductosViewModel;
        if (_viewModel is not null)
        {
            _viewModel.EdicionSolicitada += OnEdicionSolicitada;
        }
    }

    private void Desuscribir()
    {
        if (_viewModel is not null)
        {
            _viewModel.EdicionSolicitada -= OnEdicionSolicitada;
            _viewModel = null;
        }
    }

    private async void OnEdicionSolicitada(ProductoEditViewModel edicion)
    {
        var dialogo = new ProductoEditWindow(edicion) { Owner = Window.GetWindow(this) };
        if (dialogo.ShowDialog() == true && _viewModel is not null)
        {
            await _viewModel.GuardarCommand.ExecuteAsync(edicion);
        }
    }
}
