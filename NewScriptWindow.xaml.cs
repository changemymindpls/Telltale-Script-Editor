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
    /// Interaction logic for NewScriptWindow.xaml
    /// </summary>
    public partial class NewScriptWindow : Window
    {
        //our custom objects to use
        private MessageBoxes messageBoxes;
        private EditorPanelManager editorPanelManager;

        /// <summary>
        /// Creates a window for creating a new script.
        /// </summary>
        /// <param name="editorPanelManager"></param>
        public NewScriptWindow(EditorPanelManager editorPanelManager)
        {
            //xaml initalization
            InitializeComponent();

            //get our editor panel manager object
            this.editorPanelManager = editorPanelManager;

            //initalize our own objects
            InitalizeWindow();
        }

        /// <summary>
        /// Initalizes our own custom objects for this window.
        /// </summary>
        private void InitalizeWindow()
        {
            //create our message box object so we can do our custom message boxes
            messageBoxes = new MessageBoxes();

            //fill the combobox with the folder names from the project directory
            ui_folderName_comobox.ItemsSource = editorPanelManager.GetFolderNames_From_ProjectDirectory();
        }

        /// <summary>
        /// Peforms a bunch of pre-checks before we can create a script. Returns true if successful, returns false if it failed.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        private bool PreCheck(string scriptName, string folderName)
        {
            //create a temp variable for the possible case where we already have a script with the same name in the same place
            string scriptPath = editorPanelManager.projectManager.GetWorkingDirectory() + "/" + scriptName + ".lua";

            if (ui_folderName_comobox.SelectedIndex < 0) //check if they even selected a folder (by default it's -1 if they didn't)
            {
                //give them an error and tell them to select a darn folder for where the script will be placed!
                messageBoxes.Error("No Folder Selected", "Select a folder where the script will be moved to!");

                //pre check failed so return false
                return false;
            }
            else if (scriptName.Equals("Script Name")) //if the name matches the default text field then they didn't do anything!
            {
                //give them an error and tell them to name the script properly!
                messageBoxes.Error("Script Name Improper", "You haven't edited the Folder Name field!");

                //pre check failed so return false
                return false;
            }
            else if (scriptName.Contains(" ")) //if the name contains spaces then thats a no no
            {
                //give them an error and tell them to name the script without spaces!
                messageBoxes.Error("Script Name Improper", "You can't have spaces in the script name!");

                //pre check failed so return false
                return false;
            }
            else if(File.Exists(scriptPath)) //if the script we are trying to create already exists then we have a special case
            {
                //warn the user that we already have a script with the same name in the same place, but mabye they want to overwrite it? so let's ask them
                if(messageBoxes.Warning_Confirm("Script Already Exists", "There is already a script of the same name in the folder you are trying to create! Do you want to overwrite it with a new one?"))
                {
                    //if they do want to overwrite it, then proceed and the file will be overwritten by default
                    return true;
                }
                else
                {
                    //if they don't want to, then return false and then pre check fails
                    return false;
                }
            }

            //if none of the statements tripped then sucess!
            return true;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_createScript_button_Click(object sender, RoutedEventArgs e)
        {
            //do some pre checks before we make our script
            if(PreCheck(ui_scriptName_textbox.Text, (string)ui_folderName_comobox.SelectedItem))
            {
                //calls the main editor panel manager object to create our script
                editorPanelManager.Menu_CreateNewScript(ui_scriptName_textbox.Text, (string)ui_folderName_comobox.SelectedItem);

                //close the window cause we are done.
                Close();
            }
        }

        private void ui_cancel_button_Click(object sender, RoutedEventArgs e)
        {
            //close the window since they changed their mind and there is nothing else to do.
            Close();
        }
    }
}
