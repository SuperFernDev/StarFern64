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
        public void resume()
        {
            GameManager.instance.StateUnpause();
        }

        public void restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            GameManager.instance.StateUnpause();
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
