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

namespace SimpleDeviceConnection
{
    class DeviceConnectionViewModel : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private DevicePortal.OperatingSystemInformation osInfo;
        private DevicePortal cachedPortal;

        #endregion // Private Members

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
                deviceIP = value;
                OnCredentialsChanged();
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
                userName = value;
                OnCredentialsChanged();
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
                password = value;
                OnCredentialsChanged();
            }
        }
        #endregion // Password

        #region DeviceName
        private string deviceName;
        public string DeviceName
        {
            get
            {
                return cachedPortal == null ? @"<unknown>" : deviceName;
            }

            set
            {
                SetField(ref deviceName, value, "DeviceName");
            }
        }
        #endregion // DeviceName

        #region DeviceFamily
        private string deviceFamily;
        public string DeviceFamily
        {
            get
            {
                return cachedPortal == null ? @"<unknown>" : deviceFamily;
            }

            set
            {
                SetField(ref deviceFamily, value, "DeviceFamily");
            }
        }
        #endregion // DeviceFamily

        // TODO: Figure out if this is the same as DeviceName
        #region ComputerName
        public string ComputerName
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.Name;
            }
            private set { }
        }
        #endregion // ComputerName

        #region Language
        public string Language
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.Language;
            }
            private set { }
        }
        #endregion // Language

        #region OsEdition
        public string OsEdition
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.OsEdition;
            }
            private set { }
        }
        #endregion // OsEdition

        #region OsEditionId
        public string OsEditionId
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.OsEditionId.ToString();
            }
            private set { }
        }
        #endregion // OsEditionId

        #region OsVersion
        public string OsVersion
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.OsVersionString;
            }
            private set { }
        }
        #endregion // OsVersion

        #region Platform
        public string Platform
        {
            get
            {
                return osInfo == null ? @"<unknown>" : osInfo.PlatformName;
            }
            private set { }
        }
        #endregion // Platform


        public bool CanUpdateDeviceName
        {
            get
            {
                return cachedPortal != null && !CanExecuteConnect(null);
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
                if (connectCommand == null)
                {
                    connectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);
                }
                return connectCommand;
            }
        }

        private void ExecuteConnect(object obj)
        {
            Task t = ConnectAsync();
        }

        private bool CanExecuteConnect(object obj)
        {
            return
                !string.IsNullOrWhiteSpace(DeviceIP) &&
                !string.IsNullOrWhiteSpace(UserName) &&
                !string.IsNullOrWhiteSpace(Password);
        }
        #endregion // Connect Command

        #region UpdateDeviceName Command
        private ICommand updateDeviceNameCommand;
        public ICommand UpdateDeviceNameCommand
        {
            get
            {
                if(updateDeviceNameCommand == null)
                {
                    updateDeviceNameCommand = new RelayCommand(ExecuteUpdateDeviceName, CanExecuteUpdateDeviceName);
                }
                return updateDeviceNameCommand;
            }
        }

        private void ExecuteUpdateDeviceName(object obj)
        {
            Task t = UpdateDeviceNameAsync();
        }

        private bool CanExecuteUpdateDeviceName(object obj)
        {
            return CanUpdateDeviceName && !string.IsNullOrWhiteSpace(DeviceName);
        }
        #endregion // UpdateDeviceName Command
        #endregion // Commands

        //-------------------------------------------------------------------
        // Misc. Methods
        //-------------------------------------------------------------------
        #region Misc. Methods
        private async Task ConnectAsync()
        {
            DevicePortal portal = new DevicePortal(new XboxDevicePortalConnection(deviceIP, userName, password));

            await portal.Connect(updateConnection: false);

            if (portal.ConnectionHttpStatusCode == HttpStatusCode.OK)
            {
                cachedPortal = portal;
                await GetDeviceInfoAsync();
            }
            else
            {
                // TODO: bubble status up to user
                cachedPortal = null;
            }

            ClearCredentials();
        }

        private async Task UpdateDeviceNameAsync()
        {
            await cachedPortal.SetDeviceName(DeviceName);
            await GetDeviceInfoAsync();
        }

        private async Task GetDeviceInfoAsync()
        {
            DeviceName = await cachedPortal.GetDeviceName();
            DeviceFamily = await cachedPortal.GetDeviceFamily();
            osInfo = await cachedPortal.GetOperatingSystemInformation();
            OnPropertyChanged("ComputerName");
            OnPropertyChanged("Language");
            OnPropertyChanged("OsEdition");
            OnPropertyChanged("OsEditionId");
            OnPropertyChanged("OsVersion");
            OnPropertyChanged("Platform");
        }

        void ClearCredentials()
        {
            deviceIP = "";
            userName = "";
            password = "";
            OnCredentialsChanged();
        }

        private void OnCredentialsChanged()
        {
            OnPropertyChanged("DeviceIP");
            OnPropertyChanged("UserName");
            OnPropertyChanged("Password");
            OnPropertyChanged("CanUpdateDeviceName");
        }
        #endregion // Misc. Methods

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
}
