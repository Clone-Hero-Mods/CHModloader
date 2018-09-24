using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CHModloader
{
    public class ModLogs : MonoBehaviour
    {
        public static string LogsPath = ModLoader.ModsPath + "log.txt";

        public static void EnableLogs()
        {
            if (File.Exists(LogsPath))
            {
                File.Delete(LogsPath);
            }
        }

        public static void Log(string logString)
        {
            //ModConsole console = (ModConsole) ModLoader.LoadedMods.Find(mod => mod.ID == "ModConsole");

            File.AppendAllText(LogsPath, logString + Environment.NewLine);
            //console.logs += logString + Environment.NewLine;
        }
    }
}
