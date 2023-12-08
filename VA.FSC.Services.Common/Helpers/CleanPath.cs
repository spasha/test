using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Diagnostics;
using System.Data;
using System.IO;

namespace VA.FSC.Services.Common.Helpers
{
    public class CleanPath
    {
       public static String CleanString(String aString) 
       {
          if (aString == null) 
            {  return null; }

        String cleanString = "";

            for (int i = 0; i < aString.Length; ++i) 
            {
                cleanString += CleanChar(aString[i]);
            }
            return cleanString;
        }

        private static char CleanChar(char aChar) 
        {

            //--> 0 - 9
            for (int i = 48; i < 58; ++i) 
            {
                if (aChar == i) return (char) i;
            }

            //--> 'A' - 'Z'
            for (int i = 65; i < 91; ++i) 
            {
                if (aChar == i) return (char) i;
            }

            //--> 'a' - 'z'
            for (int i = 97; i < 123; ++i) 
            {
                if (aChar == i) return (char) i;
            }

            //--> other valid characters
           string cleanList = "/\\:.-_ ;=,?*$";
           char[] cleanChar = cleanList.ToCharArray();

           for (int i = 0; i < cleanChar.Length; i++) 
           { 
              if (aChar == cleanChar[i]) return (cleanChar[i]);
           }  
        
            return '%';
        }
    }
}
