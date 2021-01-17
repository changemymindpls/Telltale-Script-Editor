using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_Script_Editor.Utils
{
    public class ScriptEditorProject
    {
        public JsonProperties tseproj { get; set; }
        public ProjectProperties project { get; set; }
        public ToolProperties tool { get; set; }
    }
}
