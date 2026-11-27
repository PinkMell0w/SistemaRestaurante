using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
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

namespace ClienteSistemaRestaurante.Vistas.Mesero
{
    /// <summary>
    /// Interaction logic for ListaDetallesOrden.xaml
    /// </summary>
    public partial class ListaDetallesOrden : Page
    {
        Orden _ordenActual;
        Empleado _meseroActual;
        public ListaDetallesOrden(Orden ordenActual, Empleado meseroActual)
        {
            InitializeComponent();
            _ordenActual = ordenActual;
            _meseroActual = meseroActual;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = $"Orden #{_ordenActual.idOrden} - Mesa {_ordenActual.Mesa.numero}";
                txtDetalleOrden.Text = $"Orden #{_ordenActual.idOrden} - Mesa {_ordenActual.Mesa.numero}";
            }

            CargarListaDetallesOrden();
        }

        private void CargarListaDetallesOrden()
        {
            try
            {
                List<DetalleOrden> platillosOrden = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenActual.idOrden);
                icDetalles.ItemsSource = platillosOrden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void BtnAgregarPlatillos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                NavigationService?.Navigate(new FormularioOrden(true, _ordenActual, _meseroActual));
            }
        }
    }
}
