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

namespace ClienteSistemaRestaurante.Vistas.Gerente.Reportes
{
    /// <summary>
    /// Interaction logic for ReportesSubmenu.xaml
    /// </summary>
    public partial class ReportesSubmenu : Page
    {
        public ReportesSubmenu()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window ventanaPadre = Window.GetWindow(this);
            if (ventanaPadre != null)
            {
                ventanaPadre.Title = "Módulo de Reportes";
            }
        }

        private void BtnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FormularioReporte());
        }

        private void BtnVerHistorico_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FormularioHistoricoReportes());
        }
    }
}
