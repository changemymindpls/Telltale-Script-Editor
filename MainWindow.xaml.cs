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

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ProjectManager pManager;

        public MainWindow()
        {
            InitializeComponent();
            Console.SetOut(new ConsoleWriter(ConsoleOutputBox));

            //TODO: Implement user preferences - hardcoded for now.
            ThemeManager.SetTheme(Theme.Dark);
        }

        /// <summary>
        /// File -> Open -> Project
        /// </summary>
        private void FileOpenProject_Click(object x, RoutedEventArgs y)
        {
            if (pManager != null) //checks if another project is open & prompts the user on whether or not to continue.
            {
                if (
                    MessageBox.Show(
                        "Are you sure you'd like to continue? Unsaved changes will be lost.",
                        "Are you sure?",
                        MessageBoxButton.YesNo
                    ) != MessageBoxResult.Yes
                   )
                {
                    return;
                }
            }

            //method actually starts here :) 

            OpenFileDialog oDlg = new OpenFileDialog();

            oDlg.Filter = "Telltale Script Editor Project (*.tseproj)|*.tseproj";
            oDlg.FilterIndex = 1;
            oDlg.Multiselect = false;

            if (oDlg.ShowDialog() == false)
                return;

            Console.WriteLine($"Selected project file at {oDlg.FileName}");

            if (pManager != null)
                pManager.Destroy();

            pManager = new ProjectManager(oDlg.FileName, editorTreeView, new EditorPanelManager());
            pManager.BuildProject();

            welcomePanel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// File -> Exit
        /// </summary>
        private void FileExit_Click(object x, RoutedEventArgs y)
        {
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Project -> Build Configuration
        /// </summary>
        private void ProjectBuildConfiguration_Click(object x, RoutedEventArgs y)
        {
            if(pManager == null)
            {
                MessageBox.Show("You need to open a project first.", "You can't do that!");
                return;
            }
            BuildConfig cfg = new BuildConfig(pManager);
            cfg.Show();
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
