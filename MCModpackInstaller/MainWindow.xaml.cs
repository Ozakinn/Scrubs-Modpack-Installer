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
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;
using System.Collections;

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

        string selectedModpack;
        string selectedModpackVersion;

        List<string> versionData = new List<string>();

        string link;
        string mcversion;




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
                disableTextbox();



                Task loadMainWindow = mainWindowDelay(); //Allow Main window to load and initialize first for optimal user usage
                Task populatemodpack = retrieveModpack(); //Populate Modpack selection
                
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

                //BG VIDEO ANIMATION PLAY IF DB CONNECTED
                splayBGVideo();
            }
            else
            {
                // STOP GIF ANIMATION IF DB DISCONNECT
                //AnimationBehavior.SetRepeatBehavior(bgGIF, new RepeatBehavior(TimeSpan.Zero));

                //BG VIDEO ANIMATION STOP IF DB DISCONNECT
                stopBGVideo();
            }
        }

        public void stopBGVideo()
        {
            bgVideo.Pause();
        }

        public void splayBGVideo()
        {
            //InitializeComponent();
            //string path = Path.Combine(Environment.CurrentDirectory, @"Images\", "realisticmc.avi");
            //bgVideo.Source = new Uri(path, UriKind.RelativeOrAbsolute);
            bgVideo.Play();
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

        //This delay allow MainWindow to initialize for optimal usage
        async Task mainWindowDelay()
        {
            this.Visibility = Visibility.Hidden;
            await Task.Delay(2000);
            this.Visibility = Visibility.Visible;
        }

        public void enableTextbox()
        {
            cboModpack.IsEnabled = true;
            cboVersion.IsEnabled = true;

            rdAuto.IsChecked = true;
            rdAuto.IsEnabled = true;
            rdManual.IsEnabled = true;

            txtCustomPath.IsEnabled = true;
            btnSelectPath.IsEnabled = true;

            btnInstall.IsEnabled = true;
        }

        public void disableTextbox()
        {
            cboModpack.IsEnabled = false;
            cboVersion.IsEnabled = false;

            rdAuto.IsEnabled = false;
            rdManual.IsEnabled = false;

            txtCustomPath.IsEnabled = false;
            btnSelectPath.IsEnabled = false;

            btnInstall.IsEnabled = false;
        }

        async Task retrieveModpack()
        {

            cboModpack.IsEnabled = true;
            cboModpack.Items.Clear();
            cboModpack.Items.Insert(0, "Please Select Modpack");

            Query qrModpacks = database.Collection("Modpacks");
            QuerySnapshot modpacks = await qrModpacks.GetSnapshotAsync();

            foreach (DocumentSnapshot content in modpacks)
            {
                if (content.Exists)
                {
                    cboModpack.Items.Add(content.Id);
                }
            }
        }

        async Task retrieveModpackVersion()
        {

            cboVersion.IsEnabled = true;
            cboVersion.Items.Clear();
            cboVersion.Items.Insert(0, "Please Select " + selectedModpack + " version");
            cboVersion.SelectedIndex = 0;

            DocumentReference docref = database.Collection("Modpacks").Document(selectedModpack);
            DocumentSnapshot snap = await docref.GetSnapshotAsync();

            if (snap.Exists)
            {
                Dictionary<string, object> key = snap.ToDictionary();

                foreach (var item in key)
                {
                    cboVersion.Items.Add(item.Key);
                }
            }

            
        }

        async Task retrieveModpackVersionData()
        {
            

            DocumentReference docref = database.Collection("Modpacks").Document(selectedModpack);
            DocumentSnapshot snap = await docref.GetSnapshotAsync();


            if (snap.Exists)
            {
                Dictionary<string, object> key = snap.ToDictionary();

                foreach (var item in key)
                {
                    if (item.Key.ToString() == selectedModpackVersion)
                    {

                        versionData.Clear(); //Reset list everytime to get exact data from DB

                        foreach (var versiondata in (IList)item.Value)
                        {
                            versionData.Add(versiondata.ToString()); //add the data the list
                        }
                        break;

                    }
                }
            }

            try
            {
                //get data from list to string
                link = versionData[0];
                mcversion = versionData[1];


                if (!string.IsNullOrEmpty(link))
                {
                    rdAuto.IsChecked = true;
                    rdAuto.IsEnabled = true;
                    rdManual.IsEnabled = true;

                    txtCustomPath.IsEnabled = false;
                    btnSelectPath.IsEnabled = false;

                    btnInstall.IsEnabled = true;
                }
                else
                {
                    disableTextbox();

                    cboModpack.IsEnabled = true;
                    cboVersion.IsEnabled = true;

                    MessageBox.Show("This version is currently being uploaded.", "Try Again Later!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                disableTextbox();

                cboModpack.IsEnabled = true;
                cboVersion.IsEnabled = true;

                MessageBox.Show("This version is currently being uploaded.", "Try Again Later!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            



        }

        private async void cboModpack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            connectionStatus();
            if (cboModpack.SelectedIndex >= 1 && ConnectionStat == 1)
            {
                selectedModpack = cboModpack.SelectedItem.ToString();

                Task populateModpackVersion = retrieveModpackVersion();
                await Task.WhenAll(populateModpackVersion);
            }
            else
            {
                cboVersion.Items.Clear();
                disableTextbox();
                cboModpack.IsEnabled = true;
            }
        }

        private async void cboVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            connectionStatus();
            if (cboVersion.SelectedIndex >= 1 && ConnectionStat == 1)
            {
                selectedModpackVersion = cboVersion.SelectedItem.ToString();

                Task getVersionData = retrieveModpackVersionData();
                //Task.WaitAll(retrieveModpackVersionData());
                await Task.WhenAll(getVersionData);

            }
            else
            {
                disableTextbox();
                cboModpack.IsEnabled = true;
                cboVersion.IsEnabled = true;
            }
        }

        private void rdAuto_Checked(object sender, RoutedEventArgs e)
        {
            rdAuto.IsEnabled = true;
            rdManual.IsEnabled = true;

            txtCustomPath.IsEnabled = false;
            btnSelectPath.IsEnabled = false;

            btnInstall.IsEnabled = true;
        }

        private void rdManual_Checked(object sender, RoutedEventArgs e)
        {
            rdAuto.IsEnabled = true;
            rdManual.IsEnabled = true;

            txtCustomPath.IsEnabled = true;
            btnSelectPath.IsEnabled = true;

            btnInstall.IsEnabled = true;

        }


        // THIS IS A SAMPLE FOR RETRIEVING DATA
        /*
            Query qrModpacks = database.Collection("Modpacks").Document("BMC").Collection("Fabric");
            QuerySnapshot Modpacks = await qrModpacks.GetSnapshotAsync();

            MessageBox.Show("t1");
            foreach (DocumentSnapshot use in Modpacks)
            {
                
                DocumentReference docref = database.Collection("Modpacks").Document("BMC").Collection("Fabric").Document("ModpackVersion");
                DocumentSnapshot snap = await docref.GetSnapshotAsync();
                MessageBox.Show("t2");

                if (snap.Exists)
                {
                    MessageBox.Show("t3");
                    Dictionary<string, object> key = snap.ToDictionary();

                    foreach (var item in key)
                    {
                        MessageBox.Show(item.Key);
                        //Console.WriteLine(item);
                    }
                }
            }

            */
    }
}
