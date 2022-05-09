using Google.Cloud.Firestore;
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
        private MainWindow mainForm = null;
        FirestoreDb database;
        int accessClickCount = 0;

        public AccessRequired(MainWindow callingForm)
        {
            mainForm = callingForm;
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnAccess_Click(object sender, RoutedEventArgs e)
        {
            accessClickCount++;
            connectionStatus();
            var getCheckAccess = await cSecret.AccessReq(pssUserKey.Password);
            int CheckAccess = getCheckAccess;
            if (accessClickCount <= 3)
            {
                if(CheckAccess == 1)
                {
                    AdvanceSettings advWPF = new AdvanceSettings(mainForm);
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

        public void connectionStatus()
        {
            int ConnectionStat = cSecret.ConString();


            if (ConnectionStat == 1)
            {
                // Success
                // Background Image should continue to loop
                database = cSecret.db;
            }
            else
            {
            }
        }
    }
}
