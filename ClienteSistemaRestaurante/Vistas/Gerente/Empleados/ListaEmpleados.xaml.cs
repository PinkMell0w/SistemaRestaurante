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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Empleados
{
    /// <summary>
    /// Interaction logic for ListaEmpleados.xaml
    /// </summary>
    public partial class ListaEmpleados : Page
    {
        public ListaEmpleados()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Consultar empleados";
            }

            CargarListaEmpleados();
        }

        private void CargarListaEmpleados()
        {
            try
            {
                List<Empleado> empleados = EmpleadoDAO.ObtenerEmpleadosActivos();
                icEmpleados.ItemsSource = empleados;
            }
            catch (Exception)
            {
                MessageBox.Show("Error al cargar la lista de empleados.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BtnModificarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Empleado empleadoSeleccionado)
            {
                NavigationService?.Navigate(new FormularioEmpleado(true, empleadoSeleccionado));
            }
        }

        private void BtnEliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Empleado empleadoSeleccionado)
            {
                string nombreCompleto = string.IsNullOrWhiteSpace(empleadoSeleccionado.apellidoMaterno)
                    ? $"{empleadoSeleccionado.nombres} {empleadoSeleccionado.apellidoPaterno}"
                    : $"{empleadoSeleccionado.nombres} {empleadoSeleccionado.apellidoPaterno} {empleadoSeleccionado.apellidoMaterno}";

                MessageBoxResult result = MessageBox.Show($"¿Está seguro de que desea eliminar este empleado? Esta acción no se puede deshacer.\n\n" +
                    "Datos del empleado:\n" +
                    $"Nombre: {nombreCompleto}\n" +
                    $"RFC: {empleadoSeleccionado.rfc}\n" +
                    $"Rol: {empleadoSeleccionado.Rol.nombreRol}", "Eliminar empleado", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool eliminado = EmpleadoDAO.EliminarEmpleado(empleadoSeleccionado.idEmpleado);
                        if (eliminado)
                        {
                            MessageBox.Show("Se ha eliminado al empleado con éxito.", "Empleado eliminado");
                            CargarListaEmpleados();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error al eliminar al empleado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
