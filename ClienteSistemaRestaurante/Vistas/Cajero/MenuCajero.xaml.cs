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

namespace ClienteSistemaRestaurante.Vistas.Cajero
{
    /// <summary>
    /// Interaction logic for MenuCajero.xaml
    /// </summary>
    public partial class MenuCajero : Page
    {
        Empleado _cajeroActual;

        public MenuCajero()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is VistaCajero ventanaMesero)
            {
                ventanaMesero.Title = "Sistema de Gestión de Restaurante";
                _cajeroActual = ventanaMesero.CajeroActual;
                runNombreCompleto.Text = $"{_cajeroActual.nombres} {_cajeroActual.apellidoPaterno} {_cajeroActual.apellidoMaterno}".Trim();
            }
        }

        private void BtnGenerarPago_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdenesServidas());
        }

        private void BtnVerPagos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ListaPagosGenerados());
        }
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Close();
            }
        }
    }
}
