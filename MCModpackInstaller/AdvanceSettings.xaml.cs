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
    /// Interaction logic for AdvanceSettings.xaml
    /// </summary>
    public partial class AdvanceSettings : Window
    {
        Credentials.secrets cSecret = new Credentials.secrets();
        private MainWindow mainForm1 = null;
        FirestoreDb database;
        string isMaintenance;
        

        public AdvanceSettings(MainWindow callingForm)
        {
            mainForm1 = callingForm;
            InitializeComponent();
        }

        private void tglBypassMaintenance_Checked(object sender, RoutedEventArgs e)
        {
            this.mainForm1.bypassMaintenance = Visibility.Hidden;
        }

        private async void tglBypassMaintenance_Unchecked(object sender, RoutedEventArgs e)
        {
            connectionStatus();
            isMaintenance = await cSecret.isMaintenanceAsync();
            if (isMaintenance != "1")
            {
                this.mainForm1.bypassMaintenance = Visibility.Hidden;
            }
            else
            {
                this.mainForm1.bypassMaintenance = Visibility.Visible;
            }

        }

        public void connectionStatus()
        {
            int ConnectionStat = cSecret.ConString();


            if (ConnectionStat == 1)
            {
                database = cSecret.db;

            }
            else
            {
            }
        }
    }
}
