using TMPro;
using UnityEngine;

namespace Dialogue
{
    public class ResponseController : MonoBehaviour
    {
        public ResponseModel ResponseModel { get; set; }
        public string ResponseText => _responseText.text;

        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private TextMeshProUGUI _responseText;
        
        public void ClearText()
        {
            _responseText.text = string.Empty;
        }

        public void SetText()
        {
            _responseText.text = ResponseModel.Text;
        }

        public void ResponsePressed()
        {
            _dialogueManager.NextDialogueLine(ResponseModel.NextDialogueId);
        }
    }
}