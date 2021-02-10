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
        private bool settingsMode;

        private MainWindow mainWindow;
        private EditorPanelManager editorPanelManager;

        private IOManagement ioManagement;
        private MessageBoxes messageBoxes;

        public ProjectWindow(MainWindow mainWindow, EditorPanelManager editorPanelManager, bool settingsMode = false)
        {
            InitializeComponent();

            //TODO: Implement user preferences - hardcoded for now.
            ThemeManager.SetTheme(Theme.Light);

            this.mainWindow = mainWindow;
            this.editorPanelManager = editorPanelManager;

            ioManagement = new IOManagement();
            messageBoxes = new MessageBoxes();

            this.settingsMode = settingsMode;

            UpdateUI();
        }

        public void UpdateUI()
        {
            if(settingsMode)
                ui_createProject_button.Content = "Save Settings";
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_projectName_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_projectAuthor_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_projectVersion_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_projectDirectory_browse_button_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = "";

            ioManagement.GetFolderPath(ref folderPath);

            if (string.IsNullOrEmpty(folderPath))
                return;

            ui_projectDirectory_textBox.Text = folderPath;

            UpdateUI();
        }

        private void ui_gameVersion_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_cancelProject_button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.UpdateUI();
            Close();
        }

        private bool PreCheck_Fields()
        {
            if (ui_projectName_textBox.Text.Equals("Name"))
            {
                messageBoxes.Error("Name Field Improper", "You haven't edited the Project Name field!");
                return false;
            }

            if (ui_projectAuthor_textBox.Text.Equals("Author"))
            {
                messageBoxes.Error("Author Field Improper", "You haven't edited the Project Author field!");
                return false;
            }

            if (ui_projectVersion_textBox.Text.Equals("Version"))
            {
                messageBoxes.Error("Version Field Improper", "You haven't edited the Project Version field!");
                return false;
            }

            return true;
        }

        private void ui_createProject_button_Click(object sender, RoutedEventArgs e)
        {
            string projectName = ui_projectName_textBox.Text;
            string projectAuthor = ui_projectAuthor_textBox.Text;
            string projectVersion = ui_projectVersion_textBox.Text;
            string projectPath = ui_projectDirectory_textBox.Text;
            int gameVersion = ui_gameVersion_comboBox.SelectedIndex;

            if (!PreCheck_Fields())
                return;

            string fileName = projectName.Replace(" ", "_").ToLower();
            string finalProjectPath = projectPath + "/" + fileName + ".tseproj";

            ProjectManager new_projectManager = new ProjectManager(mainWindow, finalProjectPath, projectName, projectAuthor, projectVersion, gameVersion);

            editorPanelManager.projectManager = new_projectManager;

            mainWindow.UpdateUI();

            //close this window since we are done
            Close();
        }
    }
}
