using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.FSC.Services.Common.Enums;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VA.FSC.Services.Common.Helpers
{
    /// <summary>
    /// Extension methods used on common objects.
    /// </summary>
    public static class HelperMethods
    {
        public static bool Parse277Json(string jsonBody, out string claimId, out string timeStamp)
        {
            claimId = string.Empty;
            timeStamp = string.Empty;
            try
            {
				//string unescapedInput = System.Text.RegularExpressions.Regex.Unescape(jsonBody);
				string jsonString = JToken.Parse(jsonBody).ToString();
                JObject parsedJson = JObject.Parse(jsonString);
                claimId = parsedJson["meta"]["extension"][0]["valueString"].ToString();
                timeStamp = parsedJson["extension"][0]["valueString"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Converts the System.Diagnostics.EventLogEntryType value to VA.FSC.Logging.ErrorLevel
        /// </summary>
        /// <param name="value">System.Diagnostics.EventLogEntryType</param>
        /// <returns>VA.FSC.Logging.ErrorLevel</returns>
        public static ErrorLevel ConvertToVafscLevel(this System.Diagnostics.Tracing.EventLevel value)
        {
            switch (value)
            {
                case System.Diagnostics.Tracing.EventLevel.Critical:
                    return ErrorLevel.Critical;
                case System.Diagnostics.Tracing.EventLevel.Error:
                    return ErrorLevel.Error;
                case System.Diagnostics.Tracing.EventLevel.Informational:
                    return ErrorLevel.Information;
                case System.Diagnostics.Tracing.EventLevel.Warning:
                    return ErrorLevel.Warning;
                case System.Diagnostics.Tracing.EventLevel.LogAlways:
                case System.Diagnostics.Tracing.EventLevel.Verbose:

                default:
                    return ErrorLevel.Verbose;
            }
        }
    }
}
