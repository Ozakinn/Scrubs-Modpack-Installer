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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XamlAnimatedGif;
using Google.Cloud.Firestore;
using DocumentReference = Google.Cloud.Firestore.DocumentReference;

namespace MCModpackInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Credentials.secrets cSecret = new Credentials.secrets();
        FirestoreDb database;

        int ozakiClickCount = 0;
        int ConnectionStat;


        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
                
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOzaki_Click(object sender, RoutedEventArgs e)
        {
            ozakiClickCount++;
            if (ozakiClickCount == 10)
            {
                AccessRequired accessReqWPF = new AccessRequired();
                accessReqWPF.Owner = Application.Current.MainWindow;
                accessReqWPF.Show();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionStatus();
            if (ConnectionStat == 1)
            {
                //SaveDB();
            }
        }

        public void connectionStatus()
        {
            ConnectionStat = cSecret.ConString();

            if (ConnectionStat == 1)
            {
                // Success
                // Background Image should continue to loop
                database = cSecret.db;
            }
            else
            {
                //AnimationBehavior.SetRepeatBehavior(bgGIF, new RepeatBehavior(TimeSpan.Zero));
            }
        }

        public void SaveDB()
        {
            DocumentReference doc = database.Collection("Settings").Document("Maintenance");
            Dictionary<string, object> data1 = new Dictionary<string, object>()
            {
                {"fname","ler"}
            };
            doc.SetAsync(data1);
            MessageBox.Show("check ur DB");
        }

        private void bgVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            bgVideo.Position = new TimeSpan(0, 0, 1);
            bgVideo.Play();
        }
    }
}
