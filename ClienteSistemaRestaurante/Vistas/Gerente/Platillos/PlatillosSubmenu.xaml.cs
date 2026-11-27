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
    /// Interaction logic for PlatillosSubmenu.xaml
    /// </summary>
    public partial class PlatillosSubmenu : Page
    {
        public PlatillosSubmenu()
        {
            InitializeComponent();
        }

        private void BtnRegistrarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FormularioPlatillo(false, null));
        }

        private void BtnConsultarPlatillos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ListaPlatillos());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Módulo de Platillos";
            }
        }
    }
}
