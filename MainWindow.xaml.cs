using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit;
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
using System.ComponentModel;
using System.Xml;
using Telltale_Script_Editor.FileManagement;
using Telltale_Script_Editor.GUI;
using Telltale_Script_Editor.Utils;
using Telltale_Script_Editor.TextureConvert;
using Ookii.Dialogs.Wpf;
//using SharpGL;
//using SharpGL.SceneGraph;

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //for text editor
        private CompletionWindow completionWindow;
        private TextEditor_LuaCompletion textEditor_LuaCompletion;

        private BackgroundWorker backgroundWorker;

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

        private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if(editorPanelManager.CurrentlyOpen_IsLua())
            {
                textEditor_LuaCompletion.textEditor_TextArea_TextEntered(sender, e, completionWindow, ui_editor_textEditor);
            }
        }

        private void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (editorPanelManager.CurrentlyOpen_IsLua())
            {
                if (e.Text.Length > 0 && completionWindow != null)
                {
                    if (!char.IsLetterOrDigit(e.Text[0]))
                    {
                        // Whenever a non-letter is typed while the completion window is open, insert the currently selected element.
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                }
                // Do not set e.Handled=true.
                // We still want to insert the character that was typed.
            }
        }

        /// <summary>
        /// Initalizes the objects for this class.
        /// </summary>
        private void InitalizeApplication()
        {
            editorPanelManager = new EditorPanelManager(this);
            textEditor_LuaCompletion = new TextEditor_LuaCompletion();

            Console.SetOut(new ConsoleWriter(ui_editor_consoleOutput));

            //TODO: Implement user preferences - hardcoded for now.
            ThemeManager.SetTheme(Theme.Light);

            //for text editor
            ui_editor_textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            ui_editor_textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            SearchPanel.Install(ui_editor_textEditor.TextArea);
        }

        /// <summary>
        /// Updates the UI elements on the Main Editor Window.
        /// </summary>
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

            ui_menu_edit_script.IsEnabled = editorPanelManager.CurrentlyOpen_IsLua();
            ui_menu_edit_texture.IsEnabled = editorPanelManager.CurrentlyOpen_IsD3DTX() || editorPanelManager.CurrentlyOpen_IsDDS();
            ui_menu_edit_texture_convertToD3DTX.IsEnabled = editorPanelManager.CurrentlyOpen_IsDDS();
            ui_menu_edit_texture_convertToDDS.IsEnabled = editorPanelManager.CurrentlyOpen_IsD3DTX();
            ui_menu_edit_copy.IsEnabled = editorPanelManager.CurrentlyOpen_IsLua();
            ui_menu_edit_paste.IsEnabled = editorPanelManager.CurrentlyOpen_IsLua();
            ui_menu_file_save.IsEnabled = editorPanelManager.CurrentlyOpen_IsLua();
            ui_menu_file_saveAs.IsEnabled = editorPanelManager.CurrentlyOpen_IsLua();

            ui_editor_consoleOutput.TextWrapping = ui_editor_consoleOutput_contextmenu_textWrapping.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
            ui_editor_consoleOutput.ScrollToEnd();

            ui_editor_projectTree_contextMenu_new.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_new_folder.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_new_script.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_delete.IsEnabled = projectActive && ui_editor_projectTree_treeView.SelectedItem != null;
            ui_editor_projectTree_contextMenu_import.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_refresh.IsEnabled = projectActive;
            ui_editor_projectTree_contextMenu_openExplorer.IsEnabled = projectActive;

            ui_menu_edit_redo.IsEnabled = ui_editor_textEditor.IsModified;
            ui_menu_edit_undo.IsEnabled = ui_editor_textEditor.IsModified;
            ui_editor_textEditor_contextMenu_save.IsEnabled = ui_editor_textEditor.IsModified;
            ui_editor_textEditor_contextMenu_redo.IsEnabled = ui_editor_textEditor.IsModified;
            ui_editor_textEditor_contextMenu_undo.IsEnabled = ui_editor_textEditor.IsModified;
        }

        /// <summary>
        /// Opens the Text Editor panel. (FOR UI)
        /// </summary>
        /// <param name="textPath"></param>
        public void EditorWindow_OpenTextEditor(string textPath)
        {
            ui_editor_textEditor.IsEnabled = true;
            ui_editor_textEditor.Visibility = Visibility.Visible;
            ui_editor_textEditor.Text = File.ReadAllText(textPath);
            ui_editor_textEditor.IsModified = false; //set after the Text field is set

            ui_editor_imageViewer.IsEnabled = false;
            ui_editor_imageViewer.Visibility = Visibility.Hidden;
            ui_editor_imageViewer_image.Source = null;
        }

        /// <summary>
        /// Opens the Image Viewer panel. (FOR UI)
        /// </summary>
        /// <param name="imagePath"></param>
        public void EditorWindow_OpenImage(string imagePath)
        {
            ui_editor_textEditor.Text = null;
            ui_editor_textEditor.IsEnabled = false;
            ui_editor_textEditor.IsModified = false; //set after the Text field is set
            ui_editor_textEditor.Visibility = Visibility.Hidden;

            //set it to null since we are going to likley display a new one
            ui_editor_imageViewer_image.Source = null;

            //create our bitmap object
            BitmapImage bitmap;

            //if the selected file is not a .dds file
            if (System.IO.Path.GetExtension(imagePath).Equals(".dds") == false)
            {
                //if its a .header or .d3dtx file, we can still display the data from those files, just not the image
                if (System.IO.Path.GetExtension(imagePath).Equals(".header") || System.IO.Path.GetExtension(imagePath).Equals(".d3dtx"))
                {
                    //get our reader object
                    Read_D3DTX readD3DTX = new Read_D3DTX();
                    File_D3DTX file_D3DTX;

                    backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += backgroundWorker_readD3DTX_data_doWork;
                    backgroundWorker.RunWorkerCompleted += worker_readD3DTX_data_RunWorkerCompleted;
                    backgroundWorker.WorkerReportsProgress = true;
                    backgroundWorker.WorkerSupportsCancellation = true;
                    backgroundWorker.RunWorkerAsync(imagePath);
                    /*
                    file_D3DTX = readD3DTX.Read_D3DTX_File(imagePath, true);

                    
                    //display the data
                    ui_editor_imageViewer_properties.Text = editorPanelManager.DisplayImageProperties_D3DTX(imagePath, file_D3DTX);

                    if (System.IO.Path.GetExtension(imagePath).Equals(".header"))
                    {
                        //initalize our bitmap placeholder image object
                        bitmap = new BitmapImage();

                        //initalize the bitmap image initalizatiom method
                        bitmap.BeginInit();

                        //get the file path of the placeholder image on the disk (realtive to app)
                        bitmap.UriSource = new Uri("Resources/headerPlaceholder.png", UriKind.Relative);

                        //end the initalization
                        bitmap.EndInit();

                        //assign the source to our bitmap placeholder image object
                        ui_editor_imageViewer_image.Source = bitmap;
                    }
                    else
                    {
                        //initalize our bitmap placeholder image object
                        bitmap = new BitmapImage();

                        //initalize the bitmap image initalizatiom method
                        bitmap.BeginInit();

                        //get the file path of the placeholder image on the disk (realtive to app)
                        bitmap.UriSource = new Uri("Resources/cantDisplayPlaceholder.png", UriKind.Relative);

                        //end the initalization
                        bitmap.EndInit();

                        //assign the source to our bitmap placeholder image object
                        ui_editor_imageViewer_image.Source = bitmap;
                    }

                    //set our uI states
                    ui_editor_imageViewer.IsEnabled = true;
                    ui_editor_imageViewer.Visibility = Visibility.Visible;
                    */
                    //don't continue because WPF can't display a .header (no texture data in it anyway) or .d3dtx file natively (could try to create a temp .dds for a preview, but for the time being, no previewing)
                    return;
                }
                else
                {
                    //since we don't know what this image type is, don't bother displaying it
                    ui_editor_imageViewer_properties.Text = "Not a valid image format.";

                    //dunno what it is, so don't display it
                    return;
                }
            }

            Read_DDS readDDS = new Read_DDS();

            //we are going to try to display the bitmap (if our converter fails and the image can't be decoded as a DDS properly, then we will just catch the exception and ignore)
            try
            {
                //initalize our bitmap object
                bitmap = new BitmapImage();

                //initalize the bitmap image initalizatiom method
                bitmap.BeginInit();

                //cache the image (so other processes can use the image if needed)
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                //get the file path of the dds image on the disk
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);

                //end the initalization
                bitmap.EndInit();

                //assign the source to our bitmap object
                ui_editor_imageViewer_image.Source = bitmap;

                //display the image properties
                ui_editor_imageViewer_properties.Text = editorPanelManager.DisplayImageProperties(imagePath, readDDS.Read_DDS_File(imagePath));
            }
            catch (Exception e) //if the decode failed, display a placeholder
            {
                //notify the user and write a message.
                ui_editor_imageViewer_properties.Text = "CANT DECODE OR DISPLAY DDS IMAGE!";
                ui_editor_imageViewer_properties.Text += Environment.NewLine;
                ui_editor_imageViewer_properties.Text += editorPanelManager.DisplayImageProperties(imagePath, readDDS.Read_DDS_File(imagePath));

                //initalize our bitmap placeholder image object
                bitmap = new BitmapImage();

                //initalize the bitmap image initalizatiom method
                bitmap.BeginInit();

                //get the file path of the placeholder image on the disk (realtive to app)
                bitmap.UriSource = new Uri("Resources/decodeFailedPlaceholder.png", UriKind.Relative);

                //end the initalization
                bitmap.EndInit();

                //assign the source to our bitmap placeholder image object
                ui_editor_imageViewer_image.Source = bitmap;

            }

            ui_editor_imageViewer.IsEnabled = true;
            ui_editor_imageViewer.Visibility = Visibility.Visible;
        }

        private void worker_readD3DTX_data_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            File_D3DTX file_D3DTX = (File_D3DTX)e.Result;
            string imagePath = file_D3DTX.filePath;
            BitmapImage bitmap;

            //display the data
            ui_editor_imageViewer_properties.Text = editorPanelManager.DisplayImageProperties_D3DTX(imagePath, file_D3DTX);

            if (System.IO.Path.GetExtension(imagePath).Equals(".header"))
            {
                //initalize our bitmap placeholder image object
                bitmap = new BitmapImage();

                //initalize the bitmap image initalizatiom method
                bitmap.BeginInit();

                //get the file path of the placeholder image on the disk (realtive to app)
                bitmap.UriSource = new Uri("Resources/headerPlaceholder.png", UriKind.Relative);

                //end the initalization
                bitmap.EndInit();

                //assign the source to our bitmap placeholder image object
                ui_editor_imageViewer_image.Source = bitmap;
            }
            else
            {
                //initalize our bitmap placeholder image object
                bitmap = new BitmapImage();

                //initalize the bitmap image initalizatiom method
                bitmap.BeginInit();

                //get the file path of the placeholder image on the disk (realtive to app)
                bitmap.UriSource = new Uri("Resources/cantDisplayPlaceholder.png", UriKind.Relative);

                //end the initalization
                bitmap.EndInit();

                //assign the source to our bitmap placeholder image object
                ui_editor_imageViewer_image.Source = bitmap;
            }

            //set our uI states
            ui_editor_imageViewer.IsEnabled = true;
            ui_editor_imageViewer.Visibility = Visibility.Visible;

            UpdateUI();

            //don't continue because WPF can't display a .header (no texture data in it anyway) or .d3dtx file natively (could try to create a temp .dds for a preview, but for the time being, no previewing)
            return;
        }

        private void backgroundWorker_readD3DTX_data_doWork(object sender, DoWorkEventArgs e)
        {
            string imagePath = (string)e.Argument;

            //get our reader object
            Read_D3DTX readD3DTX = new Read_D3DTX();

            File_D3DTX file_D3DTX = readD3DTX.Read_D3DTX_File(imagePath, true);

            e.Result = file_D3DTX;
        }

        /// <summary>
        /// Resets the text editor and image viewer panels for the UI. (Hides and resets both)
        /// </summary>
        public void EditorWindow_Reset()
        {
            ui_editor_textEditor.Text = null;
            ui_editor_textEditor.IsEnabled = false;
            ui_editor_textEditor.IsModified = false; //set after the Text field is set
            ui_editor_textEditor.Visibility = Visibility.Hidden;

            ui_editor_imageViewer.IsEnabled = false;
            ui_editor_imageViewer.Visibility = Visibility.Hidden;
            ui_editor_imageViewer_image.Source = null;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------
        //---------------------------MENU FILE---------------------------
        private void ui_menu_file_new_newProject_Click(object sender, RoutedEventArgs e)
        {
            ProjectWindow projectWindow = new ProjectWindow(this, editorPanelManager);
            projectWindow.Show();

            UpdateUI();
        }

        private void ui_menu_file_open_project_Click(object x, RoutedEventArgs y)
        {
            editorPanelManager.Menu_OpenProject();

            UpdateUI();
        }

        private void ui_menu_file_open_telltaleArchive_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_file_import_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_Import();

            UpdateUI();
        }

        private void ui_menu_file_save_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveProject();

            UpdateUI();
        }

        private void ui_menu_file_saveAs_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveCurrentFileAs();

            UpdateUI();
        }

        private void ui_menu_file_saveProject_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveProject();
            
            UpdateUI();
        }

        private void ui_menu_file_saveProjectAs_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveProjectAs();
            
            UpdateUI();
        }

        private void ui_menu_file_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void ui_menu_file_new_folder_Click(object sender, RoutedEventArgs e)
        {
            NewFolderWindow newFolderWindow = new NewFolderWindow(editorPanelManager);
            newFolderWindow.ShowDialog();

            UpdateUI();
        }

        private void ui_menu_file_new_script_Click(object sender, RoutedEventArgs e)
        {
            NewScriptWindow newScriptWindow = new NewScriptWindow(editorPanelManager);
            newScriptWindow.ShowDialog();

            UpdateUI();
        }

        private void ui_menu_file_new_dataSet_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        //---------------------------MENU PROJECT---------------------------
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

        private void ui_menu_project_buildConfiguration_Click(object x, RoutedEventArgs y)
        {
            editorPanelManager.Menu_OpenBuildConfig();

            UpdateUI();
        }

        private void ui_menu_project_projectSettings_Click(object sender, RoutedEventArgs e)
        {
            ProjectWindow projectWindow = new ProjectWindow(this, editorPanelManager, true);
            projectWindow.Show();

            UpdateUI();
        }

        //---------------------------MENU HELP---------------------------
        private void ui_menu_help_about_Click(object sender, RoutedEventArgs e)
        {
            About abt = new About();
            abt.ShowDialog();

            UpdateUI();
        }

        private void ui_menu_help_docs_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_help_contribute_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_help_debug_verboseOutput_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_help_debug_showProjectInfo_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show($".tseproj version - {projectManager.project.tseproj.version}\n\nProject Name - {projectManager.project.project.name}\nProject Version - {projectManager.project.project.version}\nProject Author - {projectManager.project.project.author}\n\nTool Game - {projectManager.project.tool.game}\nTool Executable - {projectManager.project.tool.executable}\nTool Master Priority - {projectManager.project.tool.master_priority}", "Debug - Project Info");

            UpdateUI();
        }

        private void ui_menu_help_printConsoleToFile_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_PrintOutputToFile();

            UpdateUI();
        }

        //---------------------------MENU EDIT---------------------------
        private void ui_menu_edit_editorSettings_Click(object sender, RoutedEventArgs e)
        {
            EditorSettingsWindow editorSettingsWindow = new EditorSettingsWindow();
            editorSettingsWindow.Show();

            UpdateUI();
        }

        private void ui_menu_edit_undo_Click(object sender, RoutedEventArgs e)
        {
            ui_editor_textEditor.Undo();

            UpdateUI();
        }

        private void ui_menu_edit_redo_Click(object sender, RoutedEventArgs e)
        {
            ui_editor_textEditor.Redo();

            UpdateUI();
        }

        private void ui_menu_edit_copy_Click(object sender, RoutedEventArgs e)
        {
            if (editorPanelManager.CurrentlyOpen_IsLua())
            {
                ui_editor_textEditor.Copy();
            }

            UpdateUI();
        }

        private void ui_menu_edit_paste_Click(object sender, RoutedEventArgs e)
        {
            if (editorPanelManager.CurrentlyOpen_IsLua())
            {
                ui_editor_textEditor.Paste();
            }

            UpdateUI();
        }

        private void ui_menu_edit_texture_convertToDDS_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_Texture_ConvertToDDS();

            UpdateUI();
        }

        private void ui_menu_edit_texture_convertToD3DTX_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_Texture_ConvertToD3DTX();

            UpdateUI();
        }

        private void ui_menu_edit_script_compile_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_menu_edit_script_decompile_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        //---------------------------PROJECT TREE CONTEXT MENU---------------------------
        private void ui_editor_projectTree_contextMenu_refresh_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.projectManager.fileTreeManager.RepopulateFileTree();

            UpdateUI();
        }

        private void ui_editor_projectTree_contextMenu_import_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_Import();

            UpdateUI();
        }

        private void ui_editor_projectTree_contextMenu_delete_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_Delete();

            UpdateUI();
        }

        private void ui_editor_projectTree_contextMenu_openExplorer_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.ContextMenu_OpenInExplorer();

            UpdateUI();
        }

        private void ui_editor_projectTre_contextMenu_Opened(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_editor_projectTree_contextMenu_new_script_Click(object sender, RoutedEventArgs e)
        {
            NewScriptWindow newScriptWindow = new NewScriptWindow(editorPanelManager);
            newScriptWindow.ShowDialog();

            UpdateUI();
        }

        private void ui_editor_projectTree_contextMenu_new_folder_Click(object sender, RoutedEventArgs e)
        {
            NewFolderWindow newFolderWindow = new NewFolderWindow(editorPanelManager);
            newFolderWindow.ShowDialog();

            UpdateUI();
        }
        //---------------------------CONSOLE OUTPUT CONTEXT MENU---------------------------
        private void ui_editor_consoleOutput_contextmenu_verboseOutput_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ui_editor_consoleOutput_contextmenu_textWrapping_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }
        //---------------------------TEXT EDITOR CONTEXT MENU---------------------------
        private void ui_editor_textEditor_TextChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_undo_Click(object sender, RoutedEventArgs e)
        {
            ui_editor_textEditor.Undo();

            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_redo_Click(object sender, RoutedEventArgs e)
        {
            ui_editor_textEditor.Redo();

            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_copy_Click(object sender, RoutedEventArgs e)
        {
            if (editorPanelManager.CurrentlyOpen_IsLua())
            {
                ui_editor_textEditor.Copy();
            }

            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_paste_Click(object sender, RoutedEventArgs e)
        {
            if (editorPanelManager.CurrentlyOpen_IsLua())
            {
                ui_editor_textEditor.Paste();
            }

            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_save_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveCurrentFile();

            UpdateUI();
        }

        private void ui_editor_textEditor_contextMenu_saveAs_Click(object sender, RoutedEventArgs e)
        {
            editorPanelManager.Menu_SaveCurrentFileAs();

            UpdateUI();
        }














        //I was experimenting with SharpGL.WPF - might reinstall in the future, removed for now.
        /*
        /// <summary>
        /// The current rotation.
        /// </summary>
        private float rotation = 0.0f;


        private void ui_openGL_window_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            //  Get the OpenGL object.
            OpenGL gl = ui_openGL_window.OpenGL;

            //  Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //  Load the identity matrix.
            gl.LoadIdentity();

            //  Rotate around the Y axis.
            gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);

            //  Draw a coloured pyramid.
            gl.Begin(OpenGL.GL_TRIANGLES);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 1.0f, 0.0f);
            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 1.0f);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(1.0f, -1.0f, 1.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 1.0f, 0.0f);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(1.0f, -1.0f, 1.0f);
            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(1.0f, -1.0f, -1.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 1.0f, 0.0f);
            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(1.0f, -1.0f, -1.0f);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(-1.0f, -1.0f, -1.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 1.0f, 0.0f);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(-1.0f, -1.0f, -1.0f);
            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 1.0f);
            gl.End();

            //  Nudge the rotation.
            rotation += 3.0f;
        }

        private void ui_openGL_window_Initialized(object sender, EventArgs e)
        {
            //  TODO: Initialise OpenGL here.

            //  Get the OpenGL object.
            OpenGL gl = ui_openGL_window.OpenGL;

            //  Set the clear color.
            gl.ClearColor(0, 0, 0, 0);
        }

        private void ui_openGL_window_Resized(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            //  TODO: Set the projection matrix here.

            //  Get the OpenGL object.
            OpenGL gl = ui_openGL_window.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //  Create a perspective transformation.
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            gl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }*/
    }
}
