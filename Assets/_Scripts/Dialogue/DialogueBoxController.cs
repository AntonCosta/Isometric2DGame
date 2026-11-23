using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Dialogue
{
    public class DialogueBoxController : MonoBehaviour
    {
        public Action OnTextWritten;
        
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private GameObject _dialogueHolder;
        [SerializeField] private float _typingSpeed = 0.05f;
        [SerializeField] private TextMeshProUGUI _dialogueSpeaker;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Button _exitButton;
        [SerializeField] private List<ResponseController> _dialogueResponses;

        private string _fullText;
        private Coroutine _typingCoroutine;
        private bool _isTyping;
        private InputAction _skipAction;
        private bool _canClose;
        private bool _dialogueOpen;

        private void Awake()
        {
            _dialogueSpeaker.text = "";
            _dialogueText.text = "";
            _dialogueResponses.ForEach(response => response.gameObject.SetActive(false));
            EndDialogue();
            _exitButton.onClick.AddListener(EndDialogue);
        }

        private void Start()
        {
            _skipAction = InputSystem.actions.FindAction("Jump");
        }

        private void Update()
        {
            if (_skipAction.WasPressedThisFrame())
            {
                if (_isTyping)
                {
                    SkipDialogue();
                }
                else if (_canClose)
                {
                    EndDialogue();
                }
            }  
        }

        public void StartDialogue()
        {
            _dialogueHolder.SetActive(true);
        }

        public void EndDialogue()
        {
            _dialogueHolder.SetActive(false);
            _dialogueManager.CloseDialogue();
        }

        public void SetDialogueSpeaker(string text)
        {
            _dialogueSpeaker.text = text;
        }

        public void SetDialogueText(string text)
        {
            _dialogueResponses.ForEach(response =>
            {
                response.ClearText();
                response.gameObject.SetActive(false);
            });

            _fullText = text;
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _typingCoroutine = StartCoroutine(TypeText());
        }
        
        public void SetDialogueResponses(List<ResponseModel> possibleResponses)
        {
            _canClose = possibleResponses.Count == 0;
            for (var i = 0; i < possibleResponses.Count; i++)
            {
                _dialogueResponses[i].ResponseModel = possibleResponses[i];
                _dialogueResponses[i].SetText();
            }
            _dialogueResponses.ForEach(response => response.gameObject.SetActive(response.ResponseText != ""));
        }

        private IEnumerator TypeText()
        {
            _isTyping = true;
            _dialogueText.text = "";

            foreach (var character in _fullText)
            {
                _dialogueText.text += character;
                yield return new WaitForSeconds(_typingSpeed);
            }
            
            OnTextWritten?.Invoke();
            _isTyping = false;
            _typingCoroutine = null;
        }
        
        public void SkipDialogue()
        {
            if (_isTyping)
            {
                StopCoroutine(_typingCoroutine);
                _dialogueText.text = _fullText;
                _isTyping = false;
                _typingCoroutine = null;
                OnTextWritten?.Invoke();
            }
        }
    }
}