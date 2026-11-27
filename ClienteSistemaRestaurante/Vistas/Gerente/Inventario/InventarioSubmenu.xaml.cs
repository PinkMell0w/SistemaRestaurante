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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario
{
    /// <summary>
    /// Interaction logic for InventarioSubmenu.xaml
    /// </summary>
    public partial class InventarioSubmenu : Page
    {
        public InventarioSubmenu()
        {
            InitializeComponent();
        }

        private void BtnIngredientes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Ingredientes.IngredientesSubmenu());
        }

        private void BtnMesas_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Mesas.MesasSubmenu());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Inventario";
            }
        }
    }
}
