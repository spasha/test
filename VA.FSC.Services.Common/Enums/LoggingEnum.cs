using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.Enums
{
    public enum ErrorLevel
    {
        /// <summary>
        /// Use when the information is for reference only
        /// </summary>
        Verbose = 0,
        /// <summary>
        /// Use when the information represents a milestone in the process
        /// </summary>
        Information = 1,
        /// <summary>
        /// Use when an unexpected event (such as Exception) occurs that does not interrupt or break the process
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Use whenever an error occurs that interrupts or breaks the process
        /// </summary>
        Error = 3,
        /// <summary>
        /// Use whenever an error occurs that prevents a process from running
        /// </summary>
        Critical = 4,
        /// <summary>
        /// Use whenever an error occurs that breaks to root process, requiring restart or immediate attention
        /// </summary>
        Fatal = 5
    }
}
