using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Telltale_Script_Editor.FileManagement
{
    public class ProjectManager
    {
        private FileTreeManager ftManager;
        private string pFilePath;

        /// <summary>
        /// Manages opened projects.
        /// </summary>
        /// <param name="x">The location of the project file to manage</param>
        /// <param name="y">The TreeView to display the project structure on.</param>
        public ProjectManager(string x, TreeView y)
        {
            this.pFilePath = x;
            this.ftManager = new FileTreeManager(y, this.GetWorkingDirectory());
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
    }
}
