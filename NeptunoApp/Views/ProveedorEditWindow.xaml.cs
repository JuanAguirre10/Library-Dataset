using System.Windows;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class ProveedorEditWindow : Window
{
    public ProveedorEditWindow(ProveedorEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => CompaniaBox.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (ProveedorEditViewModel)DataContext;
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
