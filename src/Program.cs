using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

namespace globalwaves.Player
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Gui.PlayerInterface());
        }
    }

    public static class ApplicationInformation
    {
        public static Assembly Assembly { get { return Assembly.GetExecutingAssembly(); }}
        public static AssemblyName AssemblyName { get { return Assembly.GetName(); } }
        public static Version Version { get { return AssemblyName.Version; } }
        public static ProcessorArchitecture ProcessorArchitecture { get { return AssemblyName.ProcessorArchitecture; } }
    }
}
