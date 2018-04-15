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
        private SessionService sessionService;

        public string FileName;
        public OpenFileDialog OpenFileDialog;

        private Dictionary<String, CipherMode> EncryptionTypes = new Dictionary<String, CipherMode> {
            {"ECB", CipherMode.ECB},
            {"CBC", CipherMode.CBC},
            {"CFB", CipherMode.CFB},
            {"OFB", CipherMode.OFB}
        };

        public MainWindow(EncryptionService encryptionService, SessionService sessionService) {
            InitializeComponent();
            EncryptionTypesCombo.ItemsSource = EncryptionTypes.Keys.ToList();
            EncryptionTypesCombo.SelectedIndex = 0;
            this.encryptionService = encryptionService;
            this.sessionService = sessionService;
        }

        private void Encrypt(object sender, RoutedEventArgs e) {
            encryptionService.EncryptFile(ProgressBar,
                                          OpenFileDialog.FileName,
                                          OpenFileDialog.FileName.Replace(OpenFileDialog.SafeFileName, ""),
                                          OutputName.Text,
                                          sessionService.GetSessionKey(),
                                          EncryptionTypes[EncryptionTypesCombo.Text],
                                          FileRecipent.Text.Split(','));
        }

        private void Decrypt(object sender, RoutedEventArgs e) {
            encryptionService.DecryptFile(ProgressBar,
                                          OpenFileDialog.FileName,
                                          OpenFileDialog.FileName.Replace(OpenFileDialog.SafeFileName, ""),
                                          OutputName.Text);
        }

        private void ChooseFile(object sender, RoutedEventArgs e) {
            OpenFileDialog = new OpenFileDialog();
            if (OpenFileDialog.ShowDialog() == true) {
                FileName = OpenFileDialog.FileName;
            }
            ChosenFile.Content = FileName;
            OutputName.Text = OpenFileDialog.SafeFileName + @".crypt";
        }

    }
}
