/*==========================================================================;
 *
 *  (c) Sowa Labs. All rights reserved.
 *
 *  File:    Config.js
 *  Desc:    Configuration settings
 *  Created: Jun-2013
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using LUtils
    = Latino.Utils;

namespace DocumentViewer
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Config
       |
       '-----------------------------------------------------------------------
    */
    public static class Config
    {
        public static readonly string ConnectionString
            = LUtils.GetConfigValue<string>("ConnectionString");
        public static readonly int CommandTimeout
            = LUtils.GetConfigValue<int>("CommandTimeout", "0");
    }
}