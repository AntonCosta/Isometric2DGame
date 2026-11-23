using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [Serializable]
    public class DialogueListModel
    {
        public List<DialogueModel> Dialogues;
    }
    
    [Serializable]
    public class DialogueModel
    {
        public int Id;
        public string Text;
        public List<int> PossibleResponse;
        public string Type; //Positive, Negative, Neutral
    }
}