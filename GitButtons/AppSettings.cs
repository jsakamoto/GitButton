﻿using System.Configuration;

namespace GitButtons
{
    [System.Diagnostics.DebuggerNonUserCodeAttribute]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    public static class AppSettings
    {
        public static string Port
        {
            get { return ConfigurationManager.AppSettings["port"]; }
        }
    }
}

