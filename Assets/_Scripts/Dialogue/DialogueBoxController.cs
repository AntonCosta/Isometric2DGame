using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private List<TextMeshProUGUI> _dialogueResponses;

        private string _fullText;
        private Coroutine _typingCoroutine;
        private bool _isTyping;
        private InputAction _skipAction;

        private void Awake()
        {
            _dialogueSpeaker.text = "";
            _dialogueText.text = "";
        }

        private void Start()
        {
            _skipAction = InputSystem.actions.FindAction("Jump");
        }

        private void Update()
        {
            if (_skipAction.WasPressedThisFrame() && _isTyping)
            {
               SkipDialogue();
            }
        }

        public void StartDialogue()
        {
            _dialogueHolder.SetActive(true);
        }

        public void EndDialogue()
        {
            _dialogueHolder.SetActive(false);
        }

        public void SetDialogueSpeaker(string text)
        {
            _dialogueSpeaker.text = text;
        }

        public void SetDialogueText(string text)
        {
            _fullText = text;
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _typingCoroutine = StartCoroutine(TypeText());
        }
        
        public void SetDialogueResponses(List<ResponseModel> possibleResponses)
        {
            for (var i = 0; i < possibleResponses.Count; i++)
            {
                _dialogueResponses[i].text = possibleResponses[i].Text;
            }
            _dialogueResponses.ForEach(response => response.gameObject.SetActive(response.text != ""));
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

        public void OnResponsePressed(int nextId)
        {
            
        }
    }
}