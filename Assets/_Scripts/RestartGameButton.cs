using System;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class RestartGameButton : MonoBehaviour
    {
        private void Start()
        {
            var playerController = FindObjectsByType<PlayerController>();
            playerController[0].OnPlayerDeath += () => gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}