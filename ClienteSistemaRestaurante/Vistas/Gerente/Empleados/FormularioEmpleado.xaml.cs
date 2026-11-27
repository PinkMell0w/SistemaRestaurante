using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.LogicaNegocio;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Empleados
{
    /// <summary>
    /// Interaction logic for FormularioEmpleado.xaml
    /// </summary>
    public partial class FormularioEmpleado : Page
    {
        private bool _esModificar;
        private Empleado _empleadoActual;

        public FormularioEmpleado() : this(false, null) { }

        public FormularioEmpleado(bool esModificar, Empleado empleado)
        {
            InitializeComponent();
            _esModificar = esModificar;
            _empleadoActual = empleado;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            Window ventana = Window.GetWindow(this);

            if (!_esModificar)
            {
                ventana.Title = "Registrar nuevo empleado";
                txtTitulo.Text = "Registrar nuevo empleado";
                btnGuardar.Content = "Registrar";
            }
            else
            {
                ventana.Title = "Modificar empleado";
                txtTitulo.Text = "Modificar empleado";
                btnGuardar.Content = "Actualizar";

                if (_empleadoActual != null)
                {
                    txtNombres.Text = _empleadoActual.nombres;
                    txtNombres.Foreground = Brushes.Black;

                    txtApellidoPaterno.Text = _empleadoActual.apellidoPaterno;
                    txtApellidoPaterno.Foreground = Brushes.Black;

                    txtApellidoMaterno.Text = _empleadoActual.apellidoMaterno;
                    txtApellidoMaterno.Foreground = Brushes.Black;

                    txtRFC.Text = _empleadoActual.rfc;
                    txtRFC.Foreground = Brushes.Black;
                    txtRFC.IsEnabled = false;

                    foreach (ComboBoxItem item in cbRol.Items)
                    {
                        if (item.Content.ToString() == _empleadoActual.Rol.nombreRol)
                        {
                            cbRol.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        private void LimpiarPlaceholder_Focus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }

        private void RestaurarPlaceholder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                switch (textBox.Name)
                {
                    case "txtNombres":
                        textBox.Text = "Ej.: Roberto Carlos";
                        break;
                    case "txtApellidoPaterno":
                        textBox.Text = "Ej.: Pérez";
                        break;
                    case "txtApellidoMaterno":
                        textBox.Text = "Ej.: Martínez";
                        break;
                    case "txtRFC":
                        textBox.Text = "Ej.: PEMR980623HJ9";
                        break;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombres = txtNombres.Text.Trim();
            string apellidoPaterno = txtApellidoPaterno.Text.Trim();
            string apellidoMaterno = txtApellidoMaterno.Text.Trim();
            string rfc = txtRFC.Text.Trim().ToUpper();
            string rol = string.Empty;
            if (cbRol.SelectedItem is ComboBoxItem selectedItem)
            {
                rol = selectedItem.Content?.ToString();
            }
            else if (cbRol.SelectedItem != null)
            {
                rol = cbRol.SelectedItem.ToString();
            }

            if (!ValidarDatos(nombres, apellidoPaterno, apellidoMaterno, rfc, rol))
            {
                return;
            }

            string nombreCompleto = $"{nombres} {apellidoPaterno} {apellidoMaterno}";
            string tituloVentana = _esModificar ? "Confirmar actualización" : "Confirmar registro";
            string textoGuardar = _esModificar ? "Verifique que los datos sean correctos. ¿Desea continuar?" : "¿Está seguro de que desea confirmar el registro?";
            string textoDatosEmpleado = _esModificar ? "Datos del empleado:" : "Datos del empleado por registrar:";

            string mensajeConfirmacion =
                textoGuardar + "\n\n" +
                $"{textoDatosEmpleado}\n" +
                $"Nombre: {nombreCompleto}\n" +
                $"RFC: {rfc}\n" +
                $"Rol: {rol}";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeConfirmacion,
                tituloVentana,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                int idRol = 1;
                if (rol == "Cajero")
                {
                    idRol = 1;
                }
                else if (rol == "Mesero")
                {
                    idRol = 2;
                }
                else if (rol == "Cocinero")
                {
                    idRol = 3;
                }

                if (!_esModificar)
                {
                    RegistrarEmpleado(nombres, apellidoPaterno, apellidoMaterno, rfc, idRol, rol);
                }
                else
                {
                    ModificarEmpleado(nombres, apellidoPaterno, apellidoMaterno, rfc, idRol, rol);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            string mensajeCancelar = _esModificar
                ? "No se actualizará el empleado. ¿Está seguro?"
                : "No se registrará el empleado. ¿Está seguro?";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeCancelar,
                "Advertencia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                NavigationService?.GoBack();
            }
        }

        private bool ValidarDatos(string nombres, string apellidoPaterno, string apellidoMaterno, string rfc, string rol)
        {
            txtNombreBlock.Foreground = Brushes.Black;
            txtApellidoPaternoBlock.Foreground = Brushes.Black;
            txtRFCBlock.Foreground = Brushes.Black;
            txtRolBlock.Foreground = Brushes.Black;

            txtNombreRestriccion.Foreground = Brushes.Black;
            txtNombreRestriccion.FontWeight = FontWeights.Normal;

            txtApellidoPatRestriccion.Foreground = Brushes.Black;
            txtApellidoPatRestriccion.FontWeight = FontWeights.Normal;

            txtApellidoMatRestriccion.Foreground = Brushes.Black;
            txtApellidoMatRestriccion.FontWeight = FontWeights.Normal;

            txtRfcRestriccion.Foreground = Brushes.Black;
            txtRfcRestriccion.FontWeight = FontWeights.Normal;

            bool esValido = true;

            Regex soloLetrasRegex = new Regex(@"^[\p{L}]+(?: [\p{L}]+)*$");

            bool nombresVacioOPlaceholder = string.IsNullOrWhiteSpace(nombres) || nombres == "Ej.: Roberto Carlos";
            if (nombresVacioOPlaceholder)
            {
                txtNombreBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!nombresVacioOPlaceholder && (nombres.Length > 45 || !soloLetrasRegex.IsMatch(nombres)))
            {
                ResaltarRestriccion(txtNombreRestriccion);
                esValido = false;
            }

            bool apellidoPatVacioOPlaceholder = string.IsNullOrWhiteSpace(apellidoPaterno) || apellidoPaterno == "Ej.: Pérez";
            if (apellidoPatVacioOPlaceholder)
            {
                txtApellidoPaternoBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!apellidoPatVacioOPlaceholder && (apellidoPaterno.Length > 45 || !soloLetrasRegex.IsMatch(apellidoPaterno)))
            {
                ResaltarRestriccion(txtApellidoPatRestriccion);
                esValido = false;
            }

            bool apellidoMatTieneValor = !string.IsNullOrWhiteSpace(apellidoMaterno) && apellidoMaterno != "Ej.: Martínez";
            if (apellidoMatTieneValor && (apellidoMaterno.Length > 45 || !soloLetrasRegex.IsMatch(apellidoMaterno)))
            {
                ResaltarRestriccion(txtApellidoMatRestriccion);
                esValido = false;
            }

            Regex rfcCaracteresRegex = new Regex(@"^[A-ZÑ0-9]{1,13}$");
            Regex rfcFormatoRegex = new Regex(@"^[A-Z&Ñ]{4}\d{6}[A-Z0-9]{3}$");
            bool rfcVacioOPlaceholder = string.IsNullOrWhiteSpace(rfc) || rfc.ToUpper().Contains("EJ.:");
            if (rfcVacioOPlaceholder)
            {
                txtRFCBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            else if (rfc.Length > 13 || !rfcCaracteresRegex.IsMatch(rfc) || !rfcFormatoRegex.IsMatch(rfc))
            {
                ResaltarRestriccion(txtRfcRestriccion);
                esValido = false;
            }

            if (string.IsNullOrEmpty(rol) || rol.Contains("-- Seleccione"))
            {
                txtRolBlock.Foreground = Brushes.Red;
                esValido = false;
            }

            return esValido;
        }

        private void ResaltarRestriccion(TextBlock txtRestriccion)
        {
            txtRestriccion.Foreground = Brushes.Red;
            txtRestriccion.FontWeight = FontWeights.Bold;
        }

        private void RegistrarEmpleado(string nombres, string apellidoPaterno, string apellidoMaterno, string rfc, int idRol, string rol)
        {
            try
            {
                var empleadoExistente = EmpleadoDAO.ConsultarEmpleadoPorRfc(rfc);
                if (empleadoExistente != null)
                {
                    MessageBox.Show("Este empleado ya existe.", "Empleado duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Empleado nuevoEmpleado = new Empleado
                {
                    nombres = nombres,
                    apellidoPaterno = apellidoPaterno,
                    apellidoMaterno = apellidoMaterno,
                    rfc = rfc,
                    idRol = idRol,
                    estado = true
                };

                CreadorUsuario creadorUsuario = new CreadorUsuario();
                bool exito = creadorUsuario.RegistrarEmpleadoConUsuario(nuevoEmpleado, rol);
                if (exito)
                {
                    string mensajeExito =
                        "Se ha registrado al empleado con éxito.\n\n" +
                        "Datos del empleado:\n" +
                        $"Nombre: {nombres} {apellidoPaterno} {apellidoMaterno}\n" +
                        $"RFC: {rfc}\n" +
                        $"Rol: {rol}";
                    MessageBox.Show(mensajeExito, "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModificarEmpleado(string nombres, string apellidoPaterno, string apellidoMaterno, string rfc, int idRol, string rol)
        {
            try
            {
                bool exito = EmpleadoDAO.ModificarEmpleado(_empleadoActual.idEmpleado, nombres, apellidoPaterno, apellidoMaterno, idRol);
                if (exito)
                {
                    string mensajeExito =
                        "Se ha actualizado al empleado con éxito.\n\n" +
                        "Nuevos datos del empleado:\n" +
                        $"Nombre: {nombres} {apellidoPaterno} {apellidoMaterno}\n" +
                        $"RFC: {rfc}\n" +
                        $"Rol: {rol}";
                    MessageBox.Show(mensajeExito, "Actualización exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
