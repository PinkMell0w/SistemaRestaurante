using ClienteSistemaRestaurante.Vistas.Gerente.Empleados;
using ClienteSistemaRestaurante.Vistas.Gerente.Inventario;
using ClienteSistemaRestaurante.Vistas.Gerente.Platillos;
using ClienteSistemaRestaurante.Vistas.Gerente.Reportes;
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

namespace ClienteSistemaRestaurante.Vistas.Gerente
{
    /// <summary>
    /// Interaction logic for VistaGerente.xaml
    /// </summary>
    public partial class VistaGerente : Window
    {
        private Window _loginWindow;

        public VistaGerente(Window loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
        }

        private void BtnEmpleados_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new EmpleadosSubmenu();
        }

        private void BtnPlatillos_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PlatillosSubmenu();
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new InventarioSubmenu();
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ReportesSubmenu();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "¿Está seguro de que desea cerrar sesión y volver al login?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _loginWindow.Show();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
