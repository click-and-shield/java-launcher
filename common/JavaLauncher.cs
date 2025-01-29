using System.Runtime.InteropServices;

namespace common;

using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// A utility class for configuring and launching Java applications from a .NET environment.
/// </summary>
public class JavaLauncher
{
    private readonly string    _javaHomePath;
    private readonly string    _mainClass;
    private readonly string[]  _classPaths;
    private readonly string[]? _arguments;
    private readonly string[]? _modulesPaths;
    private readonly string[]? _modules;

    /// <summary>
    /// A utility class for configuring and launching Java applications with optional support for JavaFX and modular builds.
    /// </summary>
    /// <param name="javaHomePath">The path to the directory that contains the JRE.</param>
    /// <param name="mainClass">The fully qualified name of the main class to launch.</param>
    /// <param name="classPaths">The paths to the class files to include in the classpath.</param>
    /// <param name="arguments">Optional arguments to pass to the application.</param>
    /// <param name="modulesPaths">Specifies the root modules to resolve in addition to the initial module
    ///             (ex. "`javafx.base`", "`javafx.controls`"...).</param>
    /// <param name="modules">Specifies where to find application modules (cf. key "`ModulesPaths`") with
    ///             a list of path elements. The elements of a module path can be a file path to a module or a
    ///             directory containing modules. Each module is either a modular JAR or an exploded-module directory.</param>
    /// <exception cref="Exception">Thrown when the Java installation directory is not found.</exception>
    public JavaLauncher(string javaHomePath,
                        string mainClass,
                        string[] classPaths, 
                        string[]? arguments = null, 
                        string[]? modulesPaths = null, 
                        string[]? modules = null) {
        _javaHomePath = javaHomePath;
        _mainClass    = mainClass;
        _classPaths   = classPaths;
        _arguments    = arguments;
        _modulesPaths = modulesPaths;
        _modules      = modules;
    }

    /// <summary>
    /// A utility class for configuring and launching Java applications from a .NET environment.
    /// </summary>
    /// <param name="configPath">Path to a YAML formated file that contains the configuration for the launcher.</param>
    /// <param name="arguments">Parameters passed to the application through the command line.</param> 
    /// <exception cref="Exception">Thrown if input data is invalid or required Java location paths are missing.</exception>
    public JavaLauncher(string configPath, string[]? arguments) {
        var config = _loadConfig(configPath)!;

        _javaHomePath = config.JavaHomePath;
        _mainClass    = config.MainClass;
        _classPaths   = config.ClassPaths;
        _arguments    = arguments;
        _modulesPaths = config.ModulesPaths;
        _modules      = config.Modules;
    }

    /// <summary>
    /// Configures the required environment variables for running a Java application, including updating the PATH to include the Java executable directory,
    /// setting the JAVA_HOME environment variable, and conditionally setting JavaFX-specific environment variables if they are provided.
    /// </summary>
    /// <exception cref="Exception">Thrown if there are issues setting environment variables.</exception>
    private void _setEnvironment() {
        var javaExeDir  = Path.Combine(_javaHomePath, "bin");
        var actualPaths = Environment.GetEnvironmentVariable("PATH");

        // Add the java executable directory to the PATH environment variable.
        if (actualPaths is not null) {
            var paths = actualPaths.Split(GetPathSeparator());
            if (!paths.Contains(javaExeDir)) {
                Environment.SetEnvironmentVariable("PATH", $"{javaExeDir}{GetPathSeparator()}{actualPaths}", EnvironmentVariableTarget.Process);
            }
        }
        else {
            Environment.SetEnvironmentVariable("PATH", javaExeDir, EnvironmentVariableTarget.Process);
        }

        // Set the JAVA_HOME and PATH_TO_FX environment variables.
        Environment.SetEnvironmentVariable("JAVE_HOME", _javaHomePath, EnvironmentVariableTarget.Process);
    }
    
    /// <summary>
    /// Loads and deserializes a configuration file containing settings for launching a Java application.
    /// </summary>
    /// <param name="configPath">The file path to the YAML configuration file.</param>
    /// <param name="rootPath">If the configuration file contains relative paths, then these paths will be prefixed by this path.</param>
    /// <returns>A populated <c>Config</c> object containing the deserialized configuration values.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified configuration file does not exist.</exception>
    /// <exception cref="YamlDotNet.Core.YamlException">Thrown when there is an error parsing the YAML configuration file.</exception>
    private static JavaLauncherConfig? _loadConfig(string configPath, string? rootPath = null) {
        try {
            var json= File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<JavaLauncherConfig>(json);
            if (config is null) {
                throw new Exception($"Failed to deserialize configuration file \"{configPath}\"");
            }
            config.Check();
            // If the configuration file contains relative paths, then these paths will be prefixed by the root path (if provided)
            if (rootPath is not null) {
                config.PrefixRootPath(rootPath);    
            }
            return config;
        }
        catch (Exception e) {
            throw new Exception($"Failed to load configuration file \"{configPath}\": {e.Message}", e);
        }
    }
    
    /// <summary>
    /// Launches a Java application with the specified class, class paths, optional arguments, and module configurations.
    /// </summary>
    /// <exception cref="Exception">Thrown when the application fails to launch or encounters an error during execution.</exception>
    public void Launch() {
        _setEnvironment();
        
        const string quote = "\"";
        string[] args = [ "-classpath", quote + string.Join(";", _classPaths) + quote ];
        if (_modulesPaths is not null) {
            args = args.Concat([ "--module-path", quote + string.Join(";", _modulesPaths) + quote ]).ToArray();
        }
        if (_modules is not null) {
            args = args.Concat([ "--add-modules", quote + string.Join(",", _modules) + quote ]).ToArray();
        }
        args = args.Append(_mainClass).ToArray();
        if (_arguments is not null) {
            args = args.Concat(_arguments.Select(x => quote + x + quote)).ToArray();
        }

        try {
            var processStartInfo = new ProcessStartInfo
                                   {
                                   FileName               = "java",
                                   Arguments              = string.Join(" ", args),
                                   RedirectStandardOutput = false,
                                   RedirectStandardError  = false,
                                   UseShellExecute        = false, // we do not want I/O to be redirected
                                   CreateNoWindow         = true   // we do not want a console window to be created
                                   };
            using var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        }
        catch (Exception e) {
            throw new Exception($"Failed to launch Java application: {e.Message}", e);
        }
    }
    
    private static string GetPathSeparator() {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";
    }
}
