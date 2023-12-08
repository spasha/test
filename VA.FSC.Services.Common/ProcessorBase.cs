using System;
using log4net;

namespace VA.FSC.Services
{
    /// <summary>
    /// Processor Base Class
    /// </summary>
    public abstract class ProcessorBase : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessorBase));
        /// <summary>
        /// configuration
        /// </summary>
        public ProcessorConfigurationSettings ProcessorConfigurationSettings { get; private set; }
        
        //public Logger Logger { get; private set; }

        /// <summary>
        /// instantiates a new instance of this class
        /// </summary>
     
        /// <param name="configConnectionString">string</param>
        /// <param name="processingConnectionString">string</param>
        public ProcessorBase(string configConnectionString, string processingConnectionString = "")
        {
            this.ProcessorConfigurationSettings = new ProcessorConfigurationSettings(((IProcessor)this).Application, processingConnectionString);
          
        }

        
        /// <summary>
        /// Dispose of this class
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                this.ProcessorConfigurationSettings.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Error Disposing of Settings", ex);
            }
        }

    }

}
