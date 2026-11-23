using Dialogue;
using UnityEngine;

namespace NPC
{
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private int _dialogueId;
        [SerializeField] private string _name;
        [SerializeField] private DialogueManager _dialogueManager;

        public void OnInteract()
        {
            _dialogueManager.StartDialogue(_dialogueId, _name);
        }
    }
}