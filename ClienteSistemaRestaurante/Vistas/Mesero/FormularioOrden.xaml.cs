using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.LogicaNegocio;
using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ClienteSistemaRestaurante.Vistas.Mesero
{
    /// <summary>
    /// Interaction logic for FormularioOrden.xaml
    /// </summary>
    public partial class FormularioOrden : Page, INotifyPropertyChanged
    {
        private bool _esActualizar;
        private Orden _ordenActual;
        private Empleado _mesero;
        private List<PlatilloItemViewModel> _todosLosPlatillosVM = new List<PlatilloItemViewModel>();
        private DispatcherTimer _timer;

        private string _textoBuscar = string.Empty;
        public string TextoBuscar
        {
            get => _textoBuscar;
            set { _textoBuscar = value; OnPropertyChanged(nameof(TextoBuscar)); FiltrarCatalogo(); }
        }

        public FormularioOrden(bool esActualizar, Orden ordenActual, Empleado meseroActual)
        {
            InitializeComponent();
            DataContext = this;
            _esActualizar = esActualizar;
            _ordenActual = ordenActual;
            _mesero = meseroActual;
            ConfigurarTimer();
        }

        private void ConfigurarTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timer.Tick += (s, e) => ActualizarFechaHora();
        }

        private void ActualizarFechaHora()
        {
            runFechaHoy.Text = DateTime.Now.ToString("g");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurarInterfaz();
            CargarDatos();

            ActualizarFechaHora();
            _timer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private void ConfigurarInterfaz()
        {
            Window ventana = Window.GetWindow(this);

            if (!_esActualizar)
            {
                ventana.Title = "Nueva orden";
                txtOrden.Text = "Nueva orden";
                btnGuardar.Content = "Crear orden";
            }
            else
            {
                ventana.Title = $"Orden #{_ordenActual?.idOrden} - Mesa {_ordenActual?.Mesa?.numero}";
                txtOrden.Text = $"Actualizar Orden #{_ordenActual?.idOrden} - Mesa {_ordenActual?.Mesa?.numero}";
                btnGuardar.Content = "Actualizar";
                cbMesas.IsEnabled = false;
            }
        }

        private void CargarDatos()
        {
            try
            {
                List<Mesa> mesasDisponibles = MesaDAO.ObtenerMesasDisponibles();

                if (_esActualizar && _ordenActual?.Mesa != null)
                {
                    if (!mesasDisponibles.Any(m => m.idMesa == _ordenActual.idMesa))
                    {
                        mesasDisponibles.Add(_ordenActual.Mesa);
                    }
                    cbMesas.SelectedValue = _ordenActual.idMesa;
                }
                else if (mesasDisponibles == null || mesasDisponibles.Count() == 0)
                {
                    MessageBox.Show("No hay mesas disponibles por el momento.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }

                cbMesas.ItemsSource = mesasDisponibles;

                List<Platillo> platillosDb = PlatilloDAO.ObtenerPlatillosDisponibles();
                _todosLosPlatillosVM = platillosDb.Select(p => new PlatilloItemViewModel
                {
                    Platillo = p,
                    CantidadInput = "0",
                    TextoBoton = "Agregar",
                    EsHabilitado = true,
                    ColorBoton = new SolidColorBrush(Color.FromRgb(227, 109, 45)),
                    ColorTexto = Brushes.White
                }).ToList();

                if (_esActualizar && _ordenActual != null)
                {
                    List<DetalleOrden> detallesExistentes = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenActual.idOrden);
                    foreach (var detalle in detallesExistentes)
                    {
                        var vmMatch = _todosLosPlatillosVM.FirstOrDefault(vm => vm.Platillo.idPlatillo == detalle.idPlatillo);
                        if (vmMatch != null)
                        {
                            vmMatch.CantidadOriginal = detalle.cantidadPlatillo;
                            vmMatch.CantidadInput = detalle.cantidadPlatillo.ToString();
                            vmMatch.TextoBoton = "Agregado";
                            vmMatch.EsHabilitado = false;
                            vmMatch.ColorBoton = new SolidColorBrush(Color.FromRgb(153, 150, 148));
                            vmMatch.ColorTexto = Brushes.Black;
                        }
                    }
                }

                FiltrarCatalogo();
            }
            catch (Exception)
            {
                MessageBox.Show("Error al cargar los datos del formulario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
            }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) { }

        private void FiltrarCatalogo()
        {
            if (_todosLosPlatillosVM == null) return;

            string filtro = TextoBuscar?.Trim().ToLower() ?? string.Empty;

            var platillosFiltrados = string.IsNullOrEmpty(filtro)
                ? _todosLosPlatillosVM
                : _todosLosPlatillosVM.Where(vm => vm.Platillo.nombre.ToLower().Contains(filtro)).ToList();

            icAlimentos.ItemsSource = platillosFiltrados.Where(vm => vm.Platillo.idCategoria == 1).ToList();
            icBebidas.ItemsSource = platillosFiltrados.Where(vm => vm.Platillo.idCategoria == 2).ToList();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PlatilloItemViewModel vm)
            {
                if (int.TryParse(vm.CantidadInput, out int cantidad) && cantidad > 0)
                {
                    vm.TextoBoton = "Agregado";
                    vm.EsHabilitado = false;
                    vm.ColorBoton = new SolidColorBrush(Color.FromRgb(153, 150, 148));
                    vm.ColorTexto = Brushes.Black;
                }
                else
                {
                    MessageBox.Show("Ingrese una cantidad válida mayor a 0.", "Cantidad inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            string mensajeCancelar = _esActualizar
                ? "No se actualizará la orden. ¿Está seguro(a)?"
                : "No se creará la orden. ¿Está seguro(a)?";

            MessageBoxResult confirmacion = MessageBox.Show(
                mensajeCancelar,
                "Advertencia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                _timer?.Stop();
                NavigationService?.GoBack();
            }
        }

        private void BtnGuardarOrden_Click(object sender, RoutedEventArgs e)
        {
            if (cbMesas.SelectedValue == null)
            {
                MessageBox.Show("No se seleccionó ninguna mesa.\nVerifique la selección e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var seleccionados = _todosLosPlatillosVM
                .Where(vm => vm.TextoBoton == "Agregado")
                .ToDictionary(vm => vm.Platillo, vm => int.Parse(vm.CantidadInput));

            if (seleccionados.Count == 0)
            {
                MessageBox.Show("No se seleccionó ningún platillo.\nVerifique la selección e inténtelo de nuevo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string resumenPlatillos = "¿Son correctos los platillos?\n\n";
            foreach (var kvp in seleccionados)
            {
                Platillo platillo = kvp.Key;
                int cantidad = kvp.Value;
                resumenPlatillos += $"{cantidad} {platillo.nombre}\n";
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                resumenPlatillos.TrimEnd(),
                "Confirmar orden",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                if (!_esActualizar)
                {
                    CrearNuevaOrden(seleccionados);
                }
                else
                {
                    ActualizarOrden(seleccionados);
                }
            }
        }

        private void CrearNuevaOrden(Dictionary<Platillo, int> platillosSeleccionados)
        {
            try
            {
                foreach (var kvp in platillosSeleccionados)
                {
                    Platillo platillo = kvp.Key;
                    int cantidadOrdenada = kvp.Value;
                    List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(platillo.idPlatillo);

                    foreach (var receta in ingredientesReceta)
                    {
                        int totalRequeridoEnBase = receta.cantidadRequerida * cantidadOrdenada;
                        Ingrediente ingrediente = IngredienteDAO.ObtenerIngrediente(receta.idIngrediente);
                        if (ingrediente == null) continue;

                        int stockActual = IngredienteDAO.ObtenerCantidadActual(receta.idIngrediente);

                        if (stockActual < totalRequeridoEnBase)
                        {
                            MessageBox.Show($"No hay suficientes ingredientes para preparar {platillo.nombre}. Comuníquese con el Gerente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                using (var scope = new TransactionScope())
                {
                    Orden nuevaOrden = new Orden
                    {
                        fechaCreacion = DateTime.Now,
                        idEmpleado = _mesero.idEmpleado,
                        idMesa = (int)cbMesas.SelectedValue,
                        idEstado = 1 // 1 = En espera
                    };

                    int idOrdenCreada = OrdenDAO.InsertarOrden(nuevaOrden);

                    foreach (var kvp in platillosSeleccionados)
                    {
                        Platillo platillo = kvp.Key;
                        int cantidadOrdenada = kvp.Value;

                        DetalleOrden detalle = new DetalleOrden
                        {
                            idOrden = idOrdenCreada,
                            idPlatillo = platillo.idPlatillo,
                            cantidadPlatillo = cantidadOrdenada,
                            precioUnitario = platillo.costo,
                            estado = "En espera"
                        };
                        DetalleOrdenDAO.InsertarDetalleOrden(detalle);

                        List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(platillo.idPlatillo);
                        foreach (var receta in ingredientesReceta)
                        {
                            int totalRequeridoEnBase = receta.cantidadRequerida * cantidadOrdenada;
                            Ingrediente ingrediente = IngredienteDAO.ObtenerIngrediente(receta.idIngrediente);

                            ingrediente.cantidad -= totalRequeridoEnBase;
                            IngredienteDAO.ActualizarInventario(ingrediente.idIngrediente, ingrediente.cantidad);
                        }
                    }

                    int idMesaSeleccionada = (int)cbMesas.SelectedValue;
                    MesaDAO.ActualizarDisponibilidadMesa(idMesaSeleccionada, "Ocupada");

                    scope.Complete();
                }

                MessageBox.Show("Se ha creado la orden con éxito. ¡Buen provecho!", "Orden creada", MessageBoxButton.OK, MessageBoxImage.Information);
                _timer?.Stop();
                NavigationService?.GoBack();
            }
            catch (OrdenException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DetallesOrdenException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarOrden(Dictionary<Platillo, int> platillosSeleccionados)
        {
            try
            {
                foreach (var kvp in platillosSeleccionados)
                {
                    var vm = _todosLosPlatillosVM.FirstOrDefault(x => x.Platillo.idPlatillo == kvp.Key.idPlatillo);
                    if (vm != null && vm.CantidadOriginal > 0)
                    {
                        if (kvp.Value < vm.CantidadOriginal)
                        {
                            MessageBox.Show($"La cantidad del platillo {vm.Platillo.nombre} no puede ser menor a la cantidad previamente ordenada ({vm.CantidadOriginal}).", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                List<DetalleOrden> detallesExistentes = DetalleOrdenDAO.ObtenerDetallesOrden(_ordenActual.idOrden);

                foreach (var kvp in platillosSeleccionados)
                {
                    Platillo platillo = kvp.Key;
                    int cantidadNuevaTotal = kvp.Value;

                    var detallePrevio = detallesExistentes.FirstOrDefault(d => d.idPlatillo == platillo.idPlatillo);
                    int cantidadAnterior = detallePrevio != null ? detallePrevio.cantidadPlatillo : 0;
                    int cantidadAdicional = cantidadNuevaTotal - cantidadAnterior;

                    if (cantidadAdicional > 0)
                    {
                        List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(platillo.idPlatillo);
                        foreach (var receta in ingredientesReceta)
                        {
                            int totalRequeridoEnBase = receta.cantidadRequerida * cantidadAdicional;
                            Ingrediente ingrediente = IngredienteDAO.ObtenerIngrediente(receta.idIngrediente);
                            if (ingrediente == null) continue;

                            int stockActual = IngredienteDAO.ObtenerCantidadActual(receta.idIngrediente);

                            if (stockActual < totalRequeridoEnBase)
                            {
                                MessageBox.Show($"No hay suficientes ingredientes para preparar el platillo {platillo.nombre}. Comuníquese con el gerente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }

                using (var scope = new TransactionScope())
                {
                    foreach (var kvp in platillosSeleccionados)
                    {
                        Platillo platillo = kvp.Key;
                        int cantidadNuevaTotal = kvp.Value;

                        var detallePrevio = detallesExistentes.FirstOrDefault(d => d.idPlatillo == platillo.idPlatillo);
                        int cantidadAnterior = detallePrevio != null ? detallePrevio.cantidadPlatillo : 0;
                        int cantidadAdicional = cantidadNuevaTotal - cantidadAnterior;

                        if (detallePrevio != null)
                        {
                            if (cantidadAdicional > 0)
                            {
                                DetalleOrdenDAO.ActualizarCantidadDetalle(detallePrevio.idDetalleOrden, cantidadNuevaTotal);

                                List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(platillo.idPlatillo);
                                foreach (var receta in ingredientesReceta)
                                {
                                    int totalRequeridoEnBase = receta.cantidadRequerida * cantidadAdicional;
                                    Ingrediente ingrediente = IngredienteDAO.ObtenerIngrediente(receta.idIngrediente);

                                    ingrediente.cantidad -= totalRequeridoEnBase;
                                    IngredienteDAO.ActualizarInventario(ingrediente.idIngrediente, ingrediente.cantidad);
                                }
                            }
                        }
                        else
                        {
                            DetalleOrden detalle = new DetalleOrden
                            {
                                idOrden = _ordenActual.idOrden,
                                idPlatillo = platillo.idPlatillo,
                                cantidadPlatillo = cantidadNuevaTotal,
                                precioUnitario = platillo.costo,
                                estado = "En espera"
                            };
                            DetalleOrdenDAO.InsertarDetalleOrden(detalle);

                            List<Receta> ingredientesReceta = RecetaDAO.ObtenerReceta(platillo.idPlatillo);
                            foreach (var receta in ingredientesReceta)
                            {
                                int totalRequeridoEnBase = receta.cantidadRequerida * cantidadNuevaTotal;
                                Ingrediente ingrediente = IngredienteDAO.ObtenerIngrediente(receta.idIngrediente);

                                ingrediente.cantidad -= totalRequeridoEnBase;
                                IngredienteDAO.ActualizarInventario(ingrediente.idIngrediente, ingrediente.cantidad);
                            }
                        }
                    }

                    OrdenDAO.ActualizarEstadoOrden(_ordenActual.idOrden, 1); // 1 = En espera

                    scope.Complete();
                }

                MessageBox.Show("Se ha actualizado la orden con éxito. ¡Buen provecho!", "Orden actualizada", MessageBoxButton.OK, MessageBoxImage.Information);
                _timer?.Stop();
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class PlatilloItemViewModel : INotifyPropertyChanged
    {
        private string _cantidadInput;
        private string _textoBoton;
        private bool _esHabilitado;
        private Brush _colorBoton;
        private Brush _colorTexto;

        public Platillo Platillo { get; set; }
        public int CantidadOriginal { get; set; } = 0;

        public string CantidadInput
        {
            get => _cantidadInput;
            set
            {
                if (_cantidadInput != value)
                {
                    _cantidadInput = value;
                    OnPropertyChanged(nameof(CantidadInput));

                    TextoBoton = "Agregar";
                    EsHabilitado = true;
                    ColorBoton = new SolidColorBrush(Color.FromRgb(227, 109, 45));
                    ColorTexto = Brushes.White;
                }
            }
        }

        public string TextoBoton
        {
            get => _textoBoton;
            set { _textoBoton = value; OnPropertyChanged(nameof(TextoBoton)); }
        }

        public bool EsHabilitado
        {
            get => _esHabilitado;
            set { _esHabilitado = value; OnPropertyChanged(nameof(EsHabilitado)); }
        }

        public Brush ColorBoton
        {
            get => _colorBoton;
            set { _colorBoton = value; OnPropertyChanged(nameof(ColorBoton)); }
        }

        public Brush ColorTexto
        {
            get => _colorTexto;
            set { _colorTexto = value; OnPropertyChanged(nameof(ColorTexto)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
