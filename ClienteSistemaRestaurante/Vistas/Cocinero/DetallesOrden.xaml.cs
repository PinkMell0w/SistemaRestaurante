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
    /// Interaction logic for DetallesOrden.xaml
    /// </summary>
    public partial class DetallesOrden : Page
    {
        private Orden _ordenSeleccionada;
        private DispatcherTimer _timer;
        public DetallesOrden(Orden ordenSeleccionada)
        {
            InitializeComponent();
            _ordenSeleccionada = ordenSeleccionada;
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += (sender, e) => CargarDetallesCocina();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = $"Orden #{_ordenSeleccionada.idOrden}";
                txtOrden.Text = $"Orden #{_ordenSeleccionada.idOrden}";
            }

            CargarDetallesCocina();
            _timer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void CargarDetallesCocina()
        {
            try
            {
                List<DetalleOrden> detalles = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenSeleccionada.idOrden);

                var listaViewModel = detalles.Select(d => {
                    string estado = d.estado ?? "En espera";
                    string textoBoton = "Preparar";
                    bool habilitado = true;

                    if (estado == "En espera")
                    {
                        textoBoton = "Preparar";
                    }
                    else if (estado == "Preparando")
                    {
                        textoBoton = "Servir";
                    }
                    else if (estado == "Listo")
                    {
                        textoBoton = "Servido";
                        habilitado = false;
                    }

                    return new DetalleOrdenItemViewModel
                    {
                        DetalleOriginal = d,
                        Platillo = d.Platillo,
                        Cantidad = d.cantidadPlatillo,
                        Estado = d.estado,
                        TextoBoton = textoBoton,
                        EsBotonHabilitado = habilitado
                    };
                }).ToList();

                icDetallesPlatillos.ItemsSource = listaViewModel;

                bool todosListos = listaViewModel.All(item => item.Estado == "Listo");
                btnServirOrden.IsEnabled = todosListos && listaViewModel.Count > 0;
                btnServirOrden.Foreground = todosListos && listaViewModel.Count > 0 ? Brushes.White : Brushes.Gray;
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
            }
        }

        private void BtnAccionPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DetalleOrdenItemViewModel vm)
            {
                try
                {
                    string estadoActual = vm.Estado;
                    string nuevoEstado = string.Empty;

                    if (estadoActual == "En espera")
                    {
                        nuevoEstado = "Preparando";
                    }
                    else if (estadoActual == "Preparando")
                    {
                        nuevoEstado = "Listo";
                    }

                    bool exito = DetalleOrdenDAO.ActualizarEstadoPlatillo(vm.DetalleOriginal.idDetalleOrden, nuevoEstado);
                    if (exito)
                    {
                        List<DetalleOrden> detallesActualizados = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenSeleccionada.idOrden);

                        if (nuevoEstado == "Preparando")
                        {
                            OrdenDAO.ActualizarEstadoOrden(_ordenSeleccionada.idOrden, 2);
                        }
                        else if (nuevoEstado == "Listo")
                        {
                            bool todosListos = detallesActualizados.All(d => d.estado == "Listo");
                            if (todosListos)
                            {
                                OrdenDAO.ActualizarEstadoOrden(_ordenSeleccionada.idOrden, 3);
                            }
                        }

                        CargarDetallesCocina();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }

        private void BtnServirOrden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _timer.Stop();

                bool exito = OrdenDAO.ActualizarEstadoOrden(_ordenSeleccionada.idOrden, 4); // 4 = Servida
                if (exito)
                {
                    NavigationService?.GoBack();
                }
            }
            catch (Exception)
            {
                _timer.Start();
                MessageBox.Show("Error al servir la orden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class DetalleOrdenItemViewModel
    {
        public DetalleOrden DetalleOriginal { get; set; }
        public Platillo Platillo { get; set; }
        public int Cantidad { get; set; }
        public string Estado { get; set; }
        public string TextoBoton { get; set; }
        public bool EsBotonHabilitado { get; set; }
    }
}
