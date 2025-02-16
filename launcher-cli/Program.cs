namespace launcher_cli;

using System.Diagnostics;
using System;
using common;

internal static class Program {
    [STAThread]
    public static void Main(string[] args) {
        try {
            _launchJavaConsoleApp(args);    
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void _launchJavaConsoleApp(string[] args) {
        try {
            Console.WriteLine("Launching JavaFX application...");
#pragma warning disable CS8602
            var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
#pragma warning restore CS8602
#pragma warning disable CS8604 
            var configPath       = Path.Combine(currentDirectory, "config.json");
#pragma warning restore CS8604
            Console.WriteLine($"Config path: {configPath}");
            
            // ex: encrypt C:\Users\denis\Documents\github\shadow\test-data\input.txt
            var launcher = new JavaLauncher(configPath, args);
            launcher.Launch();
        }
        catch (Exception ex) {
            throw new Exception($"Failed to launch JavaFX application: {ex.Message}", ex);
        }
    }
}
