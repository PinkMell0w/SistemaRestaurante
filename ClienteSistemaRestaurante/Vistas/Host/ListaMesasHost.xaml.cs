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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClienteSistemaRestaurante.Vistas.Host
{
    /// <summary>
    /// Interaction logic for ListaMesasHost.xaml
    /// </summary>
    public partial class ListaMesasHost : Window
    {
        private readonly Window _loginWindow;
        private DispatcherTimer _timerRefresh;
        private List<Mesa> _listaMesasDisponibles = new List<Mesa>();

        public ListaMesasHost(Window loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
            ConfigurarTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarMesas();
            _timerRefresh.Start();
        }

        private void ConfigurarTimer()
        {
            _timerRefresh = new DispatcherTimer();
            _timerRefresh.Interval = TimeSpan.FromSeconds(3);
            _timerRefresh.Tick += (s, args) => CargarMesas();
        }

        private void CargarMesas()
        {
            try
            {
                scrollMesas.Visibility = Visibility.Visible;
                txtSinMesas.Visibility = Visibility.Collapsed;
                txtError.Visibility = Visibility.Hidden;
                _listaMesasDisponibles = MesaDAO.ObtenerMesasDisponibles();

                string inputTexto = txtFiltroPersonas?.Text.Trim();
                if (int.TryParse(inputTexto, out int val))
                {
                    FiltrarMesas(val);
                }
                else
                {
                    MostrarMesas(_listaMesasDisponibles);
                }
            }
            catch (Exception)
            {
                scrollMesas.Visibility = Visibility.Collapsed;
                scrollMesas.Visibility = Visibility.Visible;
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void TxtFiltroPersonas_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputTexto = txtFiltroPersonas?.Text.Trim();

            if (string.IsNullOrEmpty(inputTexto) || !int.TryParse(inputTexto, out int cantidadPersonas))
            {
                MostrarMesas(_listaMesasDisponibles);
                return;
            }

            FiltrarMesas(cantidadPersonas);
        }

        private void FiltrarMesas(int cantidadPersonas)
        {
            if (cantidadPersonas < 1 || cantidadPersonas > 8)
            {
                MostrarMesas(_listaMesasDisponibles);
                return;
            }

            var mesasFiltradas = _listaMesasDisponibles.Where(m => m.capacidad >= cantidadPersonas).ToList();
            MostrarMesas(mesasFiltradas);
        }

        private void MostrarMesas(List<Mesa> mesasDisponibles)
        {
            if (!mesasDisponibles.Any())
            {
                scrollMesas.Visibility = Visibility.Collapsed;
                txtSinMesas.Visibility = Visibility.Visible;
            }
            else
            {
                scrollMesas.Visibility = Visibility.Visible;
                txtSinMesas.Visibility = Visibility.Collapsed;

                var grupos = mesasDisponibles
                    .GroupBy(m => m.capacidad)
                    .OrderBy(g => g.Key)
                    .Select(g => new GrupoMesaPorCapacidad
                    {
                        TituloGrupo = $"Mesas para {g.Key}",
                        Mesas = g.OrderBy(m => m.numero).ToList()
                    })
                    .ToList();

                icGruposMesas.ItemsSource = grupos;
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "¿Está seguro de que desea cerrar sesión y volver al login?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _loginWindow.Show();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

    public class GrupoMesaPorCapacidad
    {
        public string TituloGrupo { get; set; }
        public List<Mesa> Mesas { get; set; }
    }
}
