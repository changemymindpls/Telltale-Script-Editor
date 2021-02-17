using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telltale_Script_Editor.GUI;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.FileManagement
{
    public class ProjectManager
    {
        //main project file path
        private string projectFilePath;

        //main public objects
        public ScriptEditorProject project;
        public FileTreeManager fileTreeManager;
        public EditorPanelManager editorPanelManager;
        
        //utillity objects
        private IOManagement ioManagement;
        private MessageBoxes messageBoxes;

        /// <summary>
        /// Opens a Project
        /// </summary>
        /// <param name="projectFilePath">The location of the project file to manage</param>
        /// <param name="treeView">The TreeView to display the project structure on.</param>
        /// <param name="editorPanelManager">The TextEditor to display text in.</param>
        public ProjectManager(string projectFilePath, MainWindow mainWindow)
        {
            //get our utility objects ready since we will use them right away
            ioManagement = new IOManagement();
            messageBoxes = new MessageBoxes();

            //get project file path
            this.projectFilePath = projectFilePath;

            //parse the json data from the text file
            var jsonText = File.ReadAllText(projectFilePath);

            //deseralize the object and parse the data
            if (!JsonConvert.DeserializeObject<JObject>(jsonText).IsValid(new JSchemaGenerator().Generate(typeof(ScriptEditorProject))))
            {
                //if the project fails to get parsed, throw an error message
                messageBoxes.Error("Can't open project!", "This is not a valid Telltale Script Editor project.");
            }

            //assign our main public objects
            this.fileTreeManager = new FileTreeManager(mainWindow.ui_editor_projectTree_treeView, this.GetWorkingDirectory(), mainWindow.editorPanelManager);
            this.editorPanelManager = mainWindow.editorPanelManager;
            this.project = JsonConvert.DeserializeObject<ScriptEditorProject>(jsonText);
        }

        /// <summary>
        /// Creates a new Project
        /// </summary>
        public ProjectManager(MainWindow mainWindow, string projectFilePath, string projectName, string projectAuthor, string projectVersion, int gameVersion)
        {
            //generate a new project
            ScriptEditorProject newProject = new ScriptEditorProject();

            //initalize our project properties
            ProjectProperties newProject_properties = new ProjectProperties();
            newProject_properties.Author = projectAuthor;
            newProject_properties.Name = projectName;
            newProject_properties.Version = projectVersion;

            //initalize our project tool properties
            ToolProperties newProject_toolProperties = new ToolProperties();
            newProject_toolProperties.Game = gameVersion; //game version number
            newProject_toolProperties.Master_Priority = 950; //mod archive master priority

            //initalize our project json properties
            JsonProperties newProject_jsonProperties = new JsonProperties();
            //newProject_jsonProperties.Version = ""; //tse version

            //assign our project property objects to the main project
            newProject.Project = newProject_properties;
            newProject.Tool = newProject_toolProperties;
            newProject.Tseproj = newProject_jsonProperties;

            //get project file path
            this.projectFilePath = projectFilePath;

            //get project object
            this.project = newProject;

            //write the project ofile
            ProjectFile_WriteToFile(projectFilePath);

            //get our file tree manager
            this.fileTreeManager = new FileTreeManager(mainWindow.ui_editor_projectTree_treeView, this.GetWorkingDirectory(), editorPanelManager);

            //create our utillity objects
            ioManagement = new IOManagement();
            messageBoxes = new MessageBoxes();
        }

        public void ProjectFile_WriteToFile(string projectFilePath)
        {
            if (File.Exists(projectFilePath))
                ioManagement.DeleteFile(projectFilePath);

            //open a stream writer to create the text file and write to it
            using (StreamWriter file = File.CreateText(projectFilePath))
            {
                //get our json seralizer
                JsonSerializer serializer = new JsonSerializer();

                //seralize the data and write it to the configruation file
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, project);
            }
        }

        /// <summary>
        /// Gets the working directory of the project.
        /// </summary>
        /// <returns>The current working directory</returns>
        public string GetWorkingDirectory()
        {
            return Path.GetDirectoryName(projectFilePath);
        }

        /// <summary>
        /// Gets the path of the project file.
        /// </summary>
        /// <returns>The directory of the currently open project file</returns>
        public string GetProjectFilePath()
        {
            return projectFilePath;
        }

        /// <summary>
        /// Builds the project
        /// </summary>
        /// <param name="runGameAfterBuild">Run Game After Build?</param>
        public void BuildProject(bool runGameAfterBuild = false)
        {
            if (runGameAfterBuild)
            {
                //if they want to build and run the project but they don't have the executable path assigned
                if(string.IsNullOrEmpty(project.Tool.Executable) || !File.Exists(project.Tool.Executable))
                {
                    //tell them they have to assign it
                    messageBoxes.Error("No Executable Specified!", "You need to specify the Game Executable location in the Build Configuration window to use the Build and Run feature!");

                    //open the window up for them
                    editorPanelManager.Menu_OpenBuildConfig();

                    //don't continue
                    return;
                }
            }

            string temporaryFileName = Path.GetTempFileName();

            File.WriteAllBytes(temporaryFileName, ExtractResource("Telltale_Script_Editor.Resources.ttarchext.exe"));

            Process ttext = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = temporaryFileName,
                    Arguments = ""
                }
            };

            ttext.Start();

            while (!ttext.StandardOutput.EndOfStream)
            {
                Console.WriteLine(ttext.StandardOutput.ReadLine());
                System.Windows.Forms.Application.DoEvents();
            }

            ttext.WaitForExit();

            if (ttext.HasExited)
            {
                ioManagement.DeleteFile(temporaryFileName);
            }
        }

        /// <summary>
        /// Utility - Extracts Embedded Resources
        /// </summary>
        /// <returns>The requested resource as a byte array.</returns>
        private static byte[] ExtractResource(string resourceToExtract)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream resourceFilestream = assembly.GetManifestResourceStream(resourceToExtract))
            {
                if (resourceFilestream == null)
                {
                    return null;
                }

                byte[] resourceByteArray = new byte[resourceFilestream.Length];

                resourceFilestream.Read(resourceByteArray, 0, resourceByteArray.Length);

                return resourceByteArray;
            }
        }

        internal void Destroy()
        {
            if (editorPanelManager != null && !editorPanelManager.Destroy())
                return;

            if (fileTreeManager != null)
                fileTreeManager.Destroy();
        }
    }
}
