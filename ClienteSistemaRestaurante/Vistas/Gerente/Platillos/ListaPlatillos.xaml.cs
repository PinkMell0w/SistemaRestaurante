using ClienteSistemaRestaurante.Datos;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Platillos
{
    /// <summary>
    /// Interaction logic for ListaPlatillos.xaml
    /// </summary>
    public partial class ListaPlatillos : Page
    {
        public ListaPlatillos()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Consultar platillos";
            }

            CargarListaPlatillos();
        }

        private void CargarListaPlatillos()
        {
            try
            {
                var listaPlatillos = Datos.PlatilloDAO.ObtenerPlatillos();
                icPlatillos.ItemsSource = listaPlatillos;
            }
            catch (Exception)
            {
                MessageBox.Show("Error al cargar la lista de platillos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BtnModificarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Platillo platilloSeleccionado)
            {
                NavigationService?.Navigate(new FormularioPlatillo(true, platilloSeleccionado));
            }
        }

        private void BtnEliminarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Platillo platilloSeleccionado)
            {
                string mensajeConfirmacion = "¿Está seguro de que desea eliminar este platillo? Esta acción no se puede deshacer.\n\n" +
                    "Datos del platillo:\n" +
                    $"Nombre: {platilloSeleccionado.nombre}\n" +
                    $"Costo: ${platilloSeleccionado.costo:F2}\n" +
                    $"Categoria: {platilloSeleccionado.Categoria.nombreCategoria}";
                var resultado = MessageBox.Show(mensajeConfirmacion, "Eliminar platillo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool eliminado = PlatilloDAO.EliminarPlatillo(platilloSeleccionado.idPlatillo);
                        if (eliminado)
                        {
                            MessageBox.Show("Se ha eliminado al platillo con éxito.", "Platillo eliminado");
                            CargarListaPlatillos();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error al eliminar el platillo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
