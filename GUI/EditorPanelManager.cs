using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using Telltale_Script_Editor.FileManagement;
using Telltale_Script_Editor.GUI;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.GUI
{
    public class EditorPanelManager
    {
        private MainWindow mainWindow;
        private IOManagement ioManagement;
        private MessageBoxes messageBoxes;

        public ProjectManager projectManager;

        private string currentlyOpenFile;

        /// <summary>
        /// Helps with the management of the multiple editor panels.
        /// </summary>
        public EditorPanelManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            ioManagement = new IOManagement();
            messageBoxes = new MessageBoxes();
        }

        public void OpenProject()
        {
            //checks if another project is open & prompts the user on whether or not to continue.
            if (projectManager != null)
            {
                if (!messageBoxes.Warning_Confirm("Are you sure?", "Are you sure you'd like to continue? Unsaved changes will be lost."))
                    return;
            }

            //temp variable for file path
            string filePath = "";

            //open file browser dialog
            ioManagement.GetFilePath(ref filePath, "Telltale Script Editor Project (*.tseproj)|*.tseproj", "Open a Telltale Script Editor Project");

            //if the user cancled the operation, path will be null
            if (string.IsNullOrEmpty(filePath))
                return;

            Console.WriteLine($"Selected project file at {filePath}");

            if (projectManager != null)
                projectManager.Destroy();

            try
            {
                projectManager = new ProjectManager(filePath, mainWindow);
            }
            catch (InvalidProjectException e)
            {
                messageBoxes.Error("Error!", e.Message);

                projectManager = null;

                return;
            }

            mainWindow.ui_editor_welcomePanel.Visibility = Visibility.Hidden;
        }

        public void SaveProject()
        {
            projectManager.ProjectFile_WriteToFile(projectManager.GetProjectFilePath());
        }

        public void Import()
        {
            //temp variable for file path
            string filePath = "";

            //open file browser dialog
            ioManagement.GetFilePath(ref filePath, "Import a File");

            //if the user cancled the operation, path will be null
            if (string.IsNullOrEmpty(filePath))
                return;
        }

        /// <summary>
        /// Set syntax highlighting
        /// </summary>
        /// <param name="x">The syntax highlighting definition to load</param>
        public void SetSyntaxHighlighting(string x = null)
        {
            if (x == null)
            {
                mainWindow.ui_editor_textEditor.SyntaxHighlighting = null;

                return;
            }

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Telltale_Script_Editor.Resources.{x}.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    mainWindow.ui_editor_textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Opens & displays a file in the editor as text.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public bool OpenTextFile(string x)
        {
            if (!ResetEditorView())
                return false;

            currentlyOpenFile = x;
            mainWindow.ui_editor_textEditor.Text = File.ReadAllText(x);
            mainWindow.ui_editor_textEditor.IsModified = false;
            mainWindow.ui_editor_textEditor.IsEnabled = true;
            mainWindow.ui_editor_textEditor.Visibility = Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Opens & displays a DDS image file in the editor.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public bool OpenImageFile(string x)
        {
            if (!ResetEditorView())
                return false;

            mainWindow.ui_editor_imageViewer_image.Source = new BitmapImage(new Uri(x));

            mainWindow.ui_editor_imageViewer.IsEnabled = true;
            mainWindow.ui_editor_imageViewer.Visibility = Visibility.Visible;

            return true;
        }

        /// <summary>
        /// Displays the editor for .tseproj files.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public void OpenProjectFile(string x)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes / disables all editor panels.
        /// </summary>
        private bool ResetEditorView()
        {
            if(mainWindow.ui_editor_textEditor.IsEnabled && mainWindow.ui_editor_textEditor.IsModified && currentlyOpenFile != null)
            {
                var x = MessageBox.Show("Would you like to save your changes?", "Save?", MessageBoxButton.YesNoCancel);
                if (x == MessageBoxResult.Yes)
                    File.WriteAllText(currentlyOpenFile, mainWindow.ui_editor_textEditor.Text);
                else if (x == MessageBoxResult.Cancel)
                    return false;
            }

            mainWindow.ui_editor_textEditor.IsEnabled = false;
            mainWindow.ui_editor_textEditor.Visibility = Visibility.Hidden;
            mainWindow.ui_editor_textEditor.IsModified = false;

            mainWindow.ui_editor_imageViewer.IsEnabled = false;
            mainWindow.ui_editor_imageViewer.Visibility = Visibility.Hidden;
            mainWindow.ui_editor_imageViewer_image.Source = null;
            
            currentlyOpenFile = null;

            return true;
        }

        public bool Destroy()
        {
            return ResetEditorView();
        }
    }
}
