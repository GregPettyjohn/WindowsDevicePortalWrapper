using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDeviceConnection2
{
    public class ViewModelGroup
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        public ViewModelGroup()
        {


            Diagnostics = new DiagnosticOutputViewModel();

            DebugDiagnosticSink debugDiags = new DebugDiagnosticSink();
            AggregateDiagnosticSink aggDiags = new AggregateDiagnosticSink(Diagnostics, debugDiags);

            DeviceConnection = new DeviceConnectionViewModel(aggDiags);
            DeviceConnection.AddDevicePortalConnectionFactory(new XboxDevicePortalConnectionFactory());
            DeviceConnection.AddDevicePortalConnectionFactory(new GenericDevicePortalConnectionFactory());
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        public DiagnosticOutputViewModel Diagnostics { get; private set; }
        public DeviceConnectionViewModel DeviceConnection { get; private set; }
        #endregion // Properties
    }
}
