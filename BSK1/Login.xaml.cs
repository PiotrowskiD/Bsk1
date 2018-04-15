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

namespace BSK1
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window {

        EncryptionService encryptionService;
        SessionService sessionService;

        public Login() {
            InitializeComponent();
            encryptionService = new EncryptionService();
            sessionService = new SessionService(encryptionService);
            encryptionService.SetSessionService(sessionService);
        }

        private void RegisterClick(object sender, RoutedEventArgs e) {
            try {
                sessionService.Register(Username.Text, Password.Text);
            } catch (Exception ex) {

            }
        }

        private void LoginClick(object sender, RoutedEventArgs e) {
            try {
                sessionService.Authenticate(Username.Text, Password.Text);
                GoToMainWindow();
            } catch (Exception ex) {

            }
        }

        private void GoToMainWindow() {
            var mainWindow = new MainWindow(encryptionService, sessionService);
            mainWindow.Show();
            this.Close();
        }
    }
}
