using System.Windows;
using System.Windows.Controls;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class PedidosView : UserControl
{
    private PedidosViewModel? _viewModel;

    public PedidosView()
    {
        InitializeComponent();
        Loaded += (_, _) => Suscribir();
        Unloaded += (_, _) => Desuscribir();
    }

    private void Suscribir()
    {
        Desuscribir();
        _viewModel = DataContext as PedidosViewModel;
        if (_viewModel is not null)
        {
            _viewModel.EdicionSolicitada += OnEdicionSolicitada;
            _viewModel.EdicionDetalleSolicitada += OnEdicionDetalleSolicitada;
        }
    }

    private void Desuscribir()
    {
        if (_viewModel is not null)
        {
            _viewModel.EdicionSolicitada -= OnEdicionSolicitada;
            _viewModel.EdicionDetalleSolicitada -= OnEdicionDetalleSolicitada;
            _viewModel = null;
        }
    }

    private async void OnEdicionSolicitada(PedidoEditViewModel edicion)
    {
        var dialogo = new PedidoEditWindow(edicion) { Owner = Window.GetWindow(this) };
        if (dialogo.ShowDialog() == true && _viewModel is not null)
        {
            await _viewModel.GuardarCommand.ExecuteAsync(edicion);
        }
    }

    private async void OnEdicionDetalleSolicitada(DetalleEditViewModel edicion)
    {
        var dialogo = new DetalleEditWindow(edicion) { Owner = Window.GetWindow(this) };
        if (dialogo.ShowDialog() == true && _viewModel is not null)
        {
            await _viewModel.GuardarDetalleCommand.ExecuteAsync(edicion);
        }
    }
}
