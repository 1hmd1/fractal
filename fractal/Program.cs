using System;
using System.IO;
using System.Windows.Forms;
using Python.Runtime;

namespace FractalApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            string pyDll = @"D:\Python\Python311\python311.dll"; 
            if (!File.Exists(pyDll))
            {
                MessageBox.Show("Файл DLL не найден: " + pyDll);
                return;
            }

            Runtime.PythonDLL = pyDll;

            PythonEngine.Initialize();  

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());

            PythonEngine.Shutdown();
        }
    }
}
