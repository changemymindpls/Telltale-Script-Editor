using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_Script_Editor.Utils
{
    public class ScriptEditorProject
    {
        public JsonProperties Tseproj { get; set; }
        public ProjectProperties Project { get; set; }
        public ToolProperties Tool { get; set; }
    }
}
