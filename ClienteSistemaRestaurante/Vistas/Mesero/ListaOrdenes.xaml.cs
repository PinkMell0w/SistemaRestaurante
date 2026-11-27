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

namespace ClienteSistemaRestaurante.Vistas.Mesero
{
    /// <summary>
    /// Interaction logic for ListaOrdenes.xaml
    /// </summary>
    public partial class ListaOrdenes : Page
    {
        private Empleado _meseroActual;
        private DispatcherTimer _timer;

        public ListaOrdenes(Empleado mesero)
        {
            InitializeComponent();
            _meseroActual = mesero;
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timer.Tick += (s, e) => ActualizarFechaHora();
        }

        private void ActualizarFechaHora()
        {
            runDia.Text = DateTime.Now.ToString("g");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Órdenes que atiendo";
            }

            ActualizarFechaHora();
            _timer.Start();

            CargarListaOrdenes();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private void CargarListaOrdenes()
        {
            try
            {
                List<Orden> ordenesMesero = OrdenDAO.ObtenerOrdenesActivas(_meseroActual.idEmpleado);
                icOrdenes.ItemsSource = ordenesMesero;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnActualizarOrden_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Orden ordenSeleccionada)
            {
                _timer.Stop();
                NavigationService?.Navigate(new ListaDetallesOrden(ordenSeleccionada, _meseroActual));
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }
    }
}
