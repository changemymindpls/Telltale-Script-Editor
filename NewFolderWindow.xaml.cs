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
    /// Interaction logic for NewItemWindow.xaml
    /// </summary>
    public partial class NewFolderWindow : Window
    {
        //our main objects
        private MessageBoxes messageBoxes;
        private EditorPanelManager editorPanelManager;

        /// <summary>
        /// Creates a window for creating a new folder in the project directory.
        /// </summary>
        /// <param name="editorPanelManager"></param>
        public NewFolderWindow(EditorPanelManager editorPanelManager)
        {
            //xaml initalization
            InitializeComponent();

            //get our editorPanelManager object
            this.editorPanelManager = editorPanelManager;

            //initalizes our own objects
            InitalizeWindow();
        }

        /// <summary>
        /// Our own window initalization.
        /// </summary>
        private void InitalizeWindow()
        {
            //creates a message box object for our custom message boxes.
            messageBoxes = new MessageBoxes();
        }

        /// <summary>
        /// Performs a few checks before proceeding with an action. Will return true if it's sucessful, and false if it isn't.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool PreChecks(string name)
        {
            if (name.Equals("Folder Name")) //check if the name matches the default field text (means they didn't do anything)
            {
                //give them an error because they haven't done anything!
                messageBoxes.Error("Folder Name Improper", "You haven't edited the Folder Name field!");

                //precheck failed so return false
                return false;
            }
            else if (name.Contains(" ")) //check if there are any spaces in the folder name
            {
                //give them an error because we can't put spaces in our folder names!
                messageBoxes.Error("Folder Name Improper", "Folder names can't have spaces in them!");

                //precheck failed so return false
                return false;
            }
            else if(Directory.Exists(editorPanelManager.projectManager.GetWorkingDirectory() + "/" + name)) //check if there is a folder with the same name that already exists
            {
                //give them an error because they already have a folder with the same name!
                messageBoxes.Error("Folder Already Exists!", "The folder you are trying to create already exists!");

                //precheck failed so return false
                return false;
            }

            //if none of the statements are tripped, then we passed, so return true!
            return true;
        }

        //------------------------------------------- XAML FUNCTIONS -------------------------------------------

        private void ui_cancel_button_Click(object sender, RoutedEventArgs e)
        {
            //close the window cause they changed their mind and don't want to do anything
            Close();
        }

        private void ui_createFolder_button_Click(object sender, RoutedEventArgs e)
        {
            //do a precheck with what the user has in the field before we create the folder
            if(PreChecks(ui_folderName_textbox.Text))
            {
                //call the main editor panel manager script to create the folder
                editorPanelManager.Menu_CreateNewFolder(ui_folderName_textbox.Text);

                //close the window since we are done.
                Close();
            }
        }
    }
}
