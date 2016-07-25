using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDeviceConnection
{
    /// <summary>
    /// Interface describes a destination for diagnostic output
    /// </summary>
    public interface IDiagnosticSink
    {
        void OutputDiagnosticString(bool flush, string fmt, params object[] args);

        void OutputDiagnosticString(string fmt, params object[] args);

        void FlushOutput();
    }

    /// <summary>
    /// Discards all diagnostic output
    /// </summary>
    public class NullDiagnosticSink : IDiagnosticSink
    {
        public void FlushOutput()
        {
        }

        public void OutputDiagnosticString(string fmt, params object[] args)
        {
        }

        public void OutputDiagnosticString(bool flush, string fmt, params object[] args)
        {
        }
    }

    /// <summary>
    /// Sends diagnostic output to the console
    /// </summary>
    public class ConsoleDiagnositcSink : IDiagnosticSink
    {
        public void FlushOutput()
        {
        }

        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            OutputDiagnosticString(true, fmt, args);
        }

        public void OutputDiagnosticString(bool flush, string fmt, params object[] args)
        {
            Console.Write(string.Format(fmt, args));
            if(flush)
            {
                FlushOutput();
            }
        }
    }

    /// <summary>
    /// Sends diagnostic output to the debug channel (i.e. OutputDebugString)
    /// </summary>
    public class DebugDiagnosticSink : IDiagnosticSink
    {
        public void FlushOutput()
        {
        }

        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            OutputDiagnosticString(true, fmt, args);
        }

        public void OutputDiagnosticString(bool flush, string fmt, params object[] args)
        {
            Debug.Write(string.Format(fmt, args));
            if (flush)
            {
                FlushOutput();
            }
        }
    }

    /// <summary>
    /// Combines several diagnostic sinks together
    /// </summary>
    public class AggregateDiagnosticSink : IDiagnosticSink
    {
        private IEnumerable<IDiagnosticSink> sinks;

        public AggregateDiagnosticSink(params IDiagnosticSink[] args)
        {
            sinks = args;
        }

        public void FlushOutput()
        {
            foreach(var diag in sinks)
            {
                diag.FlushOutput();
            }
        }

        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            OutputDiagnosticString(true, fmt, args);
        }

        public void OutputDiagnosticString(bool flush, string fmt, params object[] args)
        {
            foreach(var diag in sinks)
            {
                diag.OutputDiagnosticString(false, fmt, args);
            }
            if(flush)
            {
                FlushOutput();
            }
        }
    }


    /// <summary>
    /// View Model to provide diagnostic output to a view through the OutputStream string property
    /// </summary>
    public class DiagnosticOutputViewModel : INotifyPropertyChanged, IDiagnosticSink
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private const int maxBufferSize = 65535;
        #endregion // Private Members


        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region OutputStream
        private string outputStream;
        public string OutputStream
        {
            get
            {
                return outputStream;
            }

            private set
            {
                SetField(ref outputStream, value, "OutputStream");
            }
        }
        #endregion // OutputStream
        #endregion // Properties

        //-------------------------------------------------------------------
        //  IDiagnosticSink Implementation
        //-------------------------------------------------------------------
        #region IDiagnosticSink Implementation
        /// <summary>
        /// Flush pending output
        /// </summary>
        public void FlushOutput()
        {
            int len = outputStream.Length;
            if ( len > maxBufferSize)
            {
                outputStream = outputStream.Substring(len - maxBufferSize);
            }
            OnPropertyChanged("OutputStream");
        }

        /// <summary>
        /// Prints a formatted diagnostic string to the output stream
        /// </summary>
        /// <param name="fmt">Format string</param>
        /// <param name="args">Format arguments</param>
        /// <remarks>Automatically flushes the output</remarks>
        public void OutputDiagnosticString(string fmt, params object[] args)
        {
            OutputDiagnosticString(true, fmt, args);
        }

        /// <summary>
        /// Prints a formatted diagnostic string to the output stream
        /// </summary>
        /// <param name="flush">True if you want to flush the output otherwise false</param>
        /// <param name="fmt">Format string</param>
        /// <param name="args">Format arguments</param>
        public void OutputDiagnosticString(bool flush, string fmt, params object[] args)
        {
            outputStream += string.Format(fmt, args);
            if (flush)
            {
                FlushOutput();
            }
        }
        #endregion // IDiagnosticSink Implementation

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
