using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace BSK1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EncryptionService encryptionService;
        public string FileName;
        public OpenFileDialog OpenFileDialog;
       // public List<string> EncryptionTypes = new List<string> { "ECB", "CBC", "CFB", "OFB" };
        private Dictionary<String, CipherMode> EncryptionTypes = new Dictionary<String, CipherMode> {
            {"ECB", CipherMode.ECB},
            {"CBC", CipherMode.CBC},
            {"CFB", CipherMode.CFB},
            {"OFB", CipherMode.OFB}
        };

        private double SecureRand() {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[4];
            provider.GetBytes(byteArray);

            //convert 4 bytes to an integer
            var randomInteger = BitConverter.ToUInt32(byteArray, 0);

            var byteArray2 = new byte[8];
            provider.GetBytes(byteArray2);

            //convert 8 bytes to a double
            var randomDouble = BitConverter.ToDouble(byteArray2, 0);

            return randomDouble;
        }

        public MainWindow() {
            InitializeComponent();
            encryptionService = new EncryptionService();
            EncryptionTypesCombo.ItemsSource = EncryptionTypes.Keys.ToList();
            EncryptionTypesCombo.SelectedIndex = 0;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            /*for (int i = 0; i <= 100; i++) {
                ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = i, DispatcherPriority.Background);
                Thread.Sleep(100);
            }*/
        }

        private void Encrypt(object sender, RoutedEventArgs e) {
            encryptionService.EncryptFile(ProgressBar,
                                          OpenFileDialog.FileName,
                                          OpenFileDialog.FileName.Replace(OpenFileDialog.SafeFileName, ""),
                                          OutputName.Text,
                                          @"test1234",
                                          EncryptionTypes[EncryptionTypesCombo.Text]);
        }

        private void Decrypt(object sender, RoutedEventArgs e) {
            encryptionService.DecryptFile(ProgressBar,
                                          OpenFileDialog.FileName,
                                          OpenFileDialog.FileName.Replace(OpenFileDialog.SafeFileName, ""),
                                          OutputName.Text,
                                          @"test1234",
                                          EncryptionTypes[EncryptionTypesCombo.Text]);
        }

        private void ChooseFile(object sender, RoutedEventArgs e) {
            OpenFileDialog = new OpenFileDialog();
            if (OpenFileDialog.ShowDialog() == true) {
                FileName = OpenFileDialog.FileName;
            }
            ChosenFile.Content = FileName;
            OutputName.Text = OpenFileDialog.SafeFileName;
        }
    }
}
