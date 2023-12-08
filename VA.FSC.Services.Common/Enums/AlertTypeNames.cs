
namespace VA.FSC.Services.Common.Enums
{
    public enum AlertTypeNames
    {
        /// <summary>
        /// Listener Startup
        /// </summary>
        RMQConnectionFailure = 0,

        /// <summary>
        /// Any RMQ Connection Issue
        /// </summary>
        DatabaseConnectionFailure = 1,

        /// <summary>
        /// F277 Database Update Issue
        /// </summary>
        F277DatabaseUpdateFailure = 2,

        /// <summary>
        /// SQL Data Integrity
        /// </summary>
        DataIntegrity = 3
    }
}
