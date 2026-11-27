using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
    /// Interaction logic for ListaPagos.xaml
    /// </summary>
    public partial class ListaPagosGenerados : Page
    {
        private const decimal IVA = 0.16m;
        private DispatcherTimer _timer;
        private bool _isFetching = false;

        public ListaPagosGenerados()
        {
            InitializeComponent();
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (sender, e) => CargarListaPagos();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListaPagos();
            _timer.Start();

            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Órdenes pendientes de pago";
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void CargarListaPagos()
        {
            if (_isFetching) return;

            _isFetching = true;

            try
            {
                List<Pago> pagos = PagoDAO.ObtenerPagosPendientes();
                icPagos.ItemsSource = pagos;
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

        private void BtnAceptarPago_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.Tag is Pago pagoSeleccionado))
            {
                return;
            }

            _timer.Stop();

            try
            {
                decimal total = pagoSeleccionado.ventaTotal;
                decimal subtotal = Math.Round(total / (1m + IVA), 2, MidpointRounding.AwayFromZero);

                string nombreMesero = pagoSeleccionado.Orden?.Empleado != null
                    ? $"{pagoSeleccionado.Orden.Empleado.nombres} {pagoSeleccionado.Orden.Empleado.apellidoPaterno} {pagoSeleccionado.Orden.Empleado.apellidoMaterno}".Trim()
                    : "N/A";

                string numeroMesa = pagoSeleccionado.Orden?.Mesa?.numero.ToString() ?? "N/A";
                string metodoPago = pagoSeleccionado.TipoPago?.tipoPago ?? "N/A";

                string mensajeConfirmacion =
                    "Se aceptará el pago. ¿Son correctos los datos?\n\n" +
                    $"Subtotal: ${subtotal.ToString("0.00", CultureInfo.CurrentCulture)}\n" +
                    $"Total: ${total.ToString("0.00", CultureInfo.CurrentCulture)}\n" +
                    $"Mesero: {nombreMesero}\n" +
                    $"Mesa: {numeroMesa}\n" +
                    $"Método de pago: {metodoPago}";

                MessageBoxResult confirmacion = MessageBox.Show(
                    mensajeConfirmacion,
                    "Aceptar pago",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion != MessageBoxResult.Yes)
                {
                    return;
                }

                int idOrden = pagoSeleccionado.idOrden;
                int? idMesa = pagoSeleccionado.Orden?.idMesa;

                if (!idMesa.HasValue)
                {
                    MessageBox.Show("Error al liberar la mesa.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var scope = new TransactionScope())
                {
                    bool exitoPago = PagoDAO.ActualizarEstadoPago(pagoSeleccionado.idPago, DateTime.Now, "Pagado");
                    if (!exitoPago)
                    {
                        throw new PagoException("Error al aceptar el pago.");
                    }

                    bool exitoOrden = OrdenDAO.ActualizarEstadoOrden(idOrden, 5);
                    if (!exitoOrden)
                    {
                        throw new OrdenException("Error al finalizar la orden.");
                    }

                    bool exitoMesa = MesaDAO.ActualizarDisponibilidadMesa(idMesa.Value, "Disponible");
                    if (!exitoMesa)
                    {
                        throw new MesaException("Error al liberar la mesa.");
                    }

                    scope.Complete();
                }

                MessageBox.Show(
                    "Se ha aceptado el pago. Orden finalizada.",
                    "Pago aceptado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CargarListaPagos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (IsLoaded)
                {
                    _timer.Start();
                }
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }
    }
}
