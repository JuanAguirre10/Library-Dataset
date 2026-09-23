using System.Windows;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class PedidoEditWindow : Window
{
    public PedidoEditWindow(PedidoEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => ClienteCombo.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (PedidoEditViewModel)DataContext;
        var error = viewModel.Validar();
        if (error is not null)
        {
            MessageBox.Show(this, error, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
