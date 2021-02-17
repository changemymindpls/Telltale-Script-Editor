using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_Script_Editor.Utils
{
    public class Enumerators
    {
        public enum GameVersion
        {
            The_Walking_Dead_Telltale_Definitive_Series = 0,
            The_Walking_Dead_Final_Season = 1
        }

        public List<string> GameVersion_NamesList()
        {
            List<string> names = new List<string>(Enum.GetNames(typeof(GameVersion)));

            return names;
        }

        public List<string> GameVersion_NamesList_WithSpaces()
        {
            List<string> cleanNames = new List<string>();

            foreach(string name in Enum.GetNames(typeof(GameVersion)))
            {
                cleanNames.Add(name.Replace("_", " "));
            }

            return cleanNames;
        }

        public string GameVersion_GetName(int value)
        {
            string stringValue = Enum.GetName(typeof(GameVersion), value);

            return stringValue;
        }

        public GameVersion GameVersion_ParseValue_Name(string value)
        {
            int intValue = (int)Enum.Parse(typeof(GameVersion), value);

            return (GameVersion)intValue;
        }

        public GameVersion GameVersion_ParseValue_Value(int value)
        {
            return (GameVersion)value;
        }
    }
}
