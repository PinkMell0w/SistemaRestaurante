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
using System.Windows.Shapes;

namespace ClienteSistemaRestaurante.Vistas.Cajero
{
    /// <summary>
    /// Interaction logic for VistaCajero.xaml
    /// </summary>
    public partial class VistaCajero : Window
    {
        Window _loginWindow;
        Empleado _cajero;

        public VistaCajero(Window loginWindow, Empleado cajero)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
            _cajero = cajero;
        }

        public Empleado CajeroActual => _cajero;

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
