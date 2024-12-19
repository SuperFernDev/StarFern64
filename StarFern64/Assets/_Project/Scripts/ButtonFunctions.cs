using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RailShooter
{
    public class ButtonFunctions : MonoBehaviour
    {
        public void playGame()
        {
            StartCoroutine(GameManager.instance.MainMenuShipLaunch());            
        }

        public void resume()
        {
            GameManager.instance.StateUnpause();
        }

        public void restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            GameManager.instance.StateUnpause();
        }

        public void mainMenu()
        {
            SceneManager.LoadScene("Title");
            GameManager.instance.isPaused = !GameManager.instance.isPaused;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            GameManager.currentMode = GameManager.gameMode.TITLE;

        }

        public void quit()
        {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
            Application.Quit();

#endif
        }

    }
}
