# Java Launcher

## Description

Java applications cannot natively be compiled into executable files.

While `JPackage` offers a solution, it often falls short, especially when developing graphical applications (e.g., with JavaFX).
This project provides a simple way to assign an executable to your Java application, whether it's a console-based or graphical application.

* [launcher-cli.exe](cli): Serves as the launcher for Java-based console applications.
* [launcher-gui.exe](cli): Designed to launch Java-based graphical user interface applications.

## How does it work ?

The launcher is an executable that constructs a command line to start the Java application based on a configuration file (which must be named `config.json`).
This configuration file specifies the parameters required to build the command line. The key parameters are as follows:

| **Parameter**    | **Description**                                                                                                                                                            |
|------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| JavaHomePath     | Specifies the path to the directory containing the JVM required to run the Java application. The launcher will look for the `java.exe` executable within the `bin` folder. |
| ModulesPaths     | Defines the paths to the Java modules necessary for the application.                                                                                                       |
| Modules          | Lists the Java modules utilized by the application.                                                                                                                        |
| ClassPaths       | Indicates the directories and JAR files where the JVM can locate the classes and resources required by the application.                                                    |
| MainClass        | Represents the main class serving as the entry point for the Java application.                                                                                             |

Using these parameters, the launcher can assemble and execute the command line needed to start the Java application.
The configuration file uses the JSON format. Here is an example:

```json
{
  "JavaHomePath": "jdk-23",
  "ModulesPaths": [ "modules\\javafx-base-23-win.jar",
    "modules\\javafx-base-23.jar",
    "modules\\javafx-controls-23-win.jar",
    "modules\\javafx-controls-23.jar",
    "modules\\javafx-graphics-23-win.jar",
    "modules\\javafx-graphics-23.jar" ],
  "Modules": [ "javafx.base",
    "javafx.controls",
    "javafx.graphics" ],
  "ClassPaths": [ "classes", "classes\\annotations-26.0.1.jar" ],
  "MainClass": "org.shadow.skriva.Main"
}
```

> The above configuration is used for launching a Java application developed with JavaFX.

A typical directory structure for this configuration might look like:

```
app_dir
   ¦   config.json
   ¦   java-launcher.exe
   ¦   
   +---classes
   ¦   ¦   annotations-26.0.1.jar
   ¦   ¦   
   ¦   +---css
   ¦   ¦      app.css
   ¦   ¦      modern-alert.css
   ¦   ¦      modern-success.css
   ¦   ¦      modern-yes-no.css
   ¦   ¦       
   ¦   +---icons
   ¦   ¦      app16x16.png
   ¦   ¦       
   ¦   +---images
   ¦   ¦      warning.png
   ¦   ¦       
   ¦   +---org
   ¦       +---shadow
   ¦           +---skriva
   ¦                  Main.class
   ¦                           
   +---jdk-23
   ¦   +---bin
   ¦          java.exe
   ¦          
   +---modules
          javafx-base-23-win.jar
          javafx-base-23.jar
          javafx-controls-23-win.jar
          javafx-controls-23.jar
          javafx-graphics-23-win.jar
          javafx-graphics-23.jar
```

**Key Notes**:
- The launcher executable must reside in the same directory as its configuration file (`config.json`).
- If the configuration file includes absolute paths, these will be used as-is. However, if it contains relative 
  paths, they will be interpreted relative to the directory containing the configuration file.

## Packaging a Java application

You can use the provided Python script: [tools/build-config.py](tools/build-config.py).

Procedure:

1. Go to the project that implements the JAVA application to package. We assume that the project uses Maven.
2. Execute the following Maven command: `mvn -X clean javafx:run -Dargs="encrypt /path/to/file"`
3. Locate and copy the lines starting with "`[DEBUG] Executing command line: `" from the command output.
4. Assign this line as the value of the `MVN_SPEC` variable.
5. Run this script.

The script generates a "`launcher`" directory, which houses nearly all the necessary files for the Java application launcher:
- The configuration file, "`config.json`".
- The "`classes`" directory, containing all `.jar` and `.class` files, as well as resources (e.g., images, style sheets) that make up the application.
- The "`modules`" directory, which includes all the Java modules required by the application.

Then:
1. Create a subdirectory within the "`launcher`" directory to store the JVM.
2. Update the "`config.json`" configuration file by specifying a value for the "`JavaHomePath`" entry.
   Keep in mind that the launcher will look for the `java.exe` executable within the `bin` folder under
   the directory specified by the "`JavaHomePath`" entry.  
