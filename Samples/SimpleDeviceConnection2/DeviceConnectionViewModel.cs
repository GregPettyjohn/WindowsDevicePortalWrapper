using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleDeviceConnection2
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
            this.ready = true;
            this.diagnostics = new NullDiagnosticSink();
        }

        /// <summary>
        /// Use this constructor to specify a diagnostic sink for diagnostic output
        /// </summary>
        /// <param name="diags">Diagnostic sink that will receive all the diagnostic output</param>
        public DeviceConnectionViewModel(IDiagnosticSink diags)
        {
            this.ready = true;
            this.diagnostics = diags;
        }
        #endregion // Constructors

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
        private SecureString password;
        public SecureString Password
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


        #region Ready
        private bool ready;
        public bool Ready
        {
            get
            {
                return ready;
            }

            private set
            {
                SetField(ref ready, value, "Ready");
            }
        }
        #endregion // Ready

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
                    connectCommand.ObservesProperty(() => Ready);
                }
                return connectCommand;
            }
        }

        private void ExecuteConnect()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] Attempting to connect.\n", deviceIP);

            IDevicePortalConnection conn = new XboxDevicePortalConnection(deviceIP, userName, password);

            DevicePortal portal = new DevicePortal(conn);
            portal.ConnectionStatus += (DevicePortal sender, DeviceConnectionStatusEventArgs args) =>
            {
                diagnostics.OutputDiagnosticString("[{0}] Connection status update: Status: {1}, Phase: {2}\n", deviceIP, args.Status, args.Phase);
                if (args.Status == DeviceConnectionStatus.Connected)
                {
                    diagnostics.OutputDiagnosticString("[{0}] Language: {1}\n", deviceIP, conn.OsInfo.Language);
                    diagnostics.OutputDiagnosticString("[{0}] Name: {1}\n", deviceIP, conn.OsInfo.Name);
                    diagnostics.OutputDiagnosticString("[{0}] OsEdition: {1}\n", deviceIP, conn.OsInfo.OsEdition);
                    diagnostics.OutputDiagnosticString("[{0}] OsEditionId: {1}\n", deviceIP, conn.OsInfo.OsEditionId);
                    diagnostics.OutputDiagnosticString("[{0}] OsVersionString: {1}\n", deviceIP, conn.OsInfo.OsVersionString);
                    diagnostics.OutputDiagnosticString("[{0}] Platform: {1}\n", deviceIP, conn.OsInfo.Platform);
                    diagnostics.OutputDiagnosticString("[{0}] PlatformName: {1}\n", deviceIP, conn.OsInfo.PlatformName);
                    this.Ready = true;

                }
                else if (args.Status == DeviceConnectionStatus.Failed)
                {
                    diagnostics.OutputDiagnosticString("[{0}] Bummer.\n", deviceIP);
                    this.Ready = true;
                }
            };

            this.Ready = false;
            Task t = ConnectAsync(portal);
        }

        private async Task ConnectAsync(DevicePortal portal)
        {
            try
            {
                await portal.Connect();
            }
            catch (Exception exn)
            {
                diagnostics.OutputDiagnosticString("[{0}] Exception when trying to connect:\n[{0}] {1}\nStackTrace: \n[{0}] {2}\n", deviceIP, exn.Message, exn.StackTrace);
            }
        }

        private bool CanExecuteConnect()
        {
            return
                Ready &&
                !string.IsNullOrWhiteSpace(DeviceIP) &&
                !string.IsNullOrWhiteSpace(UserName) &&
                password != null &&
                password.Length > 0;
        }
        #endregion // Connect Command

        #endregion // Commands

        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members

        private IDiagnosticSink diagnostics;
        #endregion // Private Members

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
