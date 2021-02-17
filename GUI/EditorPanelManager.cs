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
using Telltale_Script_Editor.TextureConvert;

namespace Telltale_Script_Editor.GUI
{
    public class EditorPanelManager
    {
        public readonly string appVersion = "v1.0.0";

        public MainWindow mainWindow;
        private IOManagement ioManagement;
        private MessageBoxes messageBoxes;
        private Converter converter;

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
            converter = new Converter();
        }

        //------------------------------------------- MAIN WINDOW COMMANDS START -------------------------------------------
        public void Menu_CreateNewFolder(string newFolderName)
        {
            string mewFolderPath = projectManager.GetWorkingDirectory() + "/" + newFolderName;

            if (Directory.Exists(mewFolderPath))
            {
                messageBoxes.Error("There is already a folder here with the same name!", "Error!");
                return;
            }

            ioManagement.CreateDirectory(mewFolderPath);

            projectManager.fileTreeManager.RepopulateFileTree();
        }

        public void Menu_OpenProject()
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

        public void Menu_PrintOutputToFile()
        {
            //temp variable for file path
            string filePath = "";

            //open file browser dialog
            ioManagement.GetFilePath(ref filePath, "Telltale Script Editor Project (*.tseproj)|*.tseproj", "Open a Telltale Script Editor Project");

            //if the user cancled the operation, path will be null
            if (string.IsNullOrEmpty(filePath))
                return;

            string directory = string.Format("{0}/EditorLogs", projectManager.GetWorkingDirectory());

            if(!Directory.Exists(directory))
                ioManagement.CreateDirectory(directory);

            string logPath = string.Format("{0}/TelltaleScriptEditor_ConsoleOutput{1}.txt", directory, DateTime.Now.ToString());

            File.WriteAllText(logPath, mainWindow.ui_editor_consoleOutput.Text);
        }

        public void Menu_SaveProject()
        {
            projectManager.ProjectFile_WriteToFile(projectManager.GetProjectFilePath());
        }

        public void Menu_SaveProjectAs()
        {
            
        }

        public void Menu_SaveCurrentFile()
        {
            if (currentlyOpenFile != null)
            {
                mainWindow.ui_editor_textEditor.IsModified = false;

                File.WriteAllText(currentlyOpenFile, mainWindow.ui_editor_textEditor.Document.Text);
            }
        }

        public void Menu_SaveCurrentFileAs(bool openSaveFile = true)
        {
            if (currentlyOpenFile != null)
            {
                string newFilePath = "";

                ioManagement.SaveFile(ref newFilePath, projectManager.GetWorkingDirectory(), "Save File As", Path.GetExtension(currentlyOpenFile));

                if (string.IsNullOrEmpty(newFilePath))
                    return;

                mainWindow.ui_editor_textEditor.IsModified = false;

                File.WriteAllText(newFilePath, mainWindow.ui_editor_textEditor.Document.Text);

                if(openSaveFile)
                {
                    currentlyOpenFile = newFilePath;
                    OpenTextFile(currentlyOpenFile);
                }

                projectManager.fileTreeManager.RepopulateFileTree();
            }
        }

        public void Menu_Texture_ConvertToDDS()
        {
            string messageBoxMessage = string.Format("Convert '{0}' to a DDS? (Will also generate a .header)", Path.GetFileName(currentlyOpenFile));

            if (messageBoxes.Warning_Confirm("Convert D3DTX to DDS", messageBoxMessage) == false)
                return;

            DisableImageViewer();

            converter.Convert_D3DTX_To_DDS(currentlyOpenFile, true);

            string convertedDDS_path = currentlyOpenFile.Replace(".d3dtx", ".dds");
            OpenImageFile(convertedDDS_path);

            projectManager.fileTreeManager.RepopulateFileTree();
        }

        public void Menu_Texture_ConvertToD3DTX()
        {
            string messageBoxMessage = string.Format("Convert '{0}' to a D3DTX?", Path.GetFileName(currentlyOpenFile));

            if (messageBoxes.Warning_Confirm("Convert D3DTX to DDS", messageBoxMessage) == false)
                return;

            string headerCompanionPath;
            string ddsTexturePath;

            if(Path.GetExtension(currentlyOpenFile).Equals(".header"))
            {
                headerCompanionPath = currentlyOpenFile;
                ddsTexturePath = currentlyOpenFile.Replace(".header", ".dds");
            }
            else
            {
                headerCompanionPath = currentlyOpenFile.Replace(".dds", ".header");
                ddsTexturePath = currentlyOpenFile;
            }

            if (File.Exists(headerCompanionPath) && File.Exists(ddsTexturePath))
            {
                DisableImageViewer();

                converter.Convert_DDS_To_D3DTX(ddsTexturePath, headerCompanionPath, true);

                projectManager.fileTreeManager.RepopulateFileTree();

                return;
            }
            else if (File.Exists(headerCompanionPath) == false)
            {
                string error_message = string.Format("Error! Can't convert '{0}' because it can't find the .header file for converting to .d3dtx!", Path.GetFileName(currentlyOpenFile));
                messageBoxes.Error("Can't Convert!", error_message);

                return;
            }
            else if (File.Exists(ddsTexturePath) == false)
            {
                string error_message = string.Format("Error! Can't convert '{0}' because it can't find the .dds texture file for converting to .d3dtx!", Path.GetFileName(currentlyOpenFile));
                messageBoxes.Error("Can't Convert!", error_message);

                return;
            }
        }

