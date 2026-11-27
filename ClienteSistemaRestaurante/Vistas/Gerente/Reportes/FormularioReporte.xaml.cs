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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Reportes
{
    /// <summary>
    /// Interaction logic for FormularioReporte.xaml
    /// </summary>
    public partial class FormularioReporte : Page
    {
        private bool _actualizandoFechas;

        public FormularioReporte()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Generar reporte";
            }

            MostrarCalendario();
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

        private void MostrarCalendario()
        {
            string tipo = ObtenerTipoSeleccionado();
            bool tieneTipoValido = !string.IsNullOrEmpty(tipo);

            dpFechaInicio.IsEnabled = tieneTipoValido;

            if (!tieneTipoValido)
            {
                _actualizandoFechas = true;
                try
                {
                    dpFechaInicio.SelectedDate = null;
                    dpFechaFin.SelectedDate = null;
                }
                finally
                {
                    _actualizandoFechas = false;
                }
            }
        }

        private bool ComputarFecha(string tipo, DateTime fechaBase, out DateTime fechaInicio, out DateTime fechaFin)
        {
            DateTime dia = fechaBase.Date;

            switch (tipo)
            {
                case "Diario":
                    fechaInicio = dia;
                    fechaFin = dia;
                    return true;

                case "Semanal":
                    fechaInicio = dia.AddDays(-(int)dia.DayOfWeek);
                    fechaFin = fechaInicio.AddDays(6);
                    return true;

                case "Mensual":
                    fechaInicio = new DateTime(dia.Year, dia.Month, 1);
                    int ultimoDia = DateTime.DaysInMonth(dia.Year, dia.Month);
                    fechaFin = new DateTime(dia.Year, dia.Month, ultimoDia);
                    return true;

                default:
                    fechaInicio = DateTime.MinValue;
                    fechaFin = DateTime.MinValue;
                    return false;
            }
        }

        private bool ValidarFecha(string tipo, DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin.Date > DateTime.Today)
            {
                MessageBox.Show(
                    "No se puede elaborar un reporte que contenga fechas futuras.",
                    "Fechas inválidas",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _actualizandoFechas)
            {
                return;
            }

            MostrarCalendario();

            string tipo = ObtenerTipoSeleccionado();
            if (!string.IsNullOrEmpty(tipo) && dpFechaInicio.SelectedDate.HasValue)
            {
                ProcesarFechaSeleccionada(tipo, dpFechaInicio.SelectedDate.Value);
            }
        }

        private void DpFechaInicio_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _actualizandoFechas)
            {
                return;
            }

            string tipo = ObtenerTipoSeleccionado();
            if (string.IsNullOrEmpty(tipo) || !dpFechaInicio.SelectedDate.HasValue)
            {
                return;
            }

            ProcesarFechaSeleccionada(tipo, dpFechaInicio.SelectedDate.Value);
        }

        private void ProcesarFechaSeleccionada(string tipo, DateTime fechaBase)
        {
            if (!ComputarFecha(tipo, fechaBase, out DateTime fechaInicio, out DateTime fechaFin))
            {
                return;
            }

            if (!ValidarFecha(tipo, fechaInicio, fechaFin))
            {
                DateTime fechaRespaldo = tipo == "Mensual"
                    ? DateTime.Today.AddMonths(-1)
                    : DateTime.Today;

                ComputarFecha(tipo, fechaRespaldo, out fechaInicio, out fechaFin);
            }

            _actualizandoFechas = true;
            try
            {
                dpFechaInicio.SelectedDate = fechaInicio;
                dpFechaFin.SelectedDate = fechaFin;
            }
            finally
            {
                _actualizandoFechas = false;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "No se generará el reporte. ¿Está seguro?",
                "Advertencia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.GoBack();
            }
        }

        private void BtnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            string tipo = ObtenerTipoSeleccionado();
            if (string.IsNullOrEmpty(tipo))
            {
                MessageBox.Show("No se seleccionó un tipo de reporte. Verifique la selección e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
            {
                MessageBox.Show("No se seleccionó una fecha de inicio. Verifique la selección e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime inicioDisplay = dpFechaInicio.SelectedDate.Value.Date;
            DateTime finDisplay = dpFechaFin.SelectedDate.Value.Date;

            if (!ValidarFecha(tipo, inicioDisplay, finDisplay))
            {
                ProcesarFechaSeleccionada(tipo, inicioDisplay);
                return;
            }

            string mensajeConfirmacion =
                "¿Son correctos los datos?\n\n" +
                "Datos del reporte:\n" +
                $"Tipo: {tipo}\n" +
                $"Fecha de inicio: {inicioDisplay.ToString("dd/MM/yyyy")}\n" +
                $"Fecha de fin: {finDisplay.ToString("dd/MM/yyyy")}";

            MessageBoxResult confirmacion = MessageBox.Show(mensajeConfirmacion, "Confirmar generación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                DateTime fechaInicioBd = inicioDisplay;
                DateTime fechaFinBd = finDisplay.AddDays(1).AddTicks(-1);

                Reporte nuevoReporte = new Reporte
                {
                    tipo = tipo,
                    fechaInicio = fechaInicioBd,
                    fechaFin = fechaFinBd
                };

                bool exito = ReporteDAO.InsertarReporte(nuevoReporte);
                if (!exito)
                {
                    throw new ReporteException("Error de conexión al generar el reporte.");
                }

                MessageBox.Show("Se ha generado el reporte correctamente.", "Reporte generado", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new FormularioHistoricoReportes(nuevoReporte));
            }
            catch (ReporteException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
