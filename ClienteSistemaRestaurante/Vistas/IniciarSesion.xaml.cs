using ClienteSistemaRestaurante.LogicaNegocio;
using ClienteSistemaRestaurante.Modelos;
using ClienteSistemaRestaurante.Vistas.Cajero;
using ClienteSistemaRestaurante.Vistas.Cocinero;
using ClienteSistemaRestaurante.Vistas.Gerente;
using ClienteSistemaRestaurante.Vistas.Host;
using ClienteSistemaRestaurante.Vistas.Mesero;
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

namespace ClienteSistemaRestaurante.Vistas
{
    /// <summary>
    /// Interaction logic for IniciarSesion.xaml
    /// </summary>
    public partial class IniciarSesion : Window
    {
        public IniciarSesion()
        {
            InitializeComponent();
        }

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = string.Empty;
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Por favor, ingrese su usuario y contraseña.";
                return;
            }

            AutenticadorUsuario autenticador = new AutenticadorUsuario();
            object datosUsuario;
            string mensajeError;
            string rol = autenticador.Autenticar(usuario, password, out datosUsuario, out mensajeError);

            if (string.IsNullOrEmpty(rol))
            {
                txtError.Text = mensajeError;
                return;
            }

            Window ventanaSiguiente = null;

            switch (rol)
            {
                case "Gerente":
                    ventanaSiguiente = new VistaGerente(this);
                    break;
                case "Cajero":
                    Empleado cajero = datosUsuario as Empleado;
                    ventanaSiguiente = new VistaCajero(this, cajero);
                    break;
                case "Cocinero":
                    ventanaSiguiente = new VistaCocinero(this);
                    break;
                case "Host":
                    ventanaSiguiente = new ListaMesasHost(this);
                    break;
                case "Mesero":
                    Empleado mesero = datosUsuario as Empleado;
                    ventanaSiguiente = new VistaMesero(this, mesero);
                    break;
                default:
                    txtError.Text = "Rol de usuario no reconocido.";
                    return;
            }

            if (ventanaSiguiente != null)
            {
                ventanaSiguiente.Show();
                this.Hide();
                txtUsuario.Clear();
                txtPassword.Clear();
            }
        }
    }
}
