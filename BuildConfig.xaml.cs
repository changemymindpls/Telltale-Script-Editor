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

namespace Telltale_Script_Editor
{
    /// <summary>
    /// Interaction logic for BuildConfig.xaml
    /// </summary>
    public partial class BuildConfig : Window
    {
        /// <summary>
        /// Build configuration menu.
        /// </summary>
        /// <remarks>
        /// This constructor should not be used - will result in a non-functional window.
        /// </remarks>
        public BuildConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Build configuration menu - main constructor.
        /// </summary>
        /// <param name="x">The project manager to be used.</param>
        public BuildConfig(ProjectManager x)
        {
            InitializeComponent();
            FileTreeManager cfgManager = new FileTreeManager(cfgTreeView, x.GetWorkingDirectory());
        }
    }
}
