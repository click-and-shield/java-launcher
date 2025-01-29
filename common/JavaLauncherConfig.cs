namespace common;

/// <summary>
/// Represents the configuration required to launch a Java application.
/// This class encapsulates details about the Java environment, classpaths, modules, and the main class to be executed.
/// </summary>
public class JavaLauncherConfig
{
    public required string    JavaHomePath { get; set; }
    public          string[]? ModulesPaths { get; set; }
    public          string[]? Modules      { get; set; }
    public required string[]  ClassPaths   { get; set; }
    public required string    MainClass    { get; set; }
    
    private void _checkModules() {
        if (ModulesPaths is null) {
            return;
        }

        for (var i = 0; i < ModulesPaths.Length; i++) {
            if (ModulesPaths[i] is null) {
                throw new Exception($"Module #{i} is null!");
            }
        }
    }

    private void _checkAdditionalModules() {
        if (Modules is null) {
            return;
        }

        for (var i = 0; i < Modules.Length; i++) {
            if (Modules[i] is null) {
                throw new Exception($"Additional module #{i} is null!");
            }
        }
    }

    private void _checkClassPaths() {
        for (var i = 0; i < ClassPaths.Length; i++) {
            if (ClassPaths[i] is null) {
                throw new Exception($"Class path #{i} is null!");
            }
        }
    }

    /// <summary>
    /// Validates the configuration for launching a Java application.
    /// This method performs checks on all relevant properties such as modules, additional modules,
    /// and class paths to ensure none of them contain invalid or null values.
    /// Throws an exception if validation fails.
    /// </summary>
    public void Check() {
        _checkModules();
        _checkAdditionalModules();
        _checkClassPaths();
    }

    /// <summary>
    /// Updates all relevant paths in the configuration by prefixing them with the given root path
    /// if they are not already absolute. This ensures that relative paths are resolved correctly
    /// within the specified base directory.
    /// </summary>                 
    /// <param name="rootPath">
    /// The root directory path to be used as the base for resolving relative paths
    /// in the Java configuration.
    /// </param>
    public void PrefixRootPath(string rootPath) {
        if (!Path.IsPathRooted(JavaHomePath)) {
            JavaHomePath = Path.Combine(rootPath, JavaHomePath);
        }
    
        if (ModulesPaths is not null) {
            for (var i = 0; i < ModulesPaths.Length; i++) {
                if (!Path.IsPathRooted(ModulesPaths[i])) {
                    ModulesPaths[i] = Path.Combine(rootPath, ModulesPaths[i]);
                }
            }    
        }
    
        if (Modules is not null) {
            for (var i = 0; i < Modules.Length; i++) {
                if (!Path.IsPathRooted(Modules[i])) {
                    Modules[i] = Path.Combine(rootPath, Modules[i]);
                }
            }
        }
        
        for (var i = 0; i < ClassPaths.Length; i++) {
            if (!Path.IsPathRooted(ClassPaths[i])) {
                ClassPaths[i] = Path.Combine(rootPath, ClassPaths[i]);
            }
        }
    }
}