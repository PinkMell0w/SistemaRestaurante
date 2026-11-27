using ClienteSistemaRestaurante.Modelos;
using ClienteSistemaRestaurante.Vistas.Cajero;
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

namespace ClienteSistemaRestaurante.Vistas.Mesero
{
    /// <summary>
    /// Interaction logic for MenuMesero.xaml
    /// </summary>
    public partial class MenuMesero : Page
    {
        Empleado _meseroActual;

        public MenuMesero()
        {
            InitializeComponent();
        }

        private void BtnNuevaOrden_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FormularioOrden(false, null, _meseroActual));
        }

        private void BtnConsultarOrdenes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ListaOrdenes(_meseroActual));
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Close();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is VistaMesero ventanaMesero)
            {
                ventanaMesero.Title = "Sistema de Gestión de Restaurante";
                _meseroActual = ventanaMesero.MeseroActual;
                runNombreCompleto.Text = $"{_meseroActual.nombres} {_meseroActual.apellidoPaterno} {_meseroActual.apellidoMaterno}".Trim();
            }
        }
    }
}
