using System.Windows;
using System.Windows.Controls;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class CategoriasView : UserControl
{
    private CategoriasViewModel? _viewModel;

    public CategoriasView()
    {
        InitializeComponent();

        // La vista se vuelve a crear cada vez que se entra al modulo, por eso la
        // suscripcion se ata al ciclo de vida del control y no al constructor.
        Loaded += (_, _) => Suscribir();
        Unloaded += (_, _) => Desuscribir();
    }

    private void Suscribir()
    {
        Desuscribir();
        _viewModel = DataContext as CategoriasViewModel;
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

    private async void OnEdicionSolicitada(CategoriaEditViewModel edicion)
    {
        var dialogo = new CategoriaEditWindow(edicion) { Owner = Window.GetWindow(this) };
        if (dialogo.ShowDialog() == true && _viewModel is not null)
        {
            await _viewModel.GuardarCommand.ExecuteAsync(edicion);
        }
    }
}
