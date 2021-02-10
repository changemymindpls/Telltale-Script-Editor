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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //xaml windows
        private ProjectWindow projectWindow;
        private BuildConfig buildConfig;
        private EditorSettingsWindow editorSettingsWindow;

        //custom objects
        public EditorPanelManager editorPanelManager;

        public MainWindow()
        {
            //xaml initalization
            InitializeComponent();

            //initalize our application
            InitalizeApplication();

            //update our UI after initalization
            UpdateUI();
        }

        private void InitalizeApplication()
        {
            editorPanelManager = new EditorPanelManager(this);

            Console.SetOut(new ConsoleWriter(ui_editor_consoleOutput));

            //TODO: Implement user preferences - hardcoded for now.
            ThemeManager.SetTheme(Theme.Light);
        }

        public void UpdateUI()
        {
            bool projectActive = editorPanelManager.projectManager != null;

            ui_menu_file_import.IsEnabled = projectActive;
            ui_menu_file_new_dataSet.IsEnabled = projectActive;
            ui_menu_file_new_script.IsEnabled = projectActive;
            ui_menu_file_new_folder.IsEnabled = projectActive;
            ui_menu_file_open_telltaleArchive.IsEnabled = projectActive;
            ui_menu_file_save.IsEnabled = projectActive;
            ui_menu_file_saveAs.IsEnabled = projectActive;
            ui_menu_project.IsEnabled = projectActive;
            ui_menu_project_build.IsEnabled = projectActive;
            ui_menu_project_buildAndRun.IsEnabled = projectActive;
            ui_menu_project_buildConfiguration.IsEnabled = projectActive;
            ui_menu_project_projectSettings.IsEnabled = projectActive;

            ui_editor_consoleOutput.TextWrapping = ui_editor_consoleOutput_contextmenu_textWrapping.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;

            ui_editor_projectTree_contextMenu_delete.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_import.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_refresh.IsEnabled = projectActive;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_menu_file_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void ui_menu_file_new_newProject_Click(object sender, RoutedEventArgs e)
        {
            projectWindow = new ProjectWindow(this, editorPanelManager);
            projectWindow.Show();

            UpdateUI();
        }

        private void ui_menu_project_projectSettings_Click(object sender, RoutedEventArgs e)
        {
            projectWindow = new ProjectWindow(this, editorPanelManager, true);
            projectWindow.Show();

            UpdateUI();
        }

        /// <summary>
        /// File -> Open -> Project
        /// </summary>
        private void ui_menu_file_open_project_Click(object x, RoutedEventArgs y)
        {
            editorPanelManager.OpenProject();

            UpdateUI();
        }

        /// <summary>
        /// Project -> Build Configuration
        /// </summary>
        private void ui_menu_project_buildConfiguration_Click(object x, RoutedEventArgs y)
        {
            buildConfig = new BuildConfig(editorPanelManager);
            buildConfig.ShowDialog();

            UpdateUI();
        }

        private void ui_menu_project_build_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.projectManager.BuildProject();

            UpdateUI();
        }

        private void ui_menu_project_buildAndRun_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.projectManager.BuildProject(true);

            UpdateUI();
        }

        private void ui_menu_help_debug_showProjectInfo_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show($".tseproj version - {projectManager.project.tseproj.version}\n\nProject Name - {projectManager.project.project.name}\nProject Version - {projectManager.project.project.version}\nProject Author - {projectManager.project.project.author}\n\nTool Game - {projectManager.project.tool.game}\nTool Executable - {projectManager.project.tool.executable}\nTool Master Priority - {projectManager.project.tool.master_priority}", "Debug - Project Info");
            
            UpdateUI();
        }


        private void ui_menu_help_about_Click(object sender, RoutedEventArgs e)
        {
            About abt = new About();
            abt.ShowDialog();

            UpdateUI();
        }

        private void ui_editor_consoleOutput_contextmenu_verboseOutput_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_editor_consoleOutput_contextmenu_textWrapping_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_edit_editorSettings_Click(object sender, RoutedEventArgs e)
        {
            editorSettingsWindow = new EditorSettingsWindow();
            editorSettingsWindow.Show();

            UpdateUI();
        }

        private void ui_menu_file_import_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Import();

            UpdateUI();
        }

        private void ui_menu_file_open_telltaleArchive_Click(object sender, RoutedEventArgs e)
        {

        }

        /* I was experimenting with SharpGL.WPF - might reinstall in the future, removed for now.
        private void oglCtrl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.ClearColor(255, 255, 255, 1);

            gl.LoadIdentity();

            gl.Translate(-1.5f, 0.0f, 3.0f);

            gl.Translate(1.5f, 0.0f, -7.0f);
            gl.Begin(OpenGL.GL_QUADS);
                gl.Color(0.0f, 0.0f, 0.0f);
                gl.Vertex(-0.5f, 0.5f);
                gl.Vertex(0.5f, 0.5f);
                gl.Vertex(0.5f, -0.5f);
                gl.Vertex(-0.5f, -0.5f);
            gl.End();
            gl.Flush();
        }
        */


    }
}
