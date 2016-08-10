using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
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
        
        private IDiagnosticSink diagnostics;
        #endregion // Private Members

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties

        #region Current Portal
        private DevicePortal currentPortal;
        public DevicePortal CurrentPortal
        {
            get
            {
                return currentPortal;
            }
            private set
            {
                SetField(ref currentPortal, value, "CurrentPortal");
                OnPropertyChanged("CanUpdateDeviceName");
            }
        }
        #endregion // Current Portal

        #region IsConnecting
        private bool isConnecting;
        public bool IsConnecting
        {
            get
            {
                return isConnecting;
            }
            private set
            {
                SetField(ref isConnecting, value, "IsConnecting");
                OnPropertyChanged("CanUpdateDeviceName");
            }
        }
        #endregion // IsConnecting

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
                return CurrentPortal == null ? @"<unknown>" : deviceName;
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
                return CurrentPortal == null ? @"<unknown>" : deviceFamily;
            }

            set
            {
                SetField(ref deviceFamily, value, "DeviceFamily");
            }
        }
        #endregion // DeviceFamily

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
        private DelegateCommand connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (connectCommand == null)
                {
                    connectCommand = new DelegateCommand(ExecuteConnect, CanExecuteConnect);
                    connectCommand.ObservesProperty(() => DeviceIP);
                    connectCommand.ObservesProperty(() => UserName);
                    connectCommand.ObservesProperty(() => Password);
                }
                return connectCommand;
            }
        }

        private void ExecuteConnect()
        {
            diagnostics.OutputDiagnosticString("Connect clicked\n");
            if (!IsConnecting)
            {
                IsConnecting = true;
                CurrentPortal = null;
                osInfo = null;
                OnOsInfoChanged();

                IDevicePortalConnection conn = new XboxDevicePortalConnection(deviceIP, userName, password);

                DevicePortal portal = new DevicePortal(conn);

                portal.ConnectionStatus += (DevicePortal sender, DeviceConnectionStatusEventArgs args) =>
                {
                    diagnostics.OutputDiagnosticString("Connection status update: Status: {0}, Phase: {1}\n", args.Status, args.Phase);
                    if (args.Status == DeviceConnectionStatus.Connected)
                    {
                        this.CurrentPortal = portal;
                        this.osInfo = conn.OsInfo;
                        OnOsInfoChanged();
                        IsConnecting = false;
                    }
                    else if (args.Status == DeviceConnectionStatus.Failed)
                    {
                        IsConnecting = false;
                    }
                };

                Task t = ConnectAsync(portal);
            }
        }

        private bool CanExecuteConnect()
        {
            return
                !IsConnecting &&
                !string.IsNullOrWhiteSpace(DeviceIP) &&
                !string.IsNullOrWhiteSpace(UserName) &&
                !string.IsNullOrWhiteSpace(Password);
        }
        #endregion // Connect Command

        #region UpdateDeviceName Command
        private DelegateCommand updateDeviceNameCommand;

        public ICommand UpdateDeviceNameCommand
        {
            get
            {
                if(updateDeviceNameCommand == null)
                {
                    updateDeviceNameCommand = new DelegateCommand(ExecuteUpdateDeviceName, CanExecuteUpdateDeviceName);
                    updateDeviceNameCommand.ObservesProperty(() => CanUpdateDeviceName);
                    updateDeviceNameCommand.ObservesProperty(() => DeviceName);

                }
                return updateDeviceNameCommand;
            }
        }

        private void ExecuteUpdateDeviceName()
        {
            Task t = UpdateDeviceNameAsync();
        }

        private bool CanExecuteUpdateDeviceName()
        {
            return CanUpdateDeviceName && !string.IsNullOrWhiteSpace(DeviceName);
        }
        #endregion // UpdateDeviceName Command
        #endregion // Commands

        //-------------------------------------------------------------------
        // Misc. Methods
        //-------------------------------------------------------------------
        #region Misc. Methods
        private async Task ConnectAsync(DevicePortal portal)
        {
            try
            {
                await portal.Connect();
            }
            catch(Exception exn)
            {
                diagnostics.OutputDiagnosticString("Exception when trying to connect:\n{0}\nStackTrace: \n{1}\n", exn.Message, exn.StackTrace);
            }
        }

        private async Task UpdateDeviceNameAsync()
        {
            try
            {
                await CurrentPortal.SetDeviceName(DeviceName);
                // TODO: Factor this loop out into a function
                // should consume a retry count (rather than looping forever)
                do
                {
                    await CurrentPortal.Connect(updateConnection: false);
                    if (CurrentPortal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                    {
                        diagnostics.OutputDiagnosticString("Failed to connect to device. HTTP status code: {0}\nRetrying connection...", CurrentPortal.ConnectionHttpStatusCode.ToString());
                    }
                }
                while (CurrentPortal.ConnectionHttpStatusCode != HttpStatusCode.OK);
                await GetDeviceInfoAsync();
            }
            catch(Exception exn)
            {
                diagnostics.OutputDiagnosticString("Exception when trying to connect:\n{0}\nStackTrace: \n{1}\n", exn.Message, exn.StackTrace);
            }
        }

        private async Task GetDeviceInfoAsync()
        {
            DeviceName = await CurrentPortal.GetDeviceName();
            DeviceFamily = await CurrentPortal.GetDeviceFamily();
            osInfo = await CurrentPortal.GetOperatingSystemInformation();
            OnOsInfoChanged();
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

        private void OnOsInfoChanged()
        {
            OnPropertyChanged("ComputerName");
            OnPropertyChanged("Language");
            OnPropertyChanged("OsEdition");
            OnPropertyChanged("OsEditionId");
            OnPropertyChanged("OsVersion");
            OnPropertyChanged("Platform");
        }

        public bool CanUpdateDeviceName
        {
            get
            {
                return CurrentPortal != null && !IsConnecting;
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
