using System;
using Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC
{
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private int _dialogueId;
        [SerializeField] private string _name;
        [SerializeField] private Animator _animator;
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private GameObject _tutorialPopUp;
        
        private InputAction _interactAction;
        private bool _playerInRange;
        private Transform _playerTransform;

        private void Start()
        {
            _interactAction = InputSystem.actions.FindAction(Utils.INTERACT_ACTION);
        }

        public void OnInteract()
        {
            _dialogueManager.StartDialogue(_dialogueId, _name);
            _tutorialPopUp.SetActive(false);
            FacePlayer();
        }

        private void FacePlayer()
        {
            var directionToPlayer = _playerTransform.position - transform.position;
            directionToPlayer.Normalize();
            
            _animator.SetFloat(Utils.MOVE_X_HASH, directionToPlayer.x);
            _animator.SetFloat(Utils.MOVE_Y_HASH, directionToPlayer.y);
        }
        
        private void Update()
        {
            if (!_playerInRange)
            {
                return;
            }

            if (_interactAction.WasPressedThisFrame())
            {
                OnInteract();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Utils.PLAYER_TAG))
            {
                _playerInRange = true;
                _playerTransform = other.transform;
                _tutorialPopUp.SetActive(true);
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Utils.PLAYER_TAG))
            {
                _playerInRange = false;
                _playerTransform = null;
                _tutorialPopUp.SetActive(false);
            }
        }
    }
}