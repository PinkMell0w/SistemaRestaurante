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
    /// Interaction logic for EmpleadosSubmenu.xaml
    /// </summary>
    public partial class EmpleadosSubmenu : Page
    {
        public EmpleadosSubmenu()
        {
            InitializeComponent();
        }

        private void BtnAgregarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FormularioEmpleado(false, null));
        }

        private void BtnConsultarEmpleados_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ListaEmpleados());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Módulo de Empleados";
            }
        }
    }
}
