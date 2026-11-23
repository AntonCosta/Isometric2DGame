using UnityEngine;

namespace Dialogue
{
    public class ResponseController : MonoBehaviour
    {
        public ResponseModel ResponseModel { get; set; }
        
        [SerializeField] private DialogueManager _dialogueManager;
        
        public void ResponsePressed()
        {
            _dialogueManager.NextDialogueLine(ResponseModel.NextDialogueId);
        }
    }
}