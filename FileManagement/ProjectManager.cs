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
        private string pFilePath;

        public  ScriptEditorProject project;
        private FileTreeManager     ftManager;
        private EditorPanelManager  pManager;

        /// <summary>
        /// Manages opened projects.
        /// </summary>
        /// <param name="x">The location of the project file to manage</param>
        /// <param name="y">The TreeView to display the project structure on.</param>
        /// <param name="z">The TextEditor to display text in.</param>
        public ProjectManager(string x, TreeView y, EditorPanelManager z)
        {
            this.pFilePath = x;
            var jsonText = File.ReadAllText(pFilePath);
            if (!JsonConvert.DeserializeObject<JObject>(jsonText).IsValid(new JSchemaGenerator().Generate(typeof(ScriptEditorProject))))
            {
                throw new InvalidProjectException("This is not a valid Telltale Script Editor project.");
            }
            this.ftManager = new FileTreeManager(y, this.GetWorkingDirectory(), z);
            this.pManager = z;
            this.project = JsonConvert.DeserializeObject<ScriptEditorProject>(jsonText);
        }

        /// <summary>
        /// Gets the working directory of the project.
        /// </summary>
        /// <returns>The current working directory</returns>
        public string GetWorkingDirectory()
        {
            return Path.GetDirectoryName(this.pFilePath);
        }

        /// <summary>
        /// Gets the path of the project file.
        /// </summary>
        /// <returns>The directory of the currently open project file</returns>
        public string GetProjectFilePath()
        {
            return pFilePath;
        }

        /// <summary>
        /// Builds the project
        /// </summary>
        /// <param name="x">Run Game After Build?</param>
        public void BuildProject(bool x = false)
        {
            if (x)
                throw new NotImplementedException();

            string y = Path.GetTempFileName();
            File.WriteAllBytes(y, ExtractResource("Telltale_Script_Editor.Resources.ttarchext.exe"));

            Process ttext = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = y,
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
                File.Delete(y);
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
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        internal void Destroy()
        {
            if (pManager != null && !pManager.Destroy())
                return;

            if (ftManager != null)
                ftManager.Destroy();
        }
    }
}
