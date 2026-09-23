using System.Windows;
using System.Windows.Controls;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class ProveedoresView : UserControl
{
    private ProveedoresViewModel? _viewModel;

    public ProveedoresView()
    {
        InitializeComponent();
        Loaded += (_, _) => Suscribir();
        Unloaded += (_, _) => Desuscribir();
    }

    private void Suscribir()
    {
        Desuscribir();
        _viewModel = DataContext as ProveedoresViewModel;
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

    private async void OnEdicionSolicitada(ProveedorEditViewModel edicion)
    {
        var dialogo = new ProveedorEditWindow(edicion) { Owner = Window.GetWindow(this) };
        if (dialogo.ShowDialog() == true && _viewModel is not null)
        {
            await _viewModel.GuardarCommand.ExecuteAsync(edicion);
        }
    }
}
