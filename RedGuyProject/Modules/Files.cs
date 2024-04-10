﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace RedGuyMod.Modules
{
    internal static class Files
    {

        //public static SubFileSystem fileSystem;

        public static PluginInfo PluginInfo;

        internal static string assemblyDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(PluginInfo.Location);
            }
        }

        internal static void Init(PluginInfo info)
        {
            PluginInfo = info;
        }

        internal static string GetPathToFile(string folderName, string fileName)
        {
            return Path.Combine(assemblyDir, folderName, fileName);
        }
    }
}
