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
    /// Interaction logic for MoreSettings.xaml
    /// </summary>
    public partial class MoreSettings : Window
    {
        private MainWindow _mainWindow;

        public MoreSettings(MainWindow main)
        {
            _mainWindow = main;
            InitializeComponent();

            setStatus();
        }

        public void setStatus()
        {
            tglSaveMCSettings.IsChecked = _mainWindow.saveMCSettings;
        }

        private void tglSaveMCSettings_Checked(object sender, RoutedEventArgs e)
        {
            _mainWindow.saveMCSettings = true;
        }

        private void tglSaveMCSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            _mainWindow.saveMCSettings = false;
        }
    }
}
