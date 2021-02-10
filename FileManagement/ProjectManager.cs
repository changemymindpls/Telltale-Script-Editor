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
        private string projectFilePath;

        public ScriptEditorProject project;
        private FileTreeManager fileTreeManager;
        private EditorPanelManager editorPanelManager;

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
            this.projectFilePath = projectFilePath;

            var jsonText = File.ReadAllText(projectFilePath);

            if (!JsonConvert.DeserializeObject<JObject>(jsonText).IsValid(new JSchemaGenerator().Generate(typeof(ScriptEditorProject))))
            {
                throw new InvalidProjectException("This is not a valid Telltale Script Editor project.");
            }

            this.fileTreeManager = new FileTreeManager(mainWindow.ui_editor_projectTree_treeView, this.GetWorkingDirectory(), editorPanelManager);
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

            ProjectProperties newProject_properties = new ProjectProperties();
            newProject_properties.Author = projectAuthor;
            newProject_properties.Name = projectName;
            newProject_properties.Version = projectVersion;

            ToolProperties newProject_toolProperties = new ToolProperties();
            //newProject_toolProperties.Executable = ""; //game executable path
            //newProject_toolProperties.Game = 0; //game version number
            //newProject_toolProperties.Master_Priority = 0; //mod archive master priority

            JsonProperties newProject_jsonProperties = new JsonProperties();
            //newProject_jsonProperties.Version = ""; //tse version

            newProject.Project = newProject_properties;
            newProject.Tool = newProject_toolProperties;
            newProject.Tseproj = newProject_jsonProperties;

            this.projectFilePath = projectFilePath;

            this.project = newProject;

            ProjectFile_WriteToFile(projectFilePath);

            this.fileTreeManager = new FileTreeManager(mainWindow.ui_editor_projectTree_treeView, this.GetWorkingDirectory(), editorPanelManager);
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
                throw new NotImplementedException();

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
        /// <param name="x">The resource to extract</param>
        /// <returns>The requested resource as a byte array.</returns>
        private static byte[] ExtractResource(string x)
        {
            Assembly a = Assembly.GetExecutingAssembly();

            using (Stream resFilestream = a.GetManifestResourceStream(x))
            {
                if (resFilestream == null)
                {
                    return null;
                }

                byte[] ba = new byte[resFilestream.Length];

                resFilestream.Read(ba, 0, ba.Length);

                return ba;
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
