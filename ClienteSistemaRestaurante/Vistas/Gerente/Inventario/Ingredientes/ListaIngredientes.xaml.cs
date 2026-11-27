using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.LogicaNegocio;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario.Ingredientes
{
    /// <summary>
    /// Interaction logic for ListaIngredientes.xaml
    /// </summary>
    public partial class ListaIngredientes : Page
    {
        public ListaIngredientes()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Consultar ingredientes";
            }

            CargarListaIngredientes();
        }

        private void CargarListaIngredientes()
        {
            try
            {
                List<Ingrediente> ingredientes = IngredienteDAO.ObtenerTodos();
                var ingredientesConMedida = ingredientes.Select(i => new IngredienteViewModel
                {
                    idIngrediente = i.idIngrediente,
                    nombre = i.nombre,
                    cantidad = ConversorUnidades.ConvertirDeBase(i.cantidad,i.Medida != null ? i.Medida.nombreMedida : string.Empty),
                    idMedida = i.idMedida,
                    nombreMedida = i.Medida != null ? i.Medida.nombreMedida : string.Empty,
                    ModeloOriginal = i
                }).ToList();
                icIngredientes.ItemsSource = ingredientesConMedida;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BtnModificarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IngredienteViewModel ingredienteSeleccionado)
            {
                NavigationService?.Navigate(new FormularioIngrediente(true, ingredienteSeleccionado.ModeloOriginal));
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    public class IngredienteViewModel
    {
        public int idIngrediente { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
        public int idMedida { get; set; }
        public string nombreMedida { get; set; }
        public Ingrediente ModeloOriginal { get; set; }
    }
}
