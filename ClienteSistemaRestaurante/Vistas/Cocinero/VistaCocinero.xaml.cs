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
using System.Windows.Shapes;

namespace ClienteSistemaRestaurante.Vistas.Cocinero
{
    /// <summary>
    /// Interaction logic for VistaCocinero.xaml
    /// </summary>
    public partial class VistaCocinero : Window
    {
        Window _loginWindow;

        public VistaCocinero(Window loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
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
