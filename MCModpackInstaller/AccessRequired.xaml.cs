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

namespace MCModpackInstaller
{
    /// <summary>
    /// Interaction logic for AccessRequired.xaml
    /// </summary>
    public partial class AccessRequired : Window
    {
        Credentials.secrets cSecret = new Credentials.secrets();
        int accessClickCount = 0;

        public AccessRequired()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAccess_Click(object sender, RoutedEventArgs e)
        {
            accessClickCount++;
            int CheckAccess = cSecret.AccessReq(pssUserKey.Password);
            if (accessClickCount <= 3)
            {
                if(CheckAccess == 1)
                {
                    AdvanceSettings advWPF = new AdvanceSettings();
                    advWPF.Owner = Application.Current.MainWindow;
                    advWPF.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("u sussy bro?", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
            else
            {
                MessageBox.Show("ur sussy","ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();

            }
        }
    }
}
