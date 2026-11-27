using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using static System.Net.Mime.MediaTypeNames;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ClienteSistemaRestaurante.Vistas.Gerente.Reportes
{
    /// <summary>
    /// Interaction logic for FormularioHistoricoReportes.xaml
    /// </summary>
    public partial class FormularioHistoricoReportes : Page
    {
        private readonly Reporte _reporteInicial;
        private bool _cargandoInicial;
        private List<BarraGraficaItem> _ultimoDatosGrafica = new List<BarraGraficaItem>();
        private List<DesgloseVentaItem> _ultimoDesgloseMeseros = new List<DesgloseVentaItem>();
        private List<DesgloseVentaItem> _ultimoDesglosePlatillos = new List<DesgloseVentaItem>();
        private decimal _ultimoTotalVentas;

        public FormularioHistoricoReportes(Reporte reporteInicial = null)
        {
            InitializeComponent();
            _reporteInicial = reporteInicial;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _cargandoInicial = true;
            try
            {
                CargarCatalogos();

                if (_reporteInicial != null)
                {
                    AplicarReporteEnControles(_reporteInicial);
                    ConsultarReportes(usarAyerPorDefecto: false);
                }
            }
            finally
            {
                _cargandoInicial = false;
                Window ventanaPadre = Window.GetWindow(this);
                if (ventanaPadre != null)
                {
                    ventanaPadre.Title = "Ver histórico de reportes";
                }
            }
        }

        private void CargarCatalogos()
        {
            try
            {
                var empleadosMeseros = EmpleadoDAO.ObtenerEmpleadosConRol(estado: true, idRol: 2);
                var meseros = empleadosMeseros
                    .OrderBy(emp => emp.nombres)
                    .Select(emp => new FiltroItem
                    {
                        Id = emp.idEmpleado,
                        Nombre = emp.nombres + " " + emp.apellidoPaterno
                    })
                    .ToList();
                meseros.Insert(0, new FiltroItem { Id = 0, Nombre = "-- Seleccione uno --" });
                cbMesero.ItemsSource = meseros;
                cbMesero.SelectedIndex = 0;

                var listaPlatillos = PlatilloDAO.ObtenerTodos();
                var platillos = listaPlatillos
                    .OrderBy(p => p.nombre)
                    .Select(p => new FiltroItem
                    {
                        Id = p.idPlatillo,
                        Nombre = p.nombre
                    })
                    .ToList();
                platillos.Insert(0, new FiltroItem { Id = 0, Nombre = "-- Seleccione uno --" });
                cbPlatillo.ItemsSource = platillos;
                cbPlatillo.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Error de conexión al cargar los filtros.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarReporteEnControles(Reporte reporte)
        {
            foreach (ComboBoxItem item in cbTipo.Items)
            {
                if (string.Equals(item.Content?.ToString(), reporte.tipo, StringComparison.OrdinalIgnoreCase))
                {
                    cbTipo.SelectedItem = item;
                    break;
                }
            }

            dpFechaInicio.SelectedDate = reporte.fechaInicio.Date;
            dpFechaFin.SelectedDate = reporte.fechaFin.Date;
            cbMesero.SelectedIndex = 0;
            cbPlatillo.SelectedIndex = 0;
        }

        private void CbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _cargandoInicial)
            {
                return;
            }

            string tipo = ObtenerTipoSeleccionado();
            if (!string.IsNullOrEmpty(tipo) && dpFechaInicio.SelectedDate.HasValue)
            {
                CalcularRangoFin(tipo, dpFechaInicio.SelectedDate.Value);
            }
        }

        private void DpFechaInicio_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _cargandoInicial)
            {
                return;
            }

            string tipo = ObtenerTipoSeleccionado();
            if (!string.IsNullOrEmpty(tipo) && dpFechaInicio.SelectedDate.HasValue)
            {
                CalcularRangoFin(tipo, dpFechaInicio.SelectedDate.Value);
            }
        }

        private void CalcularRangoFin(string tipo, DateTime fechaInicio)
        {
            DateTime inicio = fechaInicio.Date;
            switch (tipo)
            {
                case "Diario":
                    dpFechaFin.SelectedDate = inicio;
                    break;
                case "Semanal":
                    DateTime domingo = inicio.AddDays(-(int)inicio.DayOfWeek);
                    dpFechaInicio.SelectedDate = domingo;
                    dpFechaFin.SelectedDate = domingo.AddDays(6);
                    break;
                case "Mensual":
                    DateTime inicioMes = new DateTime(inicio.Year, inicio.Month, 1);
                    int ultimoDia = DateTime.DaysInMonth(inicio.Year, inicio.Month);
                    dpFechaInicio.SelectedDate = inicioMes;
                    dpFechaFin.SelectedDate = new DateTime(inicio.Year, inicio.Month, ultimoDia);
                    break;
            }
        }

        private void BtnConsultarReportes_Click(object sender, RoutedEventArgs e)
        {
            bool sinFiltrosSeleccionados =
                string.IsNullOrEmpty(ObtenerTipoSeleccionado()) &&
                !dpFechaInicio.SelectedDate.HasValue &&
                !dpFechaFin.SelectedDate.HasValue;

            ConsultarReportes(usarAyerPorDefecto: sinFiltrosSeleccionados);
        }

        private void BtnGuardarReporte_Click(object sender, RoutedEventArgs e)
        {
            GuardarReporte();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReportesSubmenu());
        }

        private void ConsultarReportes(bool usarAyerPorDefecto)
        {
            string tipo;
            DateTime inicioDisplay;
            DateTime finDisplay;

            if (usarAyerPorDefecto)
            {
                DateTime ayer = DateTime.Today.AddDays(-1);
                tipo = "Diario";
                inicioDisplay = ayer;
                finDisplay = ayer;

                _cargandoInicial = true;
                try
                {
                    SeleccionarTipoEnCombo(tipo);
                    dpFechaInicio.SelectedDate = inicioDisplay;
                    dpFechaFin.SelectedDate = finDisplay;
                    cbMesero.SelectedIndex = 0;
                    cbPlatillo.SelectedIndex = 0;
                }
                finally
                {
                    _cargandoInicial = false;
                }
            }
            else
            {
                tipo = ObtenerTipoSeleccionado();
                if (string.IsNullOrEmpty(tipo) || !dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("Seleccione el tipo y el rango de fechas para consultar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                inicioDisplay = dpFechaInicio.SelectedDate.Value.Date;
                finDisplay = dpFechaFin.SelectedDate.Value.Date;
            }

            DateTime inicioBd = inicioDisplay;
            DateTime finExclusivoBd = finDisplay.AddDays(1);

            try
            {
                List<Reporte> reportesGuardados = ReporteDAO.ObtenerReportesDeTipo(tipo, inicioBd, finExclusivoBd);
                if (_reporteInicial == null && !usarAyerPorDefecto && reportesGuardados.Count > 0)
                {
                    Reporte ultimo = reportesGuardados.First();
                    inicioBd = ultimo.fechaInicio.Date;
                    finExclusivoBd = ultimo.fechaFin.Date;
                    inicioDisplay = inicioBd;
                    finDisplay = finExclusivoBd.AddDays(-1);
                }

                List<VentaLineaDto> lineas = ObtenerLineasVenta(inicioBd, finExclusivoBd);

                int idMesero = !usarAyerPorDefecto && cbMesero.SelectedValue is int idM ? idM : 0;
                int idPlatillo = !usarAyerPorDefecto && cbPlatillo.SelectedValue is int idP ? idP : 0;

                if (idMesero > 0)
                {
                    lineas = FiltrarPorEmpleado(idMesero, lineas);
                }

                if (idPlatillo > 0)
                {
                    lineas = FiltrarPorPlatillo(idPlatillo, lineas);
                }

                CalcularResumenesYGrafica(tipo, inicioDisplay, finDisplay, idMesero > 0, idPlatillo > 0, lineas);
            }
            catch (Exception)
            {
                MessageBox.Show("Error de conexión al consultar los reportes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<VentaLineaDto> ObtenerLineasVenta(DateTime inicioBd, DateTime finExclusivoBd)
        {
            List<Pago> pagosPagados = PagoDAO.ObtenerPagosPagadosPorRango(inicioBd, finExclusivoBd);

            return pagosPagados
                .Where(p => p.Orden != null && p.Orden.DetalleOrdens != null)
                .SelectMany(p =>
                {
                    Orden orden = p.Orden;
                    Empleado empleado = orden.Empleado;
                    string nombreMesero = empleado != null
                        ? $"{empleado.nombres} {empleado.apellidoPaterno}".Trim()
                        : "Sin asignar";

                    return orden.DetalleOrdens.Select(d =>
                    {
                        decimal precioUnitario = d.precioUnitario;

                        return new VentaLineaDto
                        {
                            Fecha = p.fechaPago ?? orden.fechaCreacion,
                            IdEmpleado = orden.idEmpleado,
                            NombreMesero = nombreMesero,
                            IdPlatillo = d.idPlatillo,
                            NombrePlatillo = d.Platillo != null ? d.Platillo.nombre : "Platillo",
                            Cantidad = d.cantidadPlatillo,
                            Subtotal = d.cantidadPlatillo * precioUnitario
                        };
                    });
                })
                .ToList();
        }

        private List<VentaLineaDto> FiltrarPorEmpleado(int idEmpleado, List<VentaLineaDto> origen)
        {
            if (idEmpleado <= 0)
            {
                return origen;
            }
            return origen.Where(x => x.IdEmpleado == idEmpleado).ToList();
        }

        private List<VentaLineaDto> FiltrarPorPlatillo(int idPlatillo, List<VentaLineaDto> origen)
        {
            if (idPlatillo <= 0)
            {
                return origen;
            }
            return origen.Where(x => x.IdPlatillo == idPlatillo).ToList();
        }

        private void CalcularResumenesYGrafica(
            string tipo,
            DateTime inicioDisplay,
            DateTime finDisplay,
            bool filtroMesero,
            bool filtroPlatillo,
            List<VentaLineaDto> lineas)
        {
            _ultimoTotalVentas = lineas.Sum(x => x.Subtotal);

            _ultimoDesgloseMeseros = lineas
                .GroupBy(x => x.NombreMesero)
                .Select(g => new DesgloseVentaItem
                {
                    Concepto = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad),
                    Total = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            _ultimoDesglosePlatillos = lineas
                .GroupBy(x => x.NombrePlatillo)
                .Select(g => new DesgloseVentaItem
                {
                    Concepto = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad),
                    Total = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            _ultimoDatosGrafica = AgruparParaGrafica(tipo, filtroMesero, filtroPlatillo, lineas);
            icBarras.ItemsSource = _ultimoDatosGrafica;

            txtTituloGrafica.Text = $"Ventas ({tipo}) - {inicioDisplay:dd/MM/yyyy} a {finDisplay:dd/MM/yyyy}";
            txtResumenTotal.Text = $"Total: {_ultimoTotalVentas.ToString("C2", new CultureInfo("es-MX"))}";
        }

        private List<BarraGraficaItem> AgruparParaGrafica(
            string tipo,
            bool filtroMesero,
            bool filtroPlatillo,
            List<VentaLineaDto> lineas)
        {
            IEnumerable<IGrouping<string, VentaLineaDto>> grupos;

            if (filtroPlatillo)
            {
                grupos = lineas.GroupBy(x => x.NombrePlatillo);
            }
            else if (filtroMesero)
            {
                grupos = lineas.GroupBy(x => x.NombreMesero);
            }
            else
            {
                switch (tipo)
                {
                    case "Diario":
                        grupos = lineas.GroupBy(x => $"{x.Fecha.Hour:00}:00");
                        break;
                    case "Semanal":
                        grupos = lineas
                            .OrderBy(x => x.Fecha.DayOfWeek)
                            .GroupBy(x => x.Fecha.ToString("ddd", new CultureInfo("es-MX")));
                        break;
                    case "Mensual":
                        grupos = lineas
                            .OrderBy(x => x.Fecha.Day)
                            .GroupBy(x => x.Fecha.Day.ToString("00"));
                        break;
                    default:
                        grupos = lineas.GroupBy(x => x.Fecha.ToString("dd/MM"));
                        break;
                }
            }

            var raw = grupos
                .Select(g => new
                {
                    Etiqueta = g.Key,
                    Total = g.Sum(x => x.Subtotal)
                })
                .ToList();

            decimal maxTotal = raw.Count > 0 ? raw.Max(r => r.Total) : 0m;
            const double maxAlturaBarra = 220.0;

            return raw.Select(r => new BarraGraficaItem
            {
                Etiqueta = r.Etiqueta,
                Valor = r.Total,
                ValorTexto = r.Total.ToString("C0", new CultureInfo("es-MX")),
                AlturaBarra = maxTotal > 0 ? Math.Max(4.0, (double)(r.Total / maxTotal) * maxAlturaBarra) : 0.0
            }).ToList();
        }

        private void GuardarReporte()
        {
            if (_ultimoDatosGrafica == null || !dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
            {
                MessageBox.Show("No se seleccionaron datos para guardar un reporte. Verifique la selección e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tipo = ObtenerTipoSeleccionado() ?? "Reporte";
            DateTime inicio = dpFechaInicio.SelectedDate.Value.Date;
            DateTime fin = dpFechaFin.SelectedDate.Value.Date;

            var dialog = new SaveFileDialog
            {
                Title = "Guardar reporte en PDF",
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Reporte_{tipo}_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                EscribirDocumentoPdf(dialog.FileName, tipo, inicio, fin);
                MessageBox.Show("El reporte PDF se guardó correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Ocurrió un error al generar el archivo PDF.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private byte[] CapturarGraficaComoPng()
        {
            graficaCapturable.UpdateLayout();
            int width = Math.Max(1, (int)graficaCapturable.ActualWidth);
            int height = Math.Max(1, (int)graficaCapturable.ActualHeight);

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(graficaCapturable);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        private void EscribirDocumentoPdf(string rutaArchivo, string tipo, DateTime inicio, DateTime fin)
        {
            var culturaMx = new CultureInfo("es-MX");
            var doc = new PdfDocument();
            PdfPage page = doc.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var fontTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSubtitulo = new XFont("Arial", 10, XFontStyle.Regular);
            var fontSeccion = new XFont("Arial", 12, XFontStyle.Bold);
            var fontTexto = new XFont("Arial", 10, XFontStyle.Regular);

            double y = 35;
            double margenIzq = 35;
            double anchoUtil = page.Width - 70;

            gfx.DrawString("REPORTE DE VENTAS", fontTitulo, XBrushes.Black, new XRect(margenIzq, y, anchoUtil, 24), XStringFormats.TopLeft);
            y += 26;
            gfx.DrawString($"Tipo: {tipo}   |   Periodo: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}   |   Total: {_ultimoTotalVentas.ToString("C2", culturaMx)}",
                fontSubtitulo, XBrushes.DimGray, new XRect(margenIzq, y, anchoUtil, 18), XStringFormats.TopLeft);
            y += 26;

            byte[] pngBytes = CapturarGraficaComoPng();
            using (var imgStream = new MemoryStream(pngBytes))
            {
                XImage imagenGrafica = XImage.FromStream(imgStream);
                double altoImg = 190;
                gfx.DrawImage(imagenGrafica, margenIzq, y, anchoUtil, altoImg);
                y += altoImg + 18;
            }

            gfx.DrawString("Desglose por mesero", fontSeccion, XBrushes.Black, margenIzq, y);
            y += 18;
            foreach (var item in _ultimoDesgloseMeseros)
            {
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 35;
                }
                gfx.DrawString($"{item.Concepto} ({item.Cantidad} ítems): {item.Total.ToString("C2", culturaMx)}", fontTexto, XBrushes.Black, margenIzq, y);
                y += 15;
            }

            y += 12;

            if (y > page.Height - 60)
            {
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 35;
            }
            gfx.DrawString("Desglose por platillo", fontSeccion, XBrushes.Black, margenIzq, y);
            y += 18;
            foreach (var item in _ultimoDesglosePlatillos)
            {
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 35;
                }
                gfx.DrawString($"{item.Concepto} (x{item.Cantidad}): {item.Total.ToString("C2", culturaMx)}", fontTexto, XBrushes.Black, margenIzq, y);
                y += 15;
            }

            doc.Save(rutaArchivo);
        }

        private string ObtenerTipoSeleccionado()
        {
            if (cbTipo.SelectedItem is ComboBoxItem item)
            {
                string contenido = item.Content?.ToString();
                if (!string.IsNullOrWhiteSpace(contenido) && contenido != "-- Seleccione uno --")
                {
                    return contenido;
                }
            }
            return null;
        }

        private void SeleccionarTipoEnCombo(string tipo)
        {
            foreach (ComboBoxItem item in cbTipo.Items)
            {
                if (string.Equals(item.Content?.ToString(), tipo, StringComparison.OrdinalIgnoreCase))
                {
                    cbTipo.SelectedItem = item;
                    return;
                }
            }
        }

        private class FiltroItem
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        private class VentaLineaDto
        {
            public DateTime Fecha { get; set; }
            public int IdEmpleado { get; set; }
            public string NombreMesero { get; set; }
            public int IdPlatillo { get; set; }
            public string NombrePlatillo { get; set; }
            public int Cantidad { get; set; }
            public decimal Subtotal { get; set; }
        }

        private class BarraGraficaItem
        {
            public string Etiqueta { get; set; }
            public decimal Valor { get; set; }
            public string ValorTexto { get; set; }
            public double AlturaBarra { get; set; }
        }

        private class DesgloseVentaItem
        {
            public string Concepto { get; set; }
            public int Cantidad { get; set; }
            public decimal Total { get; set; }
        }
    }
}