        public void Menu_CreateNewScript(string scriptName, string archiveFolder)
        {
            string scriptPath = string.Format("{0}/{1}/{2}.lua", projectManager.GetWorkingDirectory(), archiveFolder, scriptName);

            File.WriteAllText(scriptPath, "");

            if (OpenTextFile(scriptPath))
                SetSyntaxHighlighting("Lua");

            projectManager.fileTreeManager.RepopulateFileTree();
        }


        public void Menu_Import()
        {
            //temp variable for file path
            List<string> filePaths = new List<string>();

            //open file browser dialog (can select multiple)
            ioManagement.GetFilePaths(ref filePaths, "Import File(s)");

            //if the user cancled the operation, list will be empty
            if (filePaths.Count <= 0)
                return;

            //get our window object ready
            SelectFolder selectFolder;

            //if we only have 1 file selected, then just open the window with that single path, otherwise open it with the list
            if (filePaths.Count == 1)
                selectFolder = new SelectFolder(this, filePaths[0]);
            else
                selectFolder = new SelectFolder(this, filePaths);

            //show the xmal window
            selectFolder.ShowDialog();
        }

        public void Menu_Delete()
        {
            string messageBoxTitle = "";
            string messageBoxMessage = "";

            bool isDirectory = false;

            string path = projectManager.fileTreeManager.TreeView_GetSelectedFilePath(ref isDirectory);

            if (isDirectory)
            {
                if (string.IsNullOrEmpty(path) || path.Equals(Path.GetDirectoryName(projectManager.GetProjectFilePath())))
                {
                    messageBoxes.Error("Can't Delete", "Can't delete directory!");
                    return;
                }

                string folderName = path.Remove(0, Path.GetDirectoryName(path).Length + 1);

                messageBoxTitle = "Delete Folder";
                messageBoxMessage = string.Format("Are you sure you want to delete folder '{0}'? This will delete all of the contents in this folder as well.", folderName);
            }
            else
            {
                if (Path.GetExtension(path).Equals(".tseproj"))
                {
                    messageBoxes.Error("Can't Delete", "Can't delete a project file!");
                    return;
                }

                messageBoxTitle = "Delete File";
                messageBoxMessage = string.Format("Are you sure you want to delete '{0}'?", Path.GetFileName(path));
            }

            if (messageBoxes.Warning_Confirm(messageBoxTitle, messageBoxMessage))
            {
                if (isDirectory)
                {
                    ioManagement.DeleteDirectoryContents(path);
                    ioManagement.DeleteDirectory(path);
                }
                else
                {
                    ioManagement.DeleteFile(path);
                }

                projectManager.fileTreeManager.RepopulateFileTree();

                if (currentlyOpenFile != null)
                {
                    mainWindow.ui_editor_textEditor.Document.Text = "";
                    mainWindow.ui_editor_textEditor.IsModified = false;
                    currentlyOpenFile = "";
                    ResetEditorView();
                }
            }
        }

        public void Menu_OpenBuildConfig()
        {
            BuildConfig buildConfig = new BuildConfig(mainWindow, this);
            buildConfig.ShowDialog();
        }

        //------------------------------------------- MAIN WINDOW COMMANDS END -------------------------------------------
        
        public void ContextMenu_OpenInExplorer()
        {
            ioManagement.OpenInFileExplorer(projectManager.GetWorkingDirectory());
        }

        public List<string> GetFolderNames_From_ProjectDirectory()
        {
            List<string> folderPaths = new List<string>(Directory.GetDirectories(projectManager.GetWorkingDirectory()));
            List<string> folderNames = new List<string>();

            foreach(string folderPath in folderPaths)
            {
                string root = Path.GetDirectoryName(folderPath);
                string name = folderPath.Remove(0, root.Length + 1);

                folderNames.Add(name);
            }

            return folderNames;
        }

        public string DisplayImageProperties(string path, File_DDS file_DDS)
        {
            string final = "";

            string name = Path.GetFileName(path);

            final += string.Format("Image Name : {0}", name);
            final += Environment.NewLine;

            final += string.Format("Pixel Width : {0}", file_DDS.dwWidth.ToString());
            final += Environment.NewLine;

            final += string.Format("Pixel Height : {0}", file_DDS.dwHeight.ToString());
            final += Environment.NewLine;

            final += string.Format("DDS Format : {0}", file_DDS.ddspf_dwFourCC);
            final += Environment.NewLine;

            bool hasMipMaps = file_DDS.dwMipMapCount > 0;
            final += string.Format("Has Mip Maps : {0}", hasMipMaps.ToString());
            final += Environment.NewLine;

            final += string.Format("Mip Map Count : {0}", file_DDS.dwMipMapCount.ToString());
            final += Environment.NewLine;

            int headerLength = 4 + (int)file_DDS.dwSize;
            final += string.Format("DDS Header Size : {0}", headerLength.ToString());
            final += Environment.NewLine;

            final += string.Format("DDS Texture Data Size : {0}", file_DDS.textureData.Length.ToString());
            final += Environment.NewLine;

            return final;
        }

        public string DisplayImageProperties_D3DTX(string path, File_D3DTX file_D3DTX)
        {
            string final = "";

            string name = Path.GetFileName(path);

            final += string.Format("File Name : {0}", name);
            final += Environment.NewLine;

            final += string.Format("Original Name : {0}", file_D3DTX.originalFileName);
            final += Environment.NewLine;

            final += string.Format("Original Name In File : {0}", file_D3DTX.fileNameInFile_string);
            final += Environment.NewLine;

            final += string.Format("Header Byte Length : {0}", file_D3DTX.headerLength);
            final += Environment.NewLine;

            final += string.Format("File Version : {0}", file_D3DTX.magic);
            final += Environment.NewLine;

            final += string.Format("Pixel Width : {0}", file_D3DTX.imageWidth.ToString());
            final += Environment.NewLine;

            final += string.Format("Pixel Height : {0}", file_D3DTX.imageHeight.ToString());
            final += Environment.NewLine;

            final += string.Format("DDS Format : {0} [Raw Value = {1}]", file_D3DTX.parsed_dxtType_string, file_D3DTX.dxtType.ToString());
            final += Environment.NewLine;

            bool hasMipMaps = file_D3DTX.imageMipMapCount_decremented > 0;
            final += string.Format("Has Mip Maps : {0}", hasMipMaps.ToString());
            final += Environment.NewLine;

            final += string.Format("Mip Map Count : {0} ({1})", file_D3DTX.imageMipMapCount.ToString(), file_D3DTX.imageMipMapCount_decremented.ToString());
            final += Environment.NewLine;

            final += string.Format("Original Texture Byte Size : {0}", file_D3DTX.textureDataByteSize.ToString());
            final += Environment.NewLine;

            final += string.Format("Original Texture Mip Maps Byte Size : {0}", file_D3DTX.mipMapByteSize.ToString());
            final += Environment.NewLine;

            final += string.Format("Original Name In File Length : {0}", file_D3DTX.fileNameInFileLength.ToString());
            final += Environment.NewLine;

            return final;
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
        public bool OpenTextFile(string fileLocation)
        {
            if (!ResetEditorView())
                return false;

            currentlyOpenFile = fileLocation;

            mainWindow.EditorWindow_OpenTextEditor(currentlyOpenFile);

            return true;
        }

        /// <summary>
        /// Opens & displays a DDS image file in the editor.
        /// </summary>
        public bool OpenImageFile(string fileLocation)
        {
            if (!ResetEditorView())
                return false;

            currentlyOpenFile = fileLocation;

            mainWindow.EditorWindow_OpenImage(currentlyOpenFile);

            return true;
        }

        /// <summary>
        /// Displays the editor for .tseproj files.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public void OpenProjectFile(string x)
        {
            //throw new NotImplementedException();
        }

        public void DisableImageViewer()
        {
            mainWindow.ui_editor_imageViewer.IsEnabled = false;
            mainWindow.ui_editor_imageViewer.Visibility = Visibility.Hidden;
            mainWindow.ui_editor_imageViewer_image.Source = null;
        }

        /// <summary>
        /// Closes / disables all editor panels.
        /// </summary>
        private bool ResetEditorView()
        {
            if(mainWindow.ui_editor_textEditor.IsEnabled && mainWindow.ui_editor_textEditor.IsModified && currentlyOpenFile != null)
            {
                int result = messageBoxes.Error_ConfirmOrCancel("Save?", "Would you like to save your changes?");

                if (result == 0)
                    File.WriteAllText(currentlyOpenFile, mainWindow.ui_editor_textEditor.Text);
                else if (result == 2)
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

        public bool CurrentlyOpen_IsLua()
        {
            if (string.IsNullOrEmpty(currentlyOpenFile))
                return false;

            return Path.GetExtension(currentlyOpenFile).Equals(".lua");
        }

        public bool CurrentlyOpen_IsDDS()
        {
            if (string.IsNullOrEmpty(currentlyOpenFile))
                return false;

            return Path.GetExtension(currentlyOpenFile).Equals(".dds");
        }

        public bool CurrentlyOpen_IsD3DTX()
        {
            if (string.IsNullOrEmpty(currentlyOpenFile))
                return false;

            return Path.GetExtension(currentlyOpenFile).Equals(".d3dtx");
        }
    }
}
