using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class ResponseListModel
    {
        public List<ResponseModel> Responses;
    }
    
    [Serializable]
    public class ResponseModel
    {
        public int Id;
        public string Text;
        public int NextDialogueId;
    }
}