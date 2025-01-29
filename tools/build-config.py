# This script generates the JSON configuration file and the resources directory required by the Java launcher,
# based on the output of the following Maven command:
#
#       mvn -X clean javafx:run -Dargs="encrypt /path/to/file"
#
# Procedure:
#
#       1. Execute the Maven command provided above.
#       2. Locate and copy the line starting with "[DEBUG] Executing command line: " from the command output.
#       3. Assign this line as the value of the `MVN_SPEC` variable.
#       4. Run this script.

import re
import os
import json
import shutil
from pprint import pprint
from typing import List, Final, Pattern, Match, Optional, Dict, Union

### Configuration Beginning ###
VERBOSE: Final[bool] = False
TARGET_DIR: Final[str] = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'launcher')
MVN_SPEC: Final[str] = '''
[DEBUG] Executing command line: [C:\\Users\\denis\\Documents\\java\\jdk-23\\bin\\java.exe, --module-path, C:\\Users\\denis\\.m2\\repository\\org\\openjfx\\javafx-base\\23\\javafx-base-23-win.jar;C:\\Users\\denis\\.m2\\repository\\org\\openjfx\\javafx-
base\\23\\javafx-base-23.jar;C:\\Users\\denis\\.m2\\repository\\org\\openjfx\\javafx-controls\\23\\javafx-controls-23-win.jar;C:\\Users\\denis\\.m2\\repository\\org\\openjfx\\javafx-controls\\23\\javafx-controls-23.jar;C:\\Users\\denis\\.m2\\repository
\\org\\openjfx\\javafx-graphics\\23\\javafx-graphics-23-win.jar;C:\\Users\\denis\\.m2\\repository\\org\\openjfx\\javafx-graphics\\23\\javafx-graphics-23.jar, --add-modules, javafx.base,javafx.controls,javafx.graphics, -classpath, C:\\Users\\den
is\\Documents\\github\\click-and-crypt\\target\\classes;C:\\Users\\denis\\.m2\\repository\\commons-cli\\commons-cli\\1.9.0\\commons-cli-1.9.0.jar;C:\\Users\\denis\\.m2\\repository\\org\\jetbrains\\annotations\\26.0.2\\annotations-26.0.2.jar, org.shadow.click_and_crypt.Main, encrypt, C:\\Users\\denis\\Documents\\github\\click-and-crypt\\test-data\\input.txt]
'''
### Configuration End ###

MODULES_SUB_DIR: Final[str] = 'modules'
CLASSES_SUB_DIR: Final[str] = 'classes'

def get_classpath(text: str) -> List[str]:
    paths: List[str] = text.split(';')
    return paths

def get_module_path(text: str) -> List[str]:
    paths: List[str] = text.split(';')
    return paths

def get_modules(text: str) -> List[str]:
    modules: List[str] = text.split(',')
    return modules

# Extract the interesting part
text: str = MVN_SPEC.replace("\n", '')
regex: Pattern = re.compile(r"^\[DEBUG] Executing command line: \[(.+)]$")
m: Optional[Match] = regex.match(text)
if m is None:
    print("Invalid specification!")
    exit(1)
text = m.group(0)

regex_main_class: Pattern = re.compile(r"^(([a-z0-9_]+\.)*[a-z0-9_]+)$", re.IGNORECASE)
data: Dict[str, List[str]] = {}
parts: List[str] = text.split(', ')
found_class: bool = False

for i in range(1, len(parts)):
    part = parts[i].strip()
    if '-classpath' == part:
        data['classpath'] = get_classpath(parts[i+1])
        i += 1
        continue
    if '--module-path' == part:
        data['module_path'] = get_module_path(parts[i+1])
        i += 1
        continue
    if '--add-modules' == part:
        data['modules'] = get_modules(parts[i+1])
        i += 1
        continue

    if not found_class:
        m: Optional[Match] = regex_main_class.match(parts[i])
        if m is not None:
            data['class'] = m.group(0)
            found_class = True
    else:
        data['arguments'] = parts[i:]
        break

# pprint(data)

config: Dict[str, Union[List[str], str, None]] = {
    'ModulesPaths': [],
    'Modules': data['modules'],
    'ClassPaths': [],
    'MainClass': data['class'],
    'JavaHomePath': None
}

if os.path.exists(TARGET_DIR):
    shutil.rmtree(TARGET_DIR)
os.mkdir(TARGET_DIR)
modules_paths: str = os.path.join(TARGET_DIR, MODULES_SUB_DIR)
classes_paths: str = os.path.join(TARGET_DIR, CLASSES_SUB_DIR)
os.mkdir(os.path.join(modules_paths))
os.mkdir(os.path.join(classes_paths))

# Create the target directory
for path in data['module_path']:
    if not os.path.exists(path):
        print("The module {} does not exist!".format(path))
        exit(1)
    target_file: str = os.path.join(modules_paths, os.path.basename(path))
    if VERBOSE:
        print("Module: copy \"{}\" to \"{}\"".format(path, target_file))
    shutil.copy(path, target_file)
    config['ModulesPaths'].append(os.path.join(MODULES_SUB_DIR, os.path.basename(path)))

for path in data['classpath']:
    if not os.path.exists(path):
        print("The class path {} does not exist!".format(path))
        exit(1)
    if os.path.isdir(path):
        if VERBOSE:
            print("ClassPath: copy \"{}\" to \"{}\"".format(path, classes_paths))
        shutil.copytree(path, classes_paths, dirs_exist_ok=True)
    else:
        target_file: str = os.path.join(classes_paths, os.path.basename(path))
        if VERBOSE:
            print("ClassPath: copy \"{}\" to \"{}\"".format(path, target_file))
        shutil.copy(path, target_file)
    config['ClassPaths'].append(os.path.join(CLASSES_SUB_DIR, os.path.basename(path)))

with open(os.path.join(TARGET_DIR, 'config.json'), 'w') as f:
    f.write(json.dumps(config, indent=4, sort_keys=True, ensure_ascii=False))

print("")
print(json.dumps(config, indent=4, sort_keys=True, ensure_ascii=False))






