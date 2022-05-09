using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    /// Interaction logic for VersionUpdate.xaml
    /// </summary>
    public partial class VersionUpdate : Window
    {

        double getLatestVersion;
        double getCurrentVersion;
        string getLink;
        double getNeedUpdate;

        public VersionUpdate(double latestVersion, double currentVersion, string sentLink, double needupdate)
        {
            InitializeComponent();
            getLatestVersion = latestVersion;
            getCurrentVersion = currentVersion;
            getLink = sentLink;
            getNeedUpdate = needupdate;
        }

        private void btnLater_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblNew.Status = getLatestVersion;
            lblCurrent.Status = getCurrentVersion;
            if (getNeedUpdate >= 0.3)
            {
                lblUpdateNow.Visibility = Visibility.Visible;
            }
            else
            {
                lblUpdateNow.Visibility = Visibility.Hidden;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            bool isValidurl = UrlIsValid(getLink);
            if (isValidurl == true)
            {
                Process.Start("explorer", getLink);
            }
            else
            {
                MessageBox.Show("It seems that the link is broken or dead. Please report it to admin.");
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
    }
}
