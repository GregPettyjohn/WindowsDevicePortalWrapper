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
    public class DeviceConnectionViewModel : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        ///  Default constructor creates a null diagnostic sink
        /// </summary>
        /// <remarks>Diagnostic output will be lost</remarks>
        public DeviceConnectionViewModel()
        {
            diagnostics = new NullDiagnosticSink();
        }

        /// <summary>
        /// Use this constructor to specify a diagnostic sink for diagnostic output
        /// </summary>
        /// <param name="diags">Diagnostic sink that will receive all the diagnostic output</param>
        public DeviceConnectionViewModel(IDiagnosticSink diags)
        {
            diagnostics = diags;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private DevicePortal.OperatingSystemInformation osInfo;
        private DevicePortal cachedPortal;
        private IDiagnosticSink diagnostics;
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
        // Note: It is!
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
            diagnostics.OutputDiagnosticString("Connect clicked\n");
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
            try
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
                    cachedPortal = null;

                    // Propagate to the user
                    diagnostics.OutputDiagnosticString("Failed to connect to device. HTTP status code: {0}\n", portal.ConnectionHttpStatusCode.ToString());
                }
            }
            catch(Exception exn)
            {
                diagnostics.OutputDiagnosticString("Exception when trying to connect:\n{0}\nStackTrace: \n{1}\n", exn.Message, exn.StackTrace);
            }

            ClearCredentials();
        }

        private async Task UpdateDeviceNameAsync()
        {
            try
            {
                await cachedPortal.SetDeviceName(DeviceName);
                // TODO: Factor this loop out into a function
                // should consume a retry count (rather than looping forever)
                do
                {
                    await cachedPortal.Connect(updateConnection: false);
                    if (cachedPortal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                    {
                        diagnostics.OutputDiagnosticString("Failed to connect to device. HTTP status code: {0}\nRetrying connection...", cachedPortal.ConnectionHttpStatusCode.ToString());
                    }
                }
                while (cachedPortal.ConnectionHttpStatusCode != HttpStatusCode.OK);
                await GetDeviceInfoAsync();
            }
            catch(Exception exn)
            {
                diagnostics.OutputDiagnosticString("Exception when trying to connect:\n{0}\nStackTrace: \n{1}\n", exn.Message, exn.StackTrace);
            }
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

        public bool CanUpdateDeviceName
        {
            get
            {
                return cachedPortal != null && !CanExecuteConnect(null);
            }
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
