using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using Microsoft.Win32;

namespace BSK1
{
 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string FileName;
        public OpenFileDialog OpenFileDialog;
        public List<string> EncryptionTypes = new List<string>{"ECB", "CBC", "CFB", "OFB"};
        public MainWindow()
        {
            InitializeComponent();
            EncryptionTypesCombo.ItemsSource = EncryptionTypes;
            EncryptionTypesCombo.SelectedIndex = 0;
        }

        private double SecureRand()
        {
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

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            
            OpenFileDialog = new OpenFileDialog();
            if (OpenFileDialog.ShowDialog() == true)
                FileName = OpenFileDialog.FileName;
            ChosenFile.Content = FileName;
            EncryptedName.Text = "encrypted" + OpenFileDialog.SafeFileName;
            
        }
    }
}
