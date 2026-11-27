using ClienteSistemaRestaurante.Datos;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario.Mesas
{
    /// <summary>
    /// Interaction logic for FormularioMesa.xaml
    /// </summary>
    public partial class FormularioMesa : Page
    {
        private bool _esModificar;
        private Mesa _mesaActual;
        
        public FormularioMesa() : this(false, null) { }

        public FormularioMesa(bool esModificar, Mesa mesaActual)
        {
            InitializeComponent();
            _esModificar = esModificar;
            _mesaActual = mesaActual;
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
                ventana.Title = "Agregar mesa";
                txtTitulo.Text = "Agregar mesa";
                btnGuardar.Content = "Registrar";
            }
            else
            {
                ventana.Title = "Modificar mesa";
                txtTitulo.Text = "Modificar mesa";
                btnGuardar.Content = "Actualizar";

                if (_mesaActual != null)
                {
                    txtNumero.Text = _mesaActual.numero.ToString();
                    txtNumero.IsReadOnly = true;
                    txtNumero.Foreground = Brushes.Black;

                    txtCapacidad.Text = _mesaActual.capacidad.ToString();
                    txtCapacidad.Foreground = Brushes.Black;
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
                    case "txtNumero":
                        textBox.Text = "Ej.: 1";
                        break;
                    case "txtCapacidad":
                        textBox.Text = "Ej.: 4";
                        break;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string numeroString = txtNumero.Text.Trim();
            string capacidadString = txtCapacidad.Text.Trim();

            if (!ValidarDatos(numeroString, capacidadString))
            {
                return;
            }

            int numero = int.Parse(numeroString);
            int capacidad = int.Parse(capacidadString);

            string tituloVentana = _esModificar ? "Confirmar actualización" : "Confirmar registro";
            string textoGuardar = _esModificar ? "Verifique que los datos sean correctos. ¿Desea continuar?" : "¿Está seguro de que desea confirmar el registro?";

            string mensajeConfirmacion =
                textoGuardar + "\n\n" +
                "Datos de la mesa:\n" +
                $"Número: {numero}\n" +
                $"Capacidad: {capacidad}";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeConfirmacion,
                tituloVentana,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                try
                {
                    if (!_esModificar)
                    {
                        RegistrarMesa(numero, capacidad);
                    }
                    else
                    {
                        ModificarMesa(capacidad);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            string mensajeCancelar = _esModificar
                ? "No se actualizará la mesa. ¿Está seguro?"
                : "No se registrará la mesa. ¿Está seguro?";

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

        private bool ValidarDatos(string numero, string capacidad)
        {
            txtNumeroBlock.Foreground = Brushes.Black;
            txtCapacidadBlock.Foreground = Brushes.Black;

            txtNumeroRestriccion.Foreground = Brushes.Black;
            txtNumeroRestriccion.FontWeight = FontWeights.Normal;

            txtCapacidadRestriccion.Foreground = Brushes.Black;
            txtCapacidadRestriccion.FontWeight = FontWeights.Normal;

            bool esValido = true;
            Regex soloEnterosRegex = new Regex(@"^\d+$");

            bool numeroVacioOPlaceholder = string.IsNullOrWhiteSpace(numero) || numero == "Ej.: 1";
            if (numeroVacioOPlaceholder)
            {
                txtNumeroBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            else if (!soloEnterosRegex.IsMatch(numero) || !int.TryParse(numero, out int numeroMesa) || numeroMesa <= 0)
            {
                ResaltarRestriccion(txtNumeroRestriccion);
                esValido = false;
            }

            bool capacidadVaciaOPlaceholder = string.IsNullOrWhiteSpace(capacidad) || capacidad == "Ej.: 4";
            if (capacidadVaciaOPlaceholder)
            {
                txtCapacidadBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            else if (!soloEnterosRegex.IsMatch(capacidad) || !int.TryParse(capacidad, out int capacidadMesa) || capacidadMesa < 1 || capacidadMesa > 8)
            {
                ResaltarRestriccion(txtCapacidadRestriccion);
                esValido = false;
            }

            return esValido;
        }

        private void ResaltarRestriccion(TextBlock txtRestriccion)
        {
            txtRestriccion.Foreground = Brushes.Red;
            txtRestriccion.FontWeight = FontWeights.Bold;
        }

        private void RegistrarMesa(int numero, int capacidad)
        {
            var mesaExistente = MesaDAO.ConsultarMesa(numero);
            if (mesaExistente != null)
            {
                MessageBox.Show("Esta mesa ya existe.", "Mesa duplicada", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Mesa nuevaMesa = new Mesa
            {
                numero = numero,
                capacidad = capacidad,
                estado = "Disponible"
            };

            try
            {
                bool exito = MesaDAO.InsertarMesa(nuevaMesa);
                if (exito)
                {
                    string mensajeExito =
                    "Se ha registrado a la mesa con éxito.\n\n" +
                    "Datos de la mesa:\n" +
                    $"Número: {numero}\n" +
                    $"Capacidad: {capacidad}";
                    MessageBox.Show(mensajeExito, "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModificarMesa(int capacidad)
        {
            try
            {
                bool exito = MesaDAO.ActualizarMesa(_mesaActual.idMesa, capacidad);
                if (exito)
                {
                    string mensajeExito =
                    "Se ha actualizado la mesa con éxito.\n\n" +
                    "Datos de la mesa:\n" +
                    $"Número: {_mesaActual.numero}\n" +
                    $"Capacidad: {capacidad}";
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
