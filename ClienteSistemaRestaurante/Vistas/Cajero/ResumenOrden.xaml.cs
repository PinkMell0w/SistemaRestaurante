using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.IO;
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
using Path = System.IO.Path;
using Microsoft.Win32;

namespace ClienteSistemaRestaurante.Vistas.Cajero
{
    /// <summary>
    /// Interaction logic for ResumenOrden.xaml
    /// </summary>
    public partial class ResumenOrden : Page
    {
        private Orden _ordenSeleccionada;
        private const decimal IVA = 0.16m;

        public ResumenOrden(Orden ordenSeleccionada)
        {
            InitializeComponent();
            _ordenSeleccionada = ordenSeleccionada;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = $"Orden #{_ordenSeleccionada.idOrden} - Desglose de consumo";
                txtDesgloseConsumo.Text = $"Orden #{_ordenSeleccionada.idOrden} - Desglose de consumo";
            }

            CargarDetallesOrden();
        }

        private void CargarDetallesOrden()
        {
            try
            {
                var detallesOrden = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenSeleccionada.idOrden);
                icDetalles.ItemsSource = detallesOrden;

                txtNombreMesero.Text = $"{_ordenSeleccionada.Empleado.nombres} {_ordenSeleccionada.Empleado.apellidoPaterno} {_ordenSeleccionada.Empleado.apellidoMaterno}".Trim();

                decimal subtotal = detallesOrden.Sum(d => (decimal?)d.subtotal) ?? 0m;
                decimal ivaOrden = subtotal * IVA;
                decimal total = subtotal + ivaOrden;

                txtSubtotal.Text = subtotal.ToString("0.00");
                txtIva.Text = ivaOrden.ToString("0.00");
                txtTotal.Text = total.ToString("0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarArchivoTicketTxt(string metodoPago, decimal subtotal, decimal iva, decimal ventaTotal)
        {
            DateTime fechaEmision = DateTime.Now;
            string nombreSugerido = $"Ticket_Orden_{_ordenSeleccionada.idOrden}_{fechaEmision:yyyyMMdd_HHmmss}.txt";

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Guardar ticket de compra",
                FileName = nombreSugerido,
                DefaultExt = ".txt",
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            bool? resultadoDialogo = dialog.ShowDialog();
            if (resultadoDialogo != true || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return null;
            }

            string numeroMesa = _ordenSeleccionada.Mesa?.numero.ToString() ?? "N/A";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("========================================");
            sb.AppendLine("           TICKET DE COMPRA");
            sb.AppendLine("========================================");
            sb.AppendLine($"Fecha y hora : {fechaEmision:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"No. Cliente  : {_ordenSeleccionada.idOrden}");
            sb.AppendLine($"Orden        : #{_ordenSeleccionada.idOrden}");
            sb.AppendLine($"Mesa         : {numeroMesa}");
            sb.AppendLine($"Mesero       : {txtNombreMesero.Text}");
            sb.AppendLine($"Método pago  : {metodoPago}");
            sb.AppendLine($"Estado pago  : Pendiente");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("DETALLE DE CONSUMO");
            sb.AppendLine("----------------------------------------");

            if (icDetalles.ItemsSource is IEnumerable<DetalleOrden> detalles)
            {
                foreach (var item in detalles)
                {
                    string nombrePlatillo = item.Platillo?.nombre ?? "Platillo";
                    decimal subItem = item.subtotal ?? (item.precioUnitario * item.cantidadPlatillo);
                    sb.AppendLine($"{item.cantidadPlatillo}x {nombrePlatillo}");
                    sb.AppendLine($"   P/U: ${item.precioUnitario.ToString("0.00", CultureInfo.CurrentCulture)}   Sub: ${subItem.ToString("0.00", CultureInfo.CurrentCulture)}");
                }
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Subtotal     : ${subtotal.ToString("0.00", CultureInfo.CurrentCulture)}");
            sb.AppendLine($"IVA (16%)    : ${iva.ToString("0.00", CultureInfo.CurrentCulture)}");
            sb.AppendLine($"TOTAL        : ${ventaTotal.ToString("0.00", CultureInfo.CurrentCulture)}");
            sb.AppendLine("========================================");
            sb.AppendLine("       ¡Gracias por su preferencia!");
            sb.AppendLine("========================================");

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            return dialog.FileName;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("No se generará el ticket. ¿Está seguro(a)?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.GoBack();
            }
        }

        private void BtnGenerarPago_Click(object sender, RoutedEventArgs e)
        {
            if (cbMetodoPago.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() != "-- Seleccione uno --")
            {
                int idTipoPago = 1;
                if (selectedItem.Content.ToString() == "Efectivo")
                {
                    idTipoPago = 1;
                }
                else if (selectedItem.Content.ToString() == "Transferencia")
                {
                    idTipoPago = 2;
                }
                else if (selectedItem.Content.ToString() == "Terminal")
                {
                    idTipoPago = 3;
                }

                string metodoPago = selectedItem.Content.ToString();

                if (!decimal.TryParse(txtTotal.Text, out decimal ventaTotal))
                {
                    MessageBox.Show("El monto total no tiene un formato válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string mensaje = $"Se generará el ticket. ¿Son correctos los datos?\n\n" +
                    $"Subtotal: ${txtSubtotal.Text}\n" +
                    $"Total: ${txtTotal.Text}\n" +
                    $"Mesero: {txtNombreMesero.Text}\n" +
                    $"Mesa: {_ordenSeleccionada.Mesa.numero}\n" +
                    "Método de pago: " + metodoPago;
                MessageBoxResult result = MessageBox.Show(mensaje, "Generar pago", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    string rutaTicket = GenerarArchivoTicketTxt(metodoPago, decimal.Parse(txtSubtotal.Text), decimal.Parse(txtIva.Text), ventaTotal);
                    if (string.IsNullOrWhiteSpace(rutaTicket))
                    {
                        return;
                    }

                    Pago nuevoPago = new Pago
                    {
                        ventaTotal = ventaTotal,
                        estado = "Pendiente",
                        idTipoPago = idTipoPago,
                        idOrden = _ordenSeleccionada.idOrden
                    };

                    try
                    {
                        bool exitoPago = PagoDAO.InsertarPago(nuevoPago);
                        if (!exitoPago)
                        {
                            throw new Exception("Error al generar el pago.");
                        }

                        MessageBox.Show("Se ha generado el ticket. Pendiente de pagar.", "Ticket generado", MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.GoBack();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No se seleccionó un método de pago. Por favor seleccione uno e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
