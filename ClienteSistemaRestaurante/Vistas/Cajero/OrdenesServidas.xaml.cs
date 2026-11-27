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
using System.Windows.Threading;

namespace ClienteSistemaRestaurante.Vistas.Cajero
{
    /// <summary>
    /// Interaction logic for OrdenesServidas.xaml
    /// </summary>
    public partial class OrdenesServidas : Page
    {
        private DispatcherTimer _timer;

        public OrdenesServidas()
        {
            InitializeComponent();
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (sender, e) => CargarListaOrdenes();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListaOrdenes();
            _timer.Start();

            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Órdenes servidas - " + DateTime.Now.ToString("g");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void CargarListaOrdenes()
        {
            try
            {
                List<Orden> ordenes = OrdenDAO.ObtenerOrdenesServidas();
                icOrdenes.ItemsSource = ordenes;
                txtFechaHoy.Text = DateTime.Now.ToString("g");
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _timer.Start();
            }
        }

        private void BtnGenerarTotal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Orden ordenSeleccionada)
            {
                _timer.Stop();
                NavigationService?.Navigate(new ResumenOrden(ordenSeleccionada));
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }
    }
}
