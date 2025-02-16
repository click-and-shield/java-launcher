namespace launcher_gui;
using System.Diagnostics;
using System.IO;
using System;
using common;


internal static class Program
{
    private static string? CurrentDirectory { get; set; }
    
    [STAThread]
    public static void Main(string[] args)
    {
        try {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _launchJavaFxApp(args);
        }
        catch (Exception e) {
            var errorPath = Path.Combine(CurrentDirectory, "error.log");
            File.WriteAllText(errorPath, e.Message);
            Console.WriteLine(e);
        }
        Application.Exit();
    }
    
    private static void _launchJavaFxApp(string[] args)
    {
        try
        {
#pragma warning disable CS8602
            CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
#pragma warning restore CS8602
#pragma warning disable
            var configPath = Path.Combine(CurrentDirectory, "config.json");
            // Check that the configuration file exists.
            if (!File.Exists(configPath)) {
                throw new Exception($"Configuration file not found: \"{configPath}\n");
            }
           
            var launcher = new JavaLauncher(configPath, args);
            launcher.Launch();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to launch JavaFX application: {ex.Message}", ex);
        }
    }
}