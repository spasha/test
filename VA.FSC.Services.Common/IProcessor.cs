using System;

namespace VA.FSC.Services
{
    /// <summary>
    /// Processor Interface
    /// </summary>
    public interface IProcessor : IDisposable
    {
        /// <summary>
        /// an event to bubble out of this class
        /// </summary>
        event EventHandler<ProcessorEventArgs> ProcessingEvent;

        /// <summary>
        /// the name used for application in settings, process in logging
        /// </summary>
        string Application { get; }

        /// <summary>
        /// main entry point
        /// </summary>
        void RunProcess();
    }

}
