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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario.Ingredientes
{
    /// <summary>
    /// Interaction logic for FormularioIngrediente.xaml
    /// </summary>
    public partial class FormularioIngrediente : Page
    {
        private bool _esModificar;
        private Ingrediente _ingredienteActual;

        public FormularioIngrediente() : this(false, null) { }

        public FormularioIngrediente(bool esModificar, Ingrediente ingredienteActual)
        {
            InitializeComponent();
            _esModificar = esModificar;
            _ingredienteActual = ingredienteActual;
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
                ventana.Title = "Agregar ingrediente";
                txtTitulo.Text = "Agregar ingrediente";
                btnGuardar.Content = "Registrar";
            }
            else
            {
                ventana.Title = "Modificar ingrediente";
                txtTitulo.Text = "Modificar ingrediente";
                btnGuardar.Content = "Actualizar";

                if (_ingredienteActual != null)
                {
                    txtNombre.Text = _ingredienteActual.nombre;
                    txtNombre.Foreground = Brushes.Black;

                    cbMedida.SelectedItem = cbMedida.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == _ingredienteActual.Medida.nombreMedida);

                    string nombreUnidad = _ingredienteActual.Medida != null ? _ingredienteActual.Medida.nombreMedida : string.Empty;
                    int cantidad = ConversorUnidades.ConvertirDeBase(_ingredienteActual.cantidad, nombreUnidad);
                    txtCantidad.Text = cantidad.ToString();
                    txtCantidad.Foreground = Brushes.Black;
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
                    case "txtNombre":
                        textBox.Text = "Ej.: Tomates";
                        break;
                    case "txtCantidad":
                        textBox.Text = "Ej.: 10";
                        break;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string cantidadString = txtCantidad.Text.Trim();
            string medida = string.Empty;
            if (cbMedida.SelectedItem is ComboBoxItem selectedItem)
            {
                medida = selectedItem.Content?.ToString();
            }
            else if (cbMedida.SelectedItem != null)
            {
                medida = cbMedida.SelectedItem.ToString();
            }

            if (!ValidarDatos(nombre, cantidadString, medida))
            {
                return;
            }

            int cantidadIngresada = int.Parse(cantidadString);

            int cantidad = ConversorUnidades.ConvertirABase(cantidadIngresada, medida);

            string tituloVentana = _esModificar ? "Confirmar actualización" : "Confirmar registro";
            string textoGuardar = _esModificar ? "Verifique que los datos sean correctos. ¿Desea continuar?" : "¿Está seguro de que desea confirmar el registro?";

            string mensajeConfirmacion =
                textoGuardar + "\n\n" +
                "Datos del ingrediente:\n" +
                $"Nombre: {nombre}\n" +
                $"Cantidad: {cantidadIngresada}\n" +
                $"Medida: {medida}";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeConfirmacion,
                tituloVentana,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                int idMedida = 1;
                if (medida == "Unidades")
                {
                    idMedida = 1;
                }
                else if (medida == "Tazas")
                {
                    idMedida = 2;
                }
                else if (medida == "Gramos")
                {
                    idMedida = 3;
                }
                else if (medida == "Cucharadas")
                {
                    idMedida = 4;
                }
                else if (medida == "Cucharaditas")
                {
                    idMedida = 5;
                }
                else if (medida == "Onzas")
                {
                    idMedida = 6;
                }
                else if (medida == "Litros")
                {
                    idMedida = 7;
                }
                else if (medida == "Mililitros")
                {
                    idMedida = 8;
                }

                try
                {
                    if (!_esModificar)
                    {
                        RegistrarIngrediente(nombre, cantidad, idMedida, cantidadIngresada, medida);
                    }
                    else
                    {
                        ModificarIngrediente(nombre, cantidad, idMedida, cantidadIngresada, medida);
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
                ? "No se actualizará el ingrediente. ¿Está seguro?"
                : "No se registrará el ingrediente. ¿Está seguro?";

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

        private bool ValidarDatos(string nombre, string cantidad, string medida)
        {
            txtNombreBlock.Foreground = Brushes.Black;
            txtCantidadBlock.Foreground = Brushes.Black;
            txtMedidaBlock.Foreground = Brushes.Black;

            txtNombreRestriccion.Foreground = Brushes.Black;
            txtNombreRestriccion.FontWeight = FontWeights.Normal;

            txtCantidadRestriccion.Foreground = Brushes.Black;
            txtCantidadRestriccion.FontWeight = FontWeights.Normal;

            bool esValido = true;

            Regex soloLetrasRegex = new Regex(@"^[\p{L}]+(?: [\p{L}]+)*$");
            bool nombreVacioOPlaceholder = string.IsNullOrWhiteSpace(nombre) || nombre == "Ej.: Tomates";
            if (nombreVacioOPlaceholder)
            {
                txtNombreBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!nombreVacioOPlaceholder && (nombre.Length > 25 || !soloLetrasRegex.IsMatch(nombre)))
            {
                ResaltarRestriccion(txtNombreRestriccion);
                esValido = false;
            }

            Regex soloEnterosRegex = new Regex(@"^\d+$");
            bool cantidadVaciaOPlaceholder = string.IsNullOrWhiteSpace(cantidad) || cantidad == "Ej.: 10" || cantidad == "Ej.: 500";
            if (cantidadVaciaOPlaceholder)
            {
                txtCantidadBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!cantidadVaciaOPlaceholder && (!soloEnterosRegex.IsMatch(cantidad) || !int.TryParse(cantidad, out int cantidadInt) || cantidadInt <= 0))
            {
                ResaltarRestriccion(txtCantidadRestriccion);
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(medida) || medida == "Ej.: gramos")
            {
                txtMedidaBlock.Foreground = Brushes.Red;
                esValido = false;
            }

            return esValido;
        }

        private void ResaltarRestriccion(TextBlock txtRestriccion)
        {
            txtRestriccion.Foreground = Brushes.Red;
            txtRestriccion.FontWeight = FontWeights.Bold;
        }

        private void RegistrarIngrediente(string nombre, int cantidad, int idMedida, int cantidadIngresada, string medida)
        {
            var ingredienteExistente = IngredienteDAO.ConsultarIngrediente(nombre);
            if (ingredienteExistente != null)
            {
                MessageBox.Show("Este ingrediente ya existe.", "Ingrediente duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Ingrediente nuevoIngrediente = new Ingrediente
            {
                nombre = nombre,
                cantidad = cantidad,
                idMedida = idMedida
            };

            try
            {
                bool exito = IngredienteDAO.InsertarIngrediente(nuevoIngrediente);
                if (exito)
                {
                    string mensajeExito =
                    "Se ha registrado al ingrediente con éxito.\n\n" +
                    "Datos del ingrediente:\n" +
                    $"Nombre: {nombre}\n" +
                    $"Cantidad: {cantidadIngresada}\n" +
                    $"Medida: {medida}";
                    MessageBox.Show(mensajeExito, "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModificarIngrediente(string nombre, int cantidad, int idMedida, int cantidadIngresada, string medida)
        {
            if (_ingredienteActual == null)
            {
                MessageBox.Show("No se pudo obtener el ingrediente actual para modificar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _ingredienteActual.nombre = nombre;
            _ingredienteActual.cantidad = cantidad;
            _ingredienteActual.idMedida = idMedida;
            try
            {
                bool exito = IngredienteDAO.ModificarIngrediente(_ingredienteActual.idIngrediente, nombre, cantidad, idMedida);
                if (exito)
                {
                    string mensajeExito =
                    "Se ha actualizado al ingrediente con éxito.\n\n" +
                    "Nuevos datos del ingrediente:\n" +
                    $"Nombre: {nombre}\n" +
                    $"Cantidad: {cantidadIngresada}\n" +
                    $"Medida: {medida}";
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
