using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telltale_Script_Editor.FileManagement;
using Telltale_Script_Editor.GUI;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for BuildConfig.xaml
    /// </summary>
    public partial class BuildConfig : Window
    {
        private MainWindow mainWindow;
        private EditorPanelManager editorPanelManager;
        private Enumerators enumerators;
        private IOManagement ioManagement;

        /// <summary>
        /// Build configuration menu.
        /// </summary>
        public BuildConfig(MainWindow mainWindow, EditorPanelManager editorPanelManager)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.editorPanelManager = editorPanelManager;

            enumerators = new Enumerators();
            ioManagement = new IOManagement();

            InitalizeWindow();
        }

        private void InitalizeWindow()
        {
            FileTreeManager cfgManager = new FileTreeManager(ui_projectTree_treeView, editorPanelManager.projectManager.GetWorkingDirectory(), editorPanelManager, null, true);

            ui_gameVersion_combobox.ItemsSource = enumerators.GameVersion_NamesList_WithSpaces();
            ui_gameVersion_combobox.SelectedIndex = editorPanelManager.projectManager.project.Tool.Game;
            ui_masterPriority_textBox.Text = editorPanelManager.projectManager.project.Tool.Master_Priority.ToString();
            ui_gameExeLocation_textBox.Text = editorPanelManager.projectManager.project.Tool.Executable;
        }

        private void SaveConfiguration()
        {
            editorPanelManager.projectManager.project.Tool.Master_Priority = int.Parse(ui_masterPriority_textBox.Text);
            editorPanelManager.projectManager.project.Tool.Executable = ui_gameExeLocation_textBox.Text;
            editorPanelManager.projectManager.project.Tool.Game = ui_gameVersion_combobox.SelectedIndex;
        }

        private void ui_cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ui_browseExeLocation_button_Click(object sender, RoutedEventArgs e)
        {
            //temp variable for file path
            string exePath = "";

            //open file browser dialog
            ioManagement.GetFilePath(ref exePath, "Game Exe (*.exe)|*.exe", "Select the Game Exe");

            //if the user cancled the operation, path will be null
            if (string.IsNullOrEmpty(exePath))
                return;

            ui_gameExeLocation_textBox.Text = exePath;
        }

        private void ui_saveConfig_button_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();

            Close();
        }
    }
}
