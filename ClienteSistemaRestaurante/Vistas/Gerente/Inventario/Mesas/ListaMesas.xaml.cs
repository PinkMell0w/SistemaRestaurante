using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario.Mesas
{
    /// <summary>
    /// Interaction logic for ListaMesas.xaml
    /// </summary>
    public partial class ListaMesas : Page
    {
        public ListaMesas()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Consultar mesas";
            }

            CargarListaMesas();
        }

        private void CargarListaMesas()
        {
            try
            {
                List<Mesa> mesas = MesaDAO.ObtenerMesasInventario();
                icMesas.ItemsSource = mesas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BtnModificarMesa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Mesa mesaSeleccionada)
            {
                NavigationService?.Navigate(new FormularioMesa(true, mesaSeleccionada));
            }
        }

        private void BtnEliminarMesa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Mesa mesaSeleccionada)
            {
                MessageBoxResult result = MessageBox.Show($"¿Está seguro de que desea eliminar esta mesa? Esta acción no se puede deshacer.\n\n" +
                    "Datos de la mesa:\n" +
                    $"Número: {mesaSeleccionada.numero}\n" +
                    $"Capacidad: {mesaSeleccionada.capacidad}", "Eliminar mesa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool eliminado = MesaDAO.EliminarMesa(mesaSeleccionada.idMesa);
                        if (eliminado)
                        {
                            MessageBox.Show("Se ha eliminado la mesa con éxito.", "Mesa eliminada");
                            CargarListaMesas();
                        }
                    }
                    catch (MesaEnUsoException mEx)
                    {
                        MessageBox.Show(mEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    public class CapacidadToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int capacidad)
            {
                return capacidad == 1 ? "1 persona" : $"{capacidad} personas";
            }
            return "0 personas";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
