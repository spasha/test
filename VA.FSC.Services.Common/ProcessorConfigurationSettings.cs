using System;

namespace VA.FSC.Services
{
    /// <summary>
    /// a container for settings values
    /// </summary>
    public class ProcessorConfigurationSettings : IDisposable
    {
        /// <summary>
        /// the config.settings table ApplicationName value
        /// </summary>
        public string Application { get; set; }
       
        /// <summary>
        /// database connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationName">string</param>
        /// <param name="processorConnectionString">string</param>
        public ProcessorConfigurationSettings(string applicationName, string processorConnectionString = "")
        {
            this.ConnectionString = processorConnectionString;
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
