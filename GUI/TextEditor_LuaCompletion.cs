using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;

namespace Telltale_Script_Editor.GUI
{
    public class TextEditor_LuaCompletion
    {
        private void ShowNewList(CompletionWindow completionWindow)
        {
            completionWindow.CompletionList.CompletionData.Clear();

            completionWindow.Show();
            completionWindow.Closed += delegate { completionWindow = null; };
        }

        public void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e, CompletionWindow completionWindow, TextEditor textEditor)
        {
            if (e.Text != null)
            {
                //create a completion window
                completionWindow = new CompletionWindow(textEditor.TextArea);

                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData; //fill it up with data corresponding to the entered text

                if (e.Text == "a" || e.TextComposition.ControlText == "an")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("and"));
                }
                else if (e.Text == "e")
                {
                    //clear the list
                    completionWindow.CompletionList.CompletionData.Clear(); //clear the list

                    //add the elements
                    data.Add(new TextEditor_CompletionData("end"));
                    data.Add(new TextEditor_CompletionData("else"));
                    data.Add(new TextEditor_CompletionData("elseif"));

                }
                else if (e.Text == "i")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("in"));
                    data.Add(new TextEditor_CompletionData("if"));
                }
                else if (e.Text == "r")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("repeat"));
                    data.Add(new TextEditor_CompletionData("return"));
                }
                else if (e.Text == "b")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("break"));
                }
                else if (e.Text == "f")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("false"));
                    data.Add(new TextEditor_CompletionData("for"));
                    data.Add(new TextEditor_CompletionData("function"));
                }
                else if (e.Text == "l")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("local"));
                }
                else if (e.Text == "d")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("do"));
                }
                else if (e.Text == "n")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("nil"));
                    data.Add(new TextEditor_CompletionData("not"));
                }
                else if (e.Text == "t")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("then"));
                    data.Add(new TextEditor_CompletionData("true"));
                }
                else if (e.Text == "o")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("or"));
                }
                else if (e.Text == "u")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("until"));
                }
                else if (e.Text == "w")
                {
                    //clear the list
                    ShowNewList(completionWindow);

                    //add the elements
                    data.Add(new TextEditor_CompletionData("while"));
                }
            }
        }
    }
}
