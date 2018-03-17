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

namespace BSK1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EncryptionService encryptionService;

        private String ftb;
        private String fileTextBox {
            get {
                return this.ftb;
            }
            set {
                this.ftb = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            encryptionService = new EncryptionService();
            mainWindow.DataContext = fileTextBox;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                fileTextBox = dlg.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(fileTextBox)) {
                encryptionService.EncryptFile(fileTextBox, fileTextBox, @"12345678", System.Security.Cryptography.CipherMode.CBC);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(fileTextBox)) {
                encryptionService.DecryptFile(fileTextBox, fileTextBox, @"12345678", System.Security.Cryptography.CipherMode.CBC);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
