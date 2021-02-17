using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
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

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        //for editing the project settings (instead of creating a new one)
        private bool settingsMode;

        //our main objects
        private MainWindow mainWindow;
        private EditorPanelManager editorPanelManager;
        private IOManagement ioManagement;
        private MessageBoxes messageBoxes;
        private Enumerators enumerators;

        /// <summary>
        /// Creates a Project window where you can configure the settings of the project. (For a new and existing one)
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="editorPanelManager"></param>
        /// <param name="settingsMode"></param>
        public ProjectWindow(MainWindow mainWindow, EditorPanelManager editorPanelManager, bool settingsMode = false)
        {
            //xaml initlization
            InitializeComponent();

            //get our objects
            this.mainWindow = mainWindow;
            this.editorPanelManager = editorPanelManager;
            this.settingsMode = settingsMode;

            //initalize our own objects
            InitalizeWindow();
        }

        /// <summary>
        /// Initalizes our main objects for this window
        /// </summary>
        private void InitalizeWindow()
        {
            //TODO: Implement user preferences - hardcoded for now.
            //set the theme of the window
            ThemeManager.SetTheme(Theme.Light);

            //create our objects
            ioManagement = new IOManagement(); //so we can mess with the files
            messageBoxes = new MessageBoxes(); //so we can do message boxes
            enumerators = new Enumerators(); //our enums

            //assign the combobox with a list of our game versions
            ui_gameVersion_comboBox.ItemsSource = enumerators.GameVersion_NamesList_WithSpaces();

            //if we are in settings mode
            if(settingsMode)
            {
                //change the window title
                Title = "Project Settings";

                //change the create project button to save settings
                ui_createProject_button.Content = "Save Settings";

                //assign the existing project values into the UI elements
                ui_projectName_textBox.Text = editorPanelManager.projectManager.project.Project.Name;
                ui_projectAuthor_textBox.Text = editorPanelManager.projectManager.project.Project.Author;
                ui_projectVersion_textBox.Text = editorPanelManager.projectManager.project.Project.Version;
                ui_projectDirectory_textBox.Text = editorPanelManager.projectManager.GetWorkingDirectory();
                ui_gameVersion_comboBox.SelectedIndex = editorPanelManager.projectManager.project.Tool.Game;
            }
        }

        /// <summary>
        /// Performs some prechecks before proceeding. Returns true if sucessful, false if not.
        /// </summary>
        /// <returns></returns>
        private bool PreCheck_Fields()
        {
            if (ui_projectName_textBox.Text.Equals("Name")) //if they didn't bother changing the name of the field
            {
                //give them an error and tell them to give the project a damn name!
                messageBoxes.Error("Name Field Improper", "You haven't edited the Project Name field!");

                //precheck failed
                return false;
            }
            else if (ui_projectAuthor_textBox.Text.Equals("Author")) //if they didn't bother changing the author of the field
            {
                //give them an error and tell them to give the project a damn author!
                messageBoxes.Error("Author Field Improper", "You haven't edited the Project Author field!");

                //precheck failed
                return false;
            }
            else if(ui_projectVersion_textBox.Text.Equals("Version")) //if they didn't bother changing the version of the field
            {
                //give them an error and tell them to give the project a damn version!
                messageBoxes.Error("Version Field Improper", "You haven't edited the Project Version field!");

                //precheck failed
                return false;
            }
            else if (ui_gameVersion_comboBox.SelectedIndex < 0) //if they didn't bother selecting a game version
            {
                //give them an error and tell them to select a damn game version!
                messageBoxes.Error("Version Field Improper", "You haven't edited the Project Version field!");

                //precheck failed
                return false;
            }

            //if none of the statements tripped, sucess!
            return true;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_projectDirectory_browse_button_Click(object sender, RoutedEventArgs e)
        {
            //our temp folder path (will be assigned)
            string folderPath = "";

            //open a folder browser for getting a folder path
            ioManagement.GetFolderPath(ref folderPath);

            //if the string is null then the user canceled the operation
            if (string.IsNullOrEmpty(folderPath))
                return; //don't continue since they changed their mind

            //if not, assign the selected path to the UI field.
            ui_projectDirectory_textBox.Text = folderPath;
        }

        private void ui_cancelProject_button_Click(object sender, RoutedEventArgs e)
        {
            //close the window since they change their mind (nothing else to do)
            Close();
        }

        private void ui_createProject_button_Click(object sender, RoutedEventArgs e)
        {
            //get our user inputed fields
            string projectName = ui_projectName_textBox.Text;
            string projectAuthor = ui_projectAuthor_textBox.Text;
            string projectVersion = ui_projectVersion_textBox.Text;
            string projectPath = ui_projectDirectory_textBox.Text;
            int gameVersion = ui_gameVersion_comboBox.SelectedIndex;

            //perform a precheck
            if (!PreCheck_Fields())
                return; //if it returns false, don't continue since it failed.

            //build our project path (will remove spaces and make it lowercase)
            string fileName = projectName.Replace(" ", "_").ToLower();
            string finalProjectPath = projectPath + "/" + fileName + ".tseproj";

            //build a project object and assign the given values
            ProjectManager new_projectManager = new ProjectManager(mainWindow, finalProjectPath, projectName, projectAuthor, projectVersion, gameVersion);

            //assign this to the editor panel
            editorPanelManager.projectManager = new_projectManager;

            //update the UI of the main window
            mainWindow.UpdateUI();

            //close this window since we are done
            Close();
        }
    }
}
