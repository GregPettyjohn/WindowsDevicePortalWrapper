using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ObservationDeck
{

    class SomeViewModel : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties

        #region DeviceIP
        private string deviceIP;
        public string DeviceIP
        {
            get
            {
                return deviceIP;
            }

            set
            {
                SetField(ref deviceIP, value, "DeviceIP");
            }
        }
        #endregion // DeviceIP

        #region UserName
        private string userName;
        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                SetField(ref userName, value, "UserName");
            }
        }
        #endregion // UserName

        #region Password
        private string password;
        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                SetField(ref password, value, "Password");
            }
        }
        #endregion // Password

        private string deviceName;
        public string DeviceName
        {
            get
            {
                return deviceName;
            }

            set
            {
                SetField(ref deviceName, value, "DeviceName");
            }
        }

        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands

        #region Connect Command
        private ICommand connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if(connectCommand == null)
                {
                    connectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);
                }
                return connectCommand;
            }
        }

        private void ExecuteConnect(object obj)
        {
            ConnectAsync();
        }

        private bool CanExecuteConnect(object obj)
        {
            return true;
        }

        private async Task ConnectAsync()
        {
            DevicePortal portal = new DevicePortal(new DevicePortalConnection(deviceIP, userName, password));

            await portal.Connect(updateConnection: false);

            if (portal.ConnectionHttpStatusCode == HttpStatusCode.OK)
            {
                DeviceName = await portal.GetDeviceName();
            }
        }
        #endregion // Connect Command

        #endregion // Commands

        //-------------------------------------------------------------------
        //  INotifyPropertyChanged Implementation
        //-------------------------------------------------------------------
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion // INotifyPropertyChanged implementation
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
