using System;
using System.Collections.Generic;
using UnityEngine;


namespace Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueBoxController _dialogueBoxController;
        
        private List<DialogueModel> _dialogueModels => _dialogues.Dialogues;
        private List<ResponseModel> _responseModels => _responses.Responses;
        private DialogueListModel _dialogues = new ();
        private ResponseListModel _responses = new ();
        
        private int _currentDialogueId;

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
            _dialogueBoxController.SetDialogueSpeaker(speaker);
            _dialogueBoxController.SetDialogueText(GetDialogueText(dialogueId));
            _currentDialogueId = dialogueId;
        }

        public string GetDialogueText(int dialogueId)
        {
            return _dialogueModels.Find(dialogue => dialogue.Id == dialogueId).Text;
        }
        
        public List<int> GetDialogueResponses(int dialogueId)
        {
            return _dialogueModels.Find(dialogue => dialogue.Id == dialogueId).PossibleResponse;
        }

        public DialogueModel GetDialogueById(int dialogueId)
        {
            return _dialogueModels.Find(dialogue => dialogue.Id == dialogueId);
        }

        public void NextDialogueLine(int nextId)
        {
            _dialogueBoxController.SetDialogueText(GetDialogueText(nextId));
            _currentDialogueId = nextId;
        }

        public void OnTextWritten()
        {
            var responses = new List<ResponseModel>();
            var responseIds = GetDialogueResponses(_currentDialogueId);
            _responseModels.ForEach(response =>
            {
                if (responseIds.Contains(response.Id))
                {
                    responses.Add(response);
                }
            });
            _dialogueBoxController.SetDialogueResponses(responses);
        }
    }
}
