using System;
using System.Text;
using VA.FSC.Services.Common.Enums;

namespace VA.FSC.Services
{
    /// <summary>
    /// a container for the processing event
    /// </summary>
    public class ProcessorEventArgs : EventArgs
    {
        /// <summary>
        /// additional string data
        /// </summary>
        public string Comment { get; set; }

        ///// <summary>
        /////  level
        ///// </summary>
        public ErrorLevel EventLevel { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// any payload if applicable
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// creates a new instance of this class
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="level">ErrorLevel</param>
        /// <param name="comment">string</param>
        /// <param name="payload">object</param>
        public ProcessorEventArgs(string message, ErrorLevel level = ErrorLevel.Information, string comment = "", object payload = null) : base()
        {
            this.Message = message;
            this.EventLevel = level;
            this.Comment = comment;
            this.Payload = payload;
        }

        /// <summary>
        /// creates a human readable output
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Message: " + this.Message);
            sb.AppendLine("EventLevel: 0");
            sb.AppendLine("Comment: " + this.Comment.ToString());
            sb.AppendFormat("Payload: {0}", this.Payload ?? "None");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
