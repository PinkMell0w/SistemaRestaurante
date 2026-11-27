using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.LogicaNegocio;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Platillos
{
    /// <summary>
    /// Interaction logic for FormularioPlatillo.xaml
    /// </summary>
    public partial class FormularioPlatillo : Page
    {
        private bool _esModificar;
        private Platillo _platilloActual;
        private ObservableCollection<FilaIngrediente> _ingredientesSeleccionados;
        private List<Ingrediente> _ingredientes;
        private const int MAX_DESC_CHARS = 255;

        public FormularioPlatillo() : this(false, null) { }

        public FormularioPlatillo(bool esModificar, Platillo platilloActual)
        {
            InitializeComponent();
            _esModificar = esModificar;
            _platilloActual = platilloActual;
            _ingredientesSeleccionados = new ObservableCollection<FilaIngrediente>();
            icIngredientes.ItemsSource = _ingredientesSeleccionados;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarIngredientes();
            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            Window ventana = Window.GetWindow(this);

            if (!_esModificar)
            {
                ventana.Title = "Registrar platillo";
                txtTitulo.Text = "Registrar platillo";
                btnGuardarPlatillo.Content = "Registrar";
            }
            else
            {
                ventana.Title = "Modificar platillo";
                txtTitulo.Text = "Modificar platillo";
                btnGuardarPlatillo.Content = "Actualizar";

                if (_platilloActual != null)
                {
                    txtNombre.Text = _platilloActual.nombre;
                    txtNombre.Foreground = Brushes.Black;

                    txtCosto.Text = _platilloActual.costo.ToString();
                    txtCosto.Foreground = Brushes.Black;

                    txtDescripcion.Text = _platilloActual.descripcion;
                    txtDescripcion.Foreground = Brushes.Black;

                    foreach (ComboBoxItem item in cbCategoria.Items)
                    {
                        if (item.Content.ToString() == _platilloActual.Categoria.nombreCategoria)
                        {
                            cbCategoria.SelectedItem = item;
                            break;
                        }
                    }

                    foreach (ComboBoxItem item in cbTipo.Items)
                    {
                        if (item.Content.ToString() == _platilloActual.tipo)
                        {
                            cbTipo.SelectedItem = item;
                            break;
                        }
                    }

                    foreach (ComboBoxItem item in cbEstado.Items)
                    {
                        if (item.Content.ToString() == _platilloActual.EstadoPlatillo.nombreEstado)
                        {
                            cbEstado.SelectedItem = item;
                            break;
                        }
                    }

                    _ingredientesSeleccionados.Clear();
                    List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(_platilloActual.idPlatillo);

                    foreach (var receta in ingredientesReceta)
                    {
                        var ingredienteEnLista = _ingredientes.FirstOrDefault(i => i.idIngrediente == receta.idIngrediente);

                        string nombreMedida = ingredienteEnLista?.Medida.nombreMedida ?? "Unidades";
                        int cantidadDisplay = ConversorUnidades.ConvertirDeBase(receta.cantidadRequerida, nombreMedida);

                        _ingredientesSeleccionados.Add(new FilaIngrediente
                        {
                            ListaIngredientes = _ingredientes,
                            IngredienteSeleccionado = ingredienteEnLista,
                            Cantidad = cantidadDisplay
                        });
                    }

                    if (_ingredientesSeleccionados.Count == 0)
                    {
                        AgregarFilaIngrediente();
                    }
                }
            }
        }

        private void CargarIngredientes()
        {
            try
            {
                _ingredientes = IngredienteDAO.ObtenerTodos();

                if (!_esModificar && _ingredientes.Count == 0)
                {
                    AgregarFilaIngrediente();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AgregarFilaIngrediente()
        {
            _ingredientesSeleccionados.Add(new FilaIngrediente
            {
                ListaIngredientes = _ingredientes ?? new List<Ingrediente>()
            });
        }

        private void BtnAgregarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            AgregarFilaIngrediente();
        }

        private void BtnEliminarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FilaIngrediente item)
            {
                _ingredientesSeleccionados.Remove(item);
                if (_ingredientesSeleccionados.Count == 0)
                {
                    AgregarFilaIngrediente();
                }
            }
        }

        private void BtnGuardarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos(out string nombre, out int costo, out string descripcion, out string categoria, out string tipo, out string estado))
            {
                return;
            }

            string tituloVentana = _esModificar ? "Confirmar actualización" : "Confirmar registro";
            string textoGuardar = _esModificar ? "Verifique que los datos sean correctos. ¿Desea continuar?" : "¿Está seguro de que desea registrar este platillo?";
            string ingredientesResumen = string.Join(", ", _ingredientesSeleccionados.Where(i => i.IngredienteSeleccionado != null && i.Cantidad > 0).Select(i => $"{i.IngredienteSeleccionado.nombre} ({i.Cantidad} {i.UnidadMedida})"));
            string mensajeConfirmacion = $"{textoGuardar}\n\n" +
                "Datos del platillo:\n" +
                $"Nombre: {nombre}\n" +
                $"Costo: {costo:C}\n" +
                $"Descripción: {descripcion}\n" +
                $"Categoría: {categoria}\n" +
                $"Tipo: {tipo}\n" +
                $"Estado: {estado}\n" +
                $"Ingredientes: {ingredientesResumen}";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeConfirmacion,
                tituloVentana,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            int idCategoria = 1;
            if (categoria == "Alimento") idCategoria = 1;
            else if (categoria == "Bebida") idCategoria = 2; // Ajusta según tus IDs reales

            int idEstado = 1;
            if (estado == "Disponible") idEstado = 1;
            else if (estado == "No disponible") idEstado = 2;

            if (!_esModificar)
            {
                RegistrarPlatillo(nombre, costo, descripcion, categoria, tipo, idCategoria, idEstado, estado, ingredientesResumen);
            }
            else
            {
                ModificarPlatillo(nombre, costo, descripcion, categoria, tipo, idCategoria, idEstado, estado, ingredientesResumen);
            }
        }

        private bool ValidarDatos(out string nombre, out int costo, out string descripcion, out string categoria, out string tipo, out string estado)
        {
            nombre = txtNombre.Text.Trim();
            string costoStr = txtCosto.Text.Trim();
            descripcion = txtDescripcion.Text.Trim();

            ComboBoxItem catItem = cbCategoria.SelectedItem as ComboBoxItem;
            categoria = catItem != null ? catItem.Content.ToString() : string.Empty;

            ComboBoxItem tipoItem = cbTipo.SelectedItem as ComboBoxItem;
            tipo = tipoItem != null ? tipoItem.Content.ToString() : string.Empty;

            ComboBoxItem estItem = cbEstado.SelectedItem as ComboBoxItem;
            estado = estItem != null ? estItem.Content.ToString() : string.Empty;

            costo = 0;

            txtNombreBlock.Foreground = Brushes.Black;
            txtCostoBlock.Foreground = Brushes.Black;
            txtDescripcionBlock.Foreground = Brushes.Black;
            txtCategoriaBlock.Foreground = Brushes.Black;
            txtTipoBlock.Foreground = Brushes.Black;
            txtEstadoBlock.Foreground = Brushes.Black;

            txtNombreRestriccion.Foreground = Brushes.Black;
            txtNombreRestriccion.FontWeight = FontWeights.Normal;

            txtCostoRestriccion.Foreground = Brushes.Black;
            txtCostoRestriccion.FontWeight = FontWeights.Normal;

            bool esValido = true;

            Regex soloLetrasRegex = new Regex(@"^[\p{L}]+(?: [\p{L}]+)*$");
            bool nombreVacioOPlaceholder = string.IsNullOrWhiteSpace(nombre) || nombre.Contains("Ej.:");
            if (nombreVacioOPlaceholder)
            {
                txtNombreBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!nombreVacioOPlaceholder && (nombre.Length > 45 || !soloLetrasRegex.IsMatch(nombre)))
            {
                ResaltarRestriccion(txtNombreRestriccion);
                esValido = false;
            }

            Regex soloEnterosRegex = new Regex(@"^\d+$");
            bool costoVacioOPlaceholder = string.IsNullOrWhiteSpace(costoStr) || costoStr.Contains("Ej.:");
            if (costoVacioOPlaceholder)
            {
                txtCostoBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!costoVacioOPlaceholder && (!soloEnterosRegex.IsMatch(costoStr) || !int.TryParse(costoStr, out costo) || costo <= 0))
            {
                ResaltarRestriccion(txtCostoRestriccion);
                esValido = false;
            }

            bool descVaciaOPlaceholder = string.IsNullOrWhiteSpace(descripcion) || descripcion.Contains("Ej.:");
            if (descVaciaOPlaceholder)
            {
                txtDescripcionBlock.Foreground = Brushes.Red;
                esValido = false;
            }
            if (!descVaciaOPlaceholder && (descripcion.Length > MAX_DESC_CHARS || !soloLetrasRegex.IsMatch(descripcion)))
            {
                txtDescRestriccion.Text = $"0 caracteres restantes";
                txtDescRestriccion.Foreground = Brushes.Red;
                txtDescRestriccion.FontWeight = FontWeights.Bold;
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(categoria) || categoria.Contains("-- Seleccione"))
            {
                txtCategoriaBlock.Foreground = Brushes.Red;
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(tipo) || tipo.Contains("-- Seleccione"))
            {
                txtTipoBlock.Foreground = Brushes.Red;
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(estado) || estado.Contains("-- Seleccione"))
            {
                txtEstadoBlock.Foreground = Brushes.Red;
                esValido = false;
            }

            if (_ingredientesSeleccionados.Count == 0 || _ingredientesSeleccionados.Any(i => i.IngredienteSeleccionado == null || i.Cantidad <= 0))
            {
                MessageBox.Show("Debe seleccionar al menos un ingrediente válido con cantidad mayor a cero.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            return esValido;
        }

        private void ResaltarRestriccion(TextBlock txtRestriccion)
        {
            txtRestriccion.Foreground = Brushes.Red;
            txtRestriccion.FontWeight = FontWeights.Bold;
        }

        private void RegistrarPlatillo(string nombre, int costo, string descripcion, string categoria, string tipo, int idCategoria, int idEstado, string estado, string ingredientesResumen)
        {
            try
            {
                if (PlatilloDAO.ConsultarPlatillo(nombre))
                {
                    MessageBox.Show("El platillo ya existe.", "Platillo duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Platillo nuevoPlatillo = new Platillo
                {
                    nombre = nombre,
                    costo = costo,
                    descripcion = descripcion,
                    idCategoria = cbCategoria.SelectedIndex,
                    tipo = tipo,
                    idEstado = cbEstado.SelectedIndex
                };

                using (var scope = new TransactionScope())
                {
                    int idPlatilloGenerado = PlatilloDAO.InsertarPlatillo(nuevoPlatillo);

                    if (idPlatilloGenerado > 0)
                    {
                        foreach (var fila in _ingredientesSeleccionados)
                        {
                            if (fila.IngredienteSeleccionado != null && fila.Cantidad > 0)
                            {
                                string nombreMedida = fila.IngredienteSeleccionado.Medida.nombreMedida;
                                int cantidadEnBase = ConversorUnidades.ConvertirABase(fila.Cantidad, nombreMedida);

                                Receta nuevaReceta = new Receta
                                {
                                    idPlatillo = idPlatilloGenerado,
                                    idIngrediente = fila.IngredienteSeleccionado.idIngrediente,
                                    cantidadRequerida = cantidadEnBase
                                };
                                RecetaDAO.InsertarReceta(nuevaReceta);
                            }
                        }

                        scope.Complete();
                    }
                }

                string mensajeExito = $"El platillo se registró con éxito.\n\n" +
                    "Datos del platillo:\n" +
                    $"Nombre: {nombre}\n" +
                    $"Costo: {costo:C}\n" +
                    $"Descripción: {descripcion}\n" +
                    $"Categoría: {categoria}\n" +
                    $"Tipo: {tipo}\n" +
                    $"Estado: {estado}\n" +
                    $"Ingredientes: {ingredientesResumen}";

                MessageBox.Show(mensajeExito, "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModificarPlatillo(string nombre, int costo, string descripcion, string categoria, string tipo, int idCategoria, int idEstado, string estado, string ingredientesResumen)
        {
            try
            {
                using (var scope = new System.Transactions.TransactionScope())
                {
                    _platilloActual.nombre = nombre;
                    _platilloActual.costo = costo;
                    _platilloActual.descripcion = descripcion;
                    _platilloActual.idCategoria = idCategoria;
                    _platilloActual.tipo = tipo;
                    _platilloActual.idEstado = idEstado;

                    if (PlatilloDAO.ModificarPlatillo(_platilloActual.idPlatillo, nombre, costo, descripcion, _platilloActual.idCategoria, _platilloActual.tipo, _platilloActual.idEstado))
                    {
                        RecetaDAO.EliminarReceta(_platilloActual.idPlatillo);

                        foreach (var fila in _ingredientesSeleccionados)
                        {
                            if (fila.IngredienteSeleccionado != null && fila.Cantidad > 0)
                            {
                                string nombreMedida = fila.IngredienteSeleccionado.Medida.nombreMedida;
                                int cantidadEnBase = ConversorUnidades.ConvertirABase(fila.Cantidad, nombreMedida);

                                Receta nuevaReceta = new Receta
                                {
                                    idPlatillo = _platilloActual.idPlatillo,
                                    idIngrediente = fila.IngredienteSeleccionado.idIngrediente,
                                    cantidadRequerida = cantidadEnBase
                                };
                                RecetaDAO.InsertarReceta(nuevaReceta);
                            }
                        }

                        scope.Complete();
                    }
                }

                string mensajeExito = $"Se ha actualizado el platillo con éxito.\n\n" +
                    "Datos del platillo:\n" +
                    $"Nombre: {nombre}\n" +
                    $"Costo: {costo:C}\n" +
                    $"Descripción: {descripcion}\n" +
                    $"Categoría: {categoria}\n" +
                    $"Tipo: {tipo}\n" +
                    $"Estado: {estado}\n" +
                    $"Ingredientes: {ingredientesResumen}";

                MessageBox.Show(mensajeExito, "Actualización exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            string mensajeCancelar = _esModificar
                ? "No se actualizará el platillo. ¿Está seguro?"
                : "No se guardará el platillo. ¿Está seguro?";

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
                if (textBox == txtNombre)
                {
                    textBox.Text = "Ej.: Milanesa de pollo";
                }
                else if (textBox == txtCosto)
                {
                    textBox.Text = "Ej.: 150";
                }
                else if (textBox == txtDescripcion)
                {
                    textBox.Text = "Ej.: Milanesa de pollo con papas.";
                }
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void CbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipo == null) return;

            cbTipo.Items.Clear();
            cbTipo.IsEnabled = true;

            if (cbCategoria.SelectedItem is ComboBoxItem selectedItem)
            {
                string categoria = selectedItem.Content?.ToString();

                if (categoria == "Alimento")
                {
                    cbTipo.Items.Add(new ComboBoxItem { Content = "-- Seleccione uno --", IsSelected = true });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Entrada" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Plato fuerte" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Postre" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Desayuno" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Cena" });
                }
                else if (categoria == "Bebida")
                {
                    cbTipo.Items.Add(new ComboBoxItem { Content = "-- Seleccione uno --", IsSelected = true });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Bebida fría" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Bebida caliente" });
                    cbTipo.Items.Add(new ComboBoxItem { Content = "Digestivo" });
                }
                else
                {
                    cbTipo.Items.Add(new ComboBoxItem { Content = "-- Seleccione categoría --", IsSelected = true });
                    cbTipo.IsEnabled = false;
                }
            }
        }

        private void TxtDescripcion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtDescRestriccion == null || txtDescripcion == null)
            {
                return;
            }

            bool esPlaceholder = txtDescripcion.Foreground == Brushes.Gray;
            int longitudActual = esPlaceholder ? 0 : txtDescripcion.Text.Length;
            int restantes = Math.Max(0, MAX_DESC_CHARS - longitudActual);

            txtDescRestriccion.Text = $"{restantes} caracteres restantes";

            if (restantes <= 10)
            {
                txtDescRestriccion.Foreground = Brushes.Red;
                txtDescRestriccion.FontWeight = FontWeights.Bold;
            }
            else if (restantes <= 30)
            {
                txtDescRestriccion.Foreground = Brushes.DarkOrange;
                txtDescRestriccion.FontWeight = FontWeights.Bold;
            }
            else
            {
                txtDescRestriccion.Foreground = Brushes.Black;
                txtDescRestriccion.FontWeight = FontWeights.Normal;
            }
        }
    }

    public class FilaIngrediente : System.ComponentModel.INotifyPropertyChanged
    {
        private Ingrediente _ingredienteSeleccionado;
        private int _cantidad;
        public List<Ingrediente> ListaIngredientes { get; set; }
        public Ingrediente IngredienteSeleccionado
        {
            get => _ingredienteSeleccionado;
            set
            {
                if (_ingredienteSeleccionado != value)
                {
                    _ingredienteSeleccionado = value;
                    OnPropertyChanged(nameof(IngredienteSeleccionado));
                    OnPropertyChanged(nameof(UnidadMedida));
                }
            }
        }
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad != value)
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                }
            }
        }
        public string UnidadMedida => IngredienteSeleccionado?.Medida.nombreMedida ?? "Unidades";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
