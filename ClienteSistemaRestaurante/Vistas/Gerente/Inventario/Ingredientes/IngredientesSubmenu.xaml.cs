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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Inventario.Ingredientes
{
    /// <summary>
    /// Interaction logic for IngredientesSubmenu.xaml
    /// </summary>
    public partial class IngredientesSubmenu : Page
    {
        public IngredientesSubmenu()
        {
            InitializeComponent();
        }

        private void BtnAgregarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FormularioIngrediente());
        }

        private void BtnConsultarIngredientes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ListaIngredientes());
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Inventario - Ingredientes";
            }
        }
    }
}
