# Dependency-Analyzer
The philosophy behind the project is that, less number of dependencies and strong components the project files have, less the number of files that require to be updated when a single file is changed.

Features:
- Contains a GUI interface that communicates with a server process to get information about files and for doing the dependency analysis
- Uses a robust Parser for understanding C# code and can be easily changed to work with other programming languages
- Along with the dependency it also uses Tarjan's algorithm for strong component detection
- Stores all the results in text files and displays on the GUI interface
- Contains a Project OCD about the final project along with Package Diagram, Class Diagram and Activity Diagram. 
- Constains compile.bat and run.bat to compile and run the project (must be run with Administrator priviledges)
