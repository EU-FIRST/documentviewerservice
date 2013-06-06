/*==========================================================================;
 *
 *  (c) Sowa Labs. All rights reserved.
 *
 *  File:    Utils.js
 *  Desc:    Document viewer utilities
 *  Created: Jun-2013
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Data.SqlClient;
using Latino;

using LUtils
    = Latino.Utils;

namespace DocumentViewer
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Utils
       |
       '-----------------------------------------------------------------------
    */
    public static class Utils
    {
        public static string GetFileName(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT fileName FROM Documents WHERE id = @id", connection))
                {
                    cmd.CommandTimeout = Config.CommandTimeout;
                    cmd.AssignParams("id", id);
                    return (string)cmd.ExecuteScalarRetryOnDeadlock();
                }
            }
        }
    }
}