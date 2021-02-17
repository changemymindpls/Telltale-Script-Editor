using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Telltale_Script_Editor.FileManagement;
using Telltale_Script_Editor.GUI;
using Telltale_Script_Editor.Utils;
using Ookii.Dialogs.Wpf;

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for SelectFolder.xaml
    /// </summary>
    public partial class SelectFolder : Window
    {
        private string fileToImport; //for a single file
        private List<string> filesToImport; //for multiple files

        //the final directory path where the file(s) will be imported to
        private string finalDirectory;

        //are we importing multiple files? (set when a window is created)
        private bool multiImportMode;

        //our custom objects
        private MessageBoxes messageBoxes;
        private EditorPanelManager editorPanelManager;
        private IOManagement ioManagement;

        /// <summary>
        /// Creates a Window where the user selects which folder in the project directory to import the file to.
        /// </summary>
        /// <param name="editorPanelManager"></param>
        /// <param name="fileToImport"></param>
        public SelectFolder(EditorPanelManager editorPanelManager, string fileToImport)
        {
            //xaml initalization
            InitializeComponent();

            //get our objects and path
            this.editorPanelManager = editorPanelManager;
            this.fileToImport = fileToImport;

            if (CheckForLua())
            {
                ui_importOption_checkbox.Content = "Decompile .lua scripts on import?";
                ui_importOption_checkbox.IsEnabled = true;
                ui_importOption_checkbox.Visibility = Visibility.Visible;
            }
            else if (CheckForD3DTX())
            {
                ui_importOption_checkbox.Content = "Convert .d3dtx textures on import?";
                ui_importOption_checkbox.IsEnabled = true;
                ui_importOption_checkbox.Visibility = Visibility.Visible;
            }
            else
            {
                ui_importOption_checkbox.IsEnabled = false;
                ui_importOption_checkbox.Visibility = Visibility.Hidden;
            }

            //initalize our own objects for this window
            InitalizeWindow();
        }

        /// <summary>
        /// Creates a Window where the user selects which folder in the project directory to import the multiple files to.
        /// </summary>
        /// <param name="editorPanelManager"></param>
        /// <param name="filesToImport"></param>
        public SelectFolder(EditorPanelManager editorPanelManager, List<string> filesToImport)
        {
            //xaml initalization
            InitializeComponent();

            //get our objects and multiple file paths
            this.editorPanelManager = editorPanelManager;
            this.filesToImport = filesToImport;

            //we are importing multiple files, so set this to true
            multiImportMode = true;

            if(CheckForLua())
            {
                ui_importOption_checkbox.Content = "Decompile .lua scripts on import?";
                ui_importOption_checkbox.IsEnabled = true;
                ui_importOption_checkbox.Visibility = Visibility.Visible;
            }
            else if(CheckForD3DTX())
            {
                ui_importOption_checkbox.Content = "Convert .d3dtx textures on import?";
                ui_importOption_checkbox.IsEnabled = true;
                ui_importOption_checkbox.Visibility = Visibility.Visible;
            }
            else if(CheckForLua() && CheckForD3DTX())
            {
                ui_importOption_checkbox.Content = "Decompile .lua scripts and convert .d3dtx textures on import?";
                ui_importOption_checkbox.IsEnabled = true;
                ui_importOption_checkbox.Visibility = Visibility.Visible;
            }
            else
            {
                ui_importOption_checkbox.IsEnabled = false;
                ui_importOption_checkbox.Visibility = Visibility.Hidden;
            }

            //initalize our own objects for this window
            InitalizeWindow();
        }

        private bool CheckForLua()
        {
            if(multiImportMode)
            {
                foreach (string file in filesToImport)
                {
                    if (System.IO.Path.GetExtension(file).Equals(".lua"))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (System.IO.Path.GetExtension(fileToImport).Equals(".lua"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckForD3DTX()
        {
            if (multiImportMode)
            {
                foreach (string file in filesToImport)
                {
                    if (System.IO.Path.GetExtension(file).Equals(".d3dtx"))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (System.IO.Path.GetExtension(fileToImport).Equals(".d3dtx"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Initalizes our custom objects and updates UI elements.
        /// </summary>
        private void InitalizeWindow()
        {
            //initalize our custom objects
            messageBoxes = new MessageBoxes(); //for our message boxes
            ioManagement = new IOManagement(); //for messing with files on the disk

            //fille the items on the combobox with the names of the folders from the project directory
            ui_folderName_comobox.ItemsSource = editorPanelManager.GetFolderNames_From_ProjectDirectory();

            //if we are importing multiple, update the UI text to reflect that
            if (multiImportMode)
            {
                //update the label
                ui_folderSelect_label.Content = "Select a folder where the multiple files will be copied to.";

                //update the window title
                Title = "Import Files";
            }
        }

        /// <summary>
        /// Peforms some prechecks before we import said file(s). Returns true if sucessful, returns false if not.
        /// </summary>
        /// <returns></returns>
        private bool PreCheck()
        {
            //if they didn't bother selecting a folder, tell them!
            if (ui_folderName_comobox.SelectedIndex < 0)
            {
                //give them an error and tell them to select a darn folder for where the file(s) will be placed!
                messageBoxes.Error("No Folder Selected", "Select a folder where the script will be moved to!");

                //precheck failed
                return false;
            }

            //if we are in multi import mode then we gotta do some extra stuff
            if(multiImportMode)
            {
                //this will be set to false if there are files that already exist with the same name in the list of files the user is trying to import
                bool canImport = true;

                //run a loop for all of the files the user selected
                foreach(string file in filesToImport)
                {
                    //build a temp path for the file that will be imported (just for checking)
                    string finalPath = string.Format("{0}/{1}/{2}", editorPanelManager.projectManager.GetWorkingDirectory(), (string)ui_folderName_comobox.SelectedItem, System.IO.Path.GetFileName(file));

                    //using the temp path, check if there is a file of the same name and place
                    if (File.Exists(finalPath))
                    {
                        //if there is a file that already exists, we can't import!
                        //note to self - give prompt if the user wants to overwrite the files in the project
                        canImport = false;
                    }
                }

                //if we can import the files then the precheck passed!
                if(canImport)
                {
                    //sucess
                    return true;
                }
                else
                {
                    //give them an error and tell them to select a darn folder for where the script will be placed!
                    messageBoxes.Error("File Already Exists", "There is already a file of the same name in the folder you are trying to import!");

                    //precheck failed cause they already have existing files
                    return false;
                }
            }
            else //for importing a single file
            {
                //build a temp path for the file that will be imported (just for checking)
                string finalPath = string.Format("{0}/{1}/{2}", editorPanelManager.projectManager.GetWorkingDirectory(), (string)ui_folderName_comobox.SelectedItem, System.IO.Path.GetFileName(fileToImport));

                if (File.Exists(finalPath))
                {
                    //give them an error and tell them to select a darn folder for where the script will be placed!
                    messageBoxes.Error("File Already Exists", "There is already a file of the same name in the folder you are trying to import!");

                    //precheck failed cause they already have an existing file
                    return false;
                }
            }

            //if none of the statements tripped, sucess!
            return true;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_selectFolder_button_Click(object sender, RoutedEventArgs e)
        {
            //create our final directory where the imported file(s) will be copied to
            finalDirectory = string.Format("{0}/{1}/", editorPanelManager.projectManager.GetWorkingDirectory(), (string)ui_folderName_comobox.SelectedItem);

            //do some prechecks
            if (PreCheck())
            {
                //if we are in multiple import mode, then run a loop for all of the files
                if (multiImportMode)
                {
                    //do a loop for all of the selected files
                    foreach(string file in filesToImport)
                    {
                        //duplicate the file to the directory
                        ioManagement.DuplicateFileToDirectory(file, finalDirectory);
                    }
                }
                else //for single files
                {
                    //duplicate the file to the directory
                    ioManagement.DuplicateFileToDirectory(fileToImport, finalDirectory);
                }

                //repopulate the file tree in the main window
                editorPanelManager.projectManager.fileTreeManager.RepopulateFileTree();

                //close this window since we are done
                Close();
            }
        }

        private void ui_cancel_button_Click(object sender, RoutedEventArgs e)
        {
            //close this window since the user changed their mind
            Close();
        }

        private void ui_importOption_checkbox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
