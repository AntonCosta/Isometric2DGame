using System;
using System.Collections.Generic;
using UnityEngine;


namespace Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public bool DialogueOpen => _dialogueOpen;
        
        [SerializeField] private DialogueBoxController _dialogueBoxController;
        
        private List<DialogueModel> DialogueModels => _dialogues.Dialogues;
        private List<ResponseModel> ResponseModels => _responses.Responses;
        private DialogueListModel _dialogues = new ();
        private ResponseListModel _responses = new ();
        
        private int _currentDialogueId;
        private bool _dialogueOpen;

        private void Awake()
        {
            var dialogueJsonFile = Resources.Load<TextAsset>("Dialogues");
            var responseJsonFile = Resources.Load<TextAsset>("Responses");
            _dialogues = JsonUtility.FromJson<DialogueListModel>(dialogueJsonFile.text);
            _responses = JsonUtility.FromJson<ResponseListModel>(responseJsonFile.text);
        }

        private void Start()
        {
            _dialogueBoxController.OnTextWritten += OnTextWritten;
        }

        public void StartDialogue(int dialogueId, string speaker)
        {
            _dialogueBoxController.StartDialogue();
            _dialogueBoxController.SetDialogueSpeaker(speaker);
            _dialogueBoxController.SetDialogueText(GetDialogueText(dialogueId));
            _currentDialogueId = dialogueId;
            _dialogueOpen = true;
        }

        public string GetDialogueText(int dialogueId)
        {
            return DialogueModels.Find(dialogue => dialogue.Id == dialogueId).Text;
        }
        
        public List<int> GetDialogueResponses(int dialogueId)
        {
            return DialogueModels.Find(dialogue => dialogue.Id == dialogueId).PossibleResponse;
        }

        public DialogueModel GetDialogueById(int dialogueId)
        {
            return DialogueModels.Find(dialogue => dialogue.Id == dialogueId);
        }

        public void NextDialogueLine(int nextId)
        {
            if (nextId == 0)
            {
                EndDialogue();
            }
            _dialogueBoxController.SetDialogueText(GetDialogueText(nextId));
            _currentDialogueId = nextId;
        }

        public void OnTextWritten()
        {
            var responses = new List<ResponseModel>();
            var responseIds = GetDialogueResponses(_currentDialogueId);
            ResponseModels.ForEach(response =>
            {
                if (responseIds.Contains(response.Id))
                {
                    responses.Add(response);
                }
            });
            _dialogueBoxController.SetDialogueResponses(responses);
        }

        public void EndDialogue()
        {
            _dialogueBoxController.EndDialogue();
        }

        public void CloseDialogue()
        {
            _dialogueOpen = false;
        }
    }
}
