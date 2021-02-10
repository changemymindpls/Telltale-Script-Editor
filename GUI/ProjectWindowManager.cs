using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telltale_Script_Editor.FileManagement;


namespace Telltale_Script_Editor.GUI
{
    public class ProjectWindowManager
    {
        private MainWindow mainWindow;
        private EditorPanelManager editorPanelManager;
        private ProjectManager projectManager;

        public ProjectWindowManager(MainWindow mainWindow, EditorPanelManager editorPanelManager, ProjectManager projectManager)
        {
            this.mainWindow = mainWindow;
            this.editorPanelManager = editorPanelManager;
            this.projectManager = projectManager;
        }
    }
}
