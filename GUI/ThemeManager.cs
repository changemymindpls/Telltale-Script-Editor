using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_Script_Editor.GUI
{
    public class ThemeManager
    {
        /// <summary>
        /// Sets the program to use the desired theme.
        /// </summary>
        /// <param name="x">The theme to use</param>
        public static void SetTheme(IBaseTheme x)
        {
            var ph = new PaletteHelper();
            ITheme theme = ph.GetTheme();

            theme.SetBaseTheme(x);
            ph.SetTheme(theme);
        }
    }
}
