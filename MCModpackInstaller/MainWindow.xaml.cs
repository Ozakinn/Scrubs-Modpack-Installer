using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Cloud.Firestore;
using DocumentReference = Google.Cloud.Firestore.DocumentReference;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;
using System.Collections;
using System.Net;
using System.ComponentModel;
using Ionic.Zip;

namespace MCModpackInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Credentials.secrets cSecret = new Credentials.secrets();
        FirestoreDb database;

        string logoToolTip = "Developed by Ozaki\nMay 2022";

        int ozakiClickCount = 0;
        int ConnectionStat;
        string isMaintenance = "";
        double CurrentVersion = 0.6;

        public int bypassMode = 0;

        string selectedModpack;
        string selectedModpackVersion;

        List<string> versionData = new List<string>();

        string link;
        string versionmodpackDB;
        string fabricforgeVersion;
        string modloaderVersion;

        string sSelectedPath = "";

        string tempPath = Path.GetTempPath();
        string dlPath;
        string minecraftPathDefault = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\.minecraft";
        string extractPath;


        int errorLog = 0;


        WebClient webClient = new WebClient();




        //Set the maximum vaue to int.MaxValue, thus, it could be more accurate


        private BackgroundWorker extractFile;
        private long fileSize;    //the size of the zip file
        private long extractedSizeTotal;    //the bytes total extracted
        private long compressedSize;    //the size of a single compressed file
        private string compressedFileName;    //the name of the file being extracted

        int fileindex;
        int fileamount;
        string filenameInZip;


        public MainWindow()
        {
            InitializeComponent();
            extractFile = new BackgroundWorker();
            extractFile.DoWork += ExtractFile_DoWork;
            extractFile.ProgressChanged += ExtractFile_ProgressChanged;
            extractFile.RunWorkerCompleted += ExtractFile_RunWorkerCompleted;
            extractFile.WorkerReportsProgress = true;
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
            System.Windows.Application.Current.Shutdown();
        }

        private void btnOzaki_Click(object sender, RoutedEventArgs e)
        {
            ozakiClickCount++;
            if (ozakiClickCount == 10)
            {
                ozakiClickCount = 0;
                AccessRequired accessReqWPF = new AccessRequired(this);
                accessReqWPF.Owner = Application.Current.MainWindow;
                accessReqWPF.Show();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            appLogo.ToolTip = logoToolTip;
            disableTextbox();
            Task loadMainWindow = mainWindowDelay(); //Allow Main window to load and initialize first for optimal user usage
            try
            {
                connectionStatus();
                isMaintenance = await cSecret.isMaintenanceAsync();
                if (ConnectionStat == 1 && isMaintenance != "1")
                {


                    Task populatemodpack = retrieveModpack(); //Populate Modpack selection


                    CheckVersion();

                }
                else if (isMaintenance == "1")
                {
                    MaintenanceMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"error window loaded");
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

                //check if maintenance
                //Task maintainance = isMaintenanceAsync();
                //await Task.WhenAll(maintainance);


            }
            else
            {
                // STOP GIF ANIMATION IF DB DISCONNECT
                //AnimationBehavior.SetRepeatBehavior(bgGIF, new RepeatBehavior(TimeSpan.Zero));

                //BG VIDEO ANIMATION STOP IF DB DISCONNECT
                stopBGVideo();
            }
        }

        public async void MaintenanceMode()
        {
            if (bypassMode == 1)
            {
                panelMaintenance.Visibility = Visibility.Hidden;
            }
            else
            {
                panelMaintenance.Visibility = Visibility.Visible;
                disableTextbox();
            }

            while (isMaintenance != "0")
            {
                await Task.Delay(120000); // WAIT 2mins to retry again. To avoid spam read on firestore read log. 50k read daily is the limit of free firestoreDB
                isMaintenance = await cSecret.isMaintenanceAsync();
            }

            panelMaintenance.Visibility = Visibility.Hidden;
            Task populatemodpack = retrieveModpack(); //Populate Modpack selection
            CheckVersion();
            
        }

        public async void CheckVersion()
        {
            List<string> VersionData = await cSecret.VersionChecker();
            string isVersionString = VersionData[0].ToString();
            string isVersionLinkString = VersionData[1].ToString();
            double latestVersion = Convert.ToDouble(isVersionString);
            if (CurrentVersion < latestVersion)
            {
                double needsUpdateNow = latestVersion - CurrentVersion;
                if (needsUpdateNow >= 0.3)
                {
                    disableTextbox();
                    VersionUpdate vu = new VersionUpdate(latestVersion, CurrentVersion, isVersionLinkString, needsUpdateNow);
                    vu.Owner = Application.Current.MainWindow;
                    vu.Show();
                }
                else
                {
                    VersionUpdate vu = new VersionUpdate(latestVersion, CurrentVersion, isVersionLinkString, needsUpdateNow);
                    vu.Owner = Application.Current.MainWindow;
                    vu.Show();
                }
                
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
            await Task.Delay(3000);
            this.Visibility = Visibility.Visible;
        }

        public void enableTextbox()
        {
            cboModpack.IsEnabled = true;
            cboVersion.IsEnabled = true;

            rdAuto.IsEnabled = true;
            rdManual.IsEnabled = true;

            if (rdManual.IsChecked == true)
            {
                txtCustomPath.IsEnabled = false;
                btnSelectPath.IsEnabled = true;
            }
            else
            {
                txtCustomPath.IsEnabled = false;
                btnSelectPath.IsEnabled = false;
            }

            btnInstall.IsEnabled = true;
            btnDeleteModpacks.IsEnabled = true;
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
                versionmodpackDB = versionData[1];
                fabricforgeVersion = versionData[2];
                modloaderVersion = versionData[3];


                if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(versionmodpackDB))
                {

                    bool checkUrl = UrlIsValid(link);
                    if (checkUrl == true)
                    {
                        if (!string.IsNullOrEmpty(fabricforgeVersion) && !string.IsNullOrEmpty(modloaderVersion))
                        {
                            if (modloaderVersion == "Fabric")
                            {

                                System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Make sure fabric mod loader is already installed.\n\n" +
                                "Use Fabric " + fabricforgeVersion +
                                "\n\n Not installed? Click 'Yes'\n" +
                                "https://fabricmc.net/use/installer/", "FABRIC", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
                                if (messageBoxResult.ToString() == "Yes") 
                                { 
                                    System.Diagnostics.Process.Start("https://fabricmc.net/use/installer/"); 
                                }
                            }
                            else if (modloaderVersion == "Forge")
                            {

                                System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Make sure fabric mod loader is already installed.\n\n" +
                                "Make sure forge mod loader is already installed.\n\n" +
                                "Use Forge " + fabricforgeVersion +
                                "\n\n Not installed?\n" +
                                "https://files.minecraftforge.net/net/minecraftforge/forge/", "FORGE", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
                                if (messageBoxResult.ToString() == "Yes")
                                {
                                    System.Diagnostics.Process.Start("https://files.minecraftforge.net/net/minecraftforge/forge/");
                                }
                            }

                        }

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

                        MessageBox.Show("Link is broken or dead. Please report it to Admin.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }



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
            isMaintenance = await cSecret.isMaintenanceAsync();
            if ((cboModpack.SelectedIndex >= 1 && ConnectionStat == 1 && isMaintenance != "1") || (bypassMode == 1 && cboModpack.SelectedIndex >= 1))
            {
                selectedModpack = cboModpack.SelectedItem.ToString();

                Task populateModpackVersion = retrieveModpackVersion();
                await Task.WhenAll(populateModpackVersion);
            }
            else if (isMaintenance == "1")
            {
                MaintenanceMode();
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
            //Disable Installation path control on change to avoid fast click of install
            rdAuto.IsEnabled = false;
            rdManual.IsEnabled = false;
            txtCustomPath.IsEnabled = false;
            btnSelectPath.IsEnabled = false;
            btnInstall.IsEnabled = false;

            connectionStatus();
            isMaintenance = await cSecret.isMaintenanceAsync();
            if ((cboVersion.SelectedIndex >= 1 && ConnectionStat == 1 && isMaintenance != "1") || (bypassMode == 1 && cboVersion.SelectedIndex >= 1))
            {
                selectedModpackVersion = cboVersion.SelectedItem.ToString();

                Task getVersionData = retrieveModpackVersionData();
                //Task.WaitAll(retrieveModpackVersionData());
                await Task.WhenAll(getVersionData);

            }
            else if (isMaintenance == "1")
            {
                MaintenanceMode();
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

            txtCustomPath.IsEnabled = false;
            btnSelectPath.IsEnabled = true;

            btnInstall.IsEnabled = true;

        }

        private void btnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select your custom path of minecraft folder\nUsually it is named '.minecraft' folder.";

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sSelectedPath = fbd.SelectedPath;
            }

            txtCustomPath.Text = sSelectedPath;
        }

        private async void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            connectionStatus();
            isMaintenance = await cSecret.isMaintenanceAsync();
            if (isMaintenance != "1" || bypassMode == 1)
            {
                lblProgressBar.HorizontalAlignment = HorizontalAlignment.Center; //revert back to original position
                progressBarCTRL.Value = 0; //reset value back to 0

                // Check if folder exist, if not do create
                string folderpath = tempPath + @"ScrubsInstaller\Modpack\" + selectedModpack;
                if (!Directory.Exists(folderpath))
                {
                    Directory.CreateDirectory(folderpath);
                }

                dlink();
            }
            else
            {
                MaintenanceMode();
            }
            
        }

        public void dlink()
        {

            string folderpath = tempPath + @"ScrubsInstaller\Modpack\" + selectedModpack;
            string filename = versionmodpackDB+".zip";

            dlPath = folderpath + "\\"+ filename;

            webClient.CancelAsync(); // cancel previous async download

            
            if (File.Exists(dlPath) == true)
            {
                setExtractPath();
            }
            else
            {
                //MessageBox.Show("download started");

                /*
                this.Dispatcher.Invoke(() =>
                {
                    disableTextbox();

                    progressBarCTRL.Maximum = 100;

                    lblProgressBar.Text = "Downloading " + selectedModpackVersion + "...";
                    panelProgress.Visibility = Visibility.Visible;

                    System.IO.Directory.CreateDirectory(tempPath);
                    
                });
                */
                bool checkUrl = UrlIsValid(link);
                if (checkUrl == true)
                {
                    disableTextbox();
                    btnDeleteModpacks.IsEnabled = false;

                    progressBarCTRL.Maximum = 100;

                    lblProgressBar.Text = "Downloading " + selectedModpackVersion + "...";
                    panelProgress.Visibility = Visibility.Visible;

                    System.IO.Directory.CreateDirectory(tempPath);

                    //this are sample of direct link
                    // for debug purposes
                    //string dropbox = "https://www.dropbox.com/s/fodvhb1vsgxbm0x/v8-cs-scrubs-bmc.zip?dl=1";
                    //string gdrive = "https://drive.google.com/uc?export=download&id=1ZczgIrqi1u5gqsoRUn6tQJcoM_X0QNTy&confirm=t";
                    //string odrive = "https://stamariasti-my.sharepoint.com/:u:/g/personal/cataniag_138704_stamaria_sti_edu_ph/Eavcbd5ZfltEnDi8uOWPQOABA6SVBmwGfVKc56sXrfW0lg?e=rQDaYC&download=1";

                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36");
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(link), dlPath);
                }
                else
                {
                    MessageBox.Show("The download link just died.\nPlease try again.","Try again...",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                


            }
        }

        public void setExtractPath()
        {
            //This should detect the radio button AUTO AND INSTALL PATH
            if (rdAuto.IsChecked == true)
            {
                extractPath = minecraftPathDefault;
            }
            else
            {
                extractPath = txtCustomPath.Text;
            }

            string extractPathMods = extractPath + @"\mods";
            string extractPathConfig = extractPath + @"\config";

            System.IO.DirectoryInfo modspath = new DirectoryInfo(extractPathMods);
            System.IO.DirectoryInfo configpath = new DirectoryInfo(extractPathConfig);

            if (Directory.Exists(extractPath) || rdManual.IsChecked == true)
            {

                if (rdManual.IsChecked == true && String.IsNullOrWhiteSpace(txtCustomPath.Text) == true)
                {
                    MessageBox.Show("Please select a custom path");
                }
                else if (rdAuto.IsChecked == true)
                {
                    //delete mods folder content if exist
                    if (Directory.Exists(extractPathMods))
                    {
                        foreach (FileInfo file in modspath.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    //delete config folder content if exist
                    if (Directory.Exists(extractPathConfig))
                    {
                        foreach (FileInfo file in configpath.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    disableTextbox();
                    btnDeleteModpacks.IsEnabled = false;
                    progressBarCTRL.Maximum = int.MaxValue;
                    panelProgress.Visibility = Visibility.Visible;
                    extractFile.RunWorkerAsync();
                }
                else
                {

                    if (Directory.Exists(txtCustomPath.Text))
                    {

                        //delete mods folder content if exist
                        if (Directory.Exists(extractPathMods))
                        {
                            foreach (FileInfo file in modspath.GetFiles())
                            {
                                file.Delete();
                            }
                        }

                        //delete config folder content if exist
                        if (Directory.Exists(extractPathConfig))
                        {
                            foreach (FileInfo file in configpath.GetFiles())
                            {
                                file.Delete();
                            }
                        }

                        disableTextbox();
                        btnDeleteModpacks.IsEnabled = false;
                        progressBarCTRL.Maximum = int.MaxValue;
                        panelProgress.Visibility = Visibility.Visible;
                        extractFile.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Folder did not exist. Select custom path again.");
                    }
                }
            }
            else
            {
                MessageBox.Show("It appears that Minecraft is not installed on your computer.\nInstall it first.");
            }
            
            
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            /*
            Dispatcher.BeginInvoke(
            new ThreadStart(() => progressBarCTRL.Value = e.ProgressPercentage));
            */
            progressBarCTRL.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            setExtractPath();
        }

        private void ExtractFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (errorLog == 1) // Error for ZipException
            {
                errorLog = 0; //reset Errorlog always
                dlink();
            }
            else
            {
                //Set the maximum vaue to int.MaxValue because the process is completed
                //reset back to center position
                lblProgressBar.HorizontalAlignment = HorizontalAlignment.Center;
                progressBarCTRL.Value = int.MaxValue;
                lblProgressBar.Text = "(" + fileamount + "/" + fileamount + "): Installation Complete!";
                //MessageBox.Show("Done!");
                enableTextbox();
                btnDeleteModpacks.IsEnabled = true;
            }
        }

        private void ExtractFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            lblProgressBar.Text = compressedFileName;

            progressBarCTRL.Value = e.ProgressPercentage;

            //calculate the totalPercent
            long totalPercent = ((long)e.ProgressPercentage * compressedSize + extractedSizeTotal * int.MaxValue) / fileSize;
            if (totalPercent > int.MaxValue)
            {
                totalPercent = int.MaxValue;
            }
            progressBarCTRL.Value = (int)totalPercent;
        }

        private void ExtractFile_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string fileName = dlPath;
                


                //get the size of the zip file
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                fileSize = fileInfo.Length;
                using (Ionic.Zip.ZipFile zipFile = Ionic.Zip.ZipFile.Read(fileName))
                {
                    //reset the bytes total extracted to 0
                    extractedSizeTotal = 0;
                    int fileAmount = zipFile.Count;
                    int fileIndex = 0;

                    

                    zipFile.ExtractProgress += Zip_ExtractProgress;
                    foreach (Ionic.Zip.ZipEntry ZipEntry in zipFile)
                    {
                        fileIndex++;
                        compressedFileName = "(" + fileIndex.ToString() + "/" + fileAmount + "): " + ZipEntry.FileName;
                        //get the size of a single compressed file
                        compressedSize = ZipEntry.CompressedSize;
                        ZipEntry.Extract(extractPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                        //calculate the bytes total extracted
                        extractedSizeTotal += compressedSize;

                        //custom addition
                        fileindex = fileIndex;
                        fileamount = fileAmount;
                        filenameInZip = ZipEntry.FileName;
                    }
                }
            }
            catch (Ionic.Zip.ZipException)
            {
                //If file is bad or corrupted - it should start to redownload the file
                errorLog = 1; // Error log zip exception

                //MessageBox.Show("It seems that the file is corrupted.");

                File.Delete(dlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Zip_ExtractProgress(object sender, Ionic.Zip.ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0)
            {
                long percent = e.BytesTransferred * int.MaxValue / e.TotalBytesToTransfer;
                //Console.WriteLine("Indivual: " + percent);
                extractFile.ReportProgress((int)percent);
            }
        }

        private void btnViewModpacks_Click(object sender, RoutedEventArgs e)
        {
            string scrubspath = tempPath + @"ScrubsInstaller\Modpack\";
            if (!Directory.Exists(scrubspath))
            {
                Directory.CreateDirectory(scrubspath);
                Process.Start("explorer.exe", tempPath + @"ScrubsInstaller\Modpack\");
            }
            else
            {
                Process.Start("explorer.exe", tempPath + @"ScrubsInstaller\Modpack\");
            }
            
        }

        private void btnDeleteModpacks_Click(object sender, RoutedEventArgs e)
        {
            string deleteModpackPath = tempPath + @"\ScrubsInstaller\Modpack";
            System.IO.DirectoryInfo modpackpath = new DirectoryInfo(deleteModpackPath);

            if (Directory.Exists(deleteModpackPath))
            {
                foreach (DirectoryInfo dir in modpackpath.GetDirectories())
                {
                    dir.Delete(true);
                }

                foreach (FileInfo file in modpackpath.GetFiles())
                {
                    file.Delete();
                }

                MessageBox.Show("All modpacks is now deleted.");
            }
            else
            {
                MessageBox.Show("Folder did not exist or is already deleted.");
            }
        }

        public bool UrlIsValid(string url)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; //Get only the header information -- no need to download any content

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        return true;
                    }
                    else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                    {
                        //log.Warn(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                        Debug.WriteLine(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
                {
                    return false;
                }
                else
                {
                    MessageBox.Show(String.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, url), ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Could not test url {0}.", url), ex.ToString());
            }
            return false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F9)
            {
                ozakiClickCount = 0;
                AccessRequired accessReqWPF = new AccessRequired(this);
                accessReqWPF.Owner = Application.Current.MainWindow;
                accessReqWPF.Show();
                
            }
        }


        public object bypassMaintenance
        {
            get { return panelMaintenance.Visibility; }
            set 
            { 
                Task populatemodpack = retrieveModpack();
                panelMaintenance.Visibility = (Visibility)value;
                bypassMode = 1;
            }
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
