using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Dialogue.Editor
{
    public class DialogueNodeView : Node
    {
        public string Guid;
        public int DialogueId;
        public string DialogueText;
        public DialogueModel DialogueModel;
    }
}