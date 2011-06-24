using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace TDSMBasicPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public static class ItemSerializer
    {
        /// <summary>
        /// Objects to json.
        /// </summary>
        /// <param name="Object">The object.</param>
        /// <param name="ExportFile">The export file.</param>
        /// <returns></returns>
        public static string ObjectToJson(object Object, string ExportFile)
        {
            string sJson = JsonConvert.SerializeObject(Object, Formatting.Indented);

            if (File.Exists(ExportFile)) File.Delete(ExportFile);

            using (StreamWriter oOutFile = new StreamWriter(ExportFile))
            {
                oOutFile.Write(sJson);
            }

            return ExportFile;
        }
    }
}
