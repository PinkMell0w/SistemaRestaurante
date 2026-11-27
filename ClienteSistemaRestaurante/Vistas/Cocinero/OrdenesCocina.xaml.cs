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

namespace ClienteSistemaRestaurante.Vistas.Cocinero
{
    /// <summary>
    /// Interaction logic for OrdenesCocina.xaml
    /// </summary>
    public partial class OrdenesCocina : Page
    {
        private DispatcherTimer _timer;
        private bool _isFetching = false;

        public OrdenesCocina()
        {
            InitializeComponent();
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += (sender, e) => CargarOrdenesCocina();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarOrdenesCocina();
            _timer.Start();

            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Órdenes del día - " + DateTime.Now.ToString("g");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void CargarOrdenesCocina()
        {
            if (_isFetching) return;

            _isFetching = true;

            try
            {
                List<Orden> ordenesCocina = OrdenDAO.ObtenerOrdenesCocina();
                icOrdenesCocina.ItemsSource = ordenesCocina;
                txtFechaHoy.Text = DateTime.Now.ToString("g");
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _timer.Start();
            }
            finally
            {
                _isFetching = false;
            }
        }

        private void BtnAtender_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Orden ordenSeleccionada)
            {
                _timer.Stop();

                NavigationService?.Navigate(new DetallesOrden(ordenSeleccionada));
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                _timer.Stop();
                ventanaPadre.Close();
            }
        }
    }
}
