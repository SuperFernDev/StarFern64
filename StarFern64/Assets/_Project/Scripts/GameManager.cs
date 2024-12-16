using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RailShooter
{
    public class GameManager : ValidatedMonoBehaviour
    {
        public static GameManager instance;
        public enum gameMode { TITLE, STAR_FERN }
        public static gameMode currentMode;

        [Header("EventSystem")]
        public GameObject eventSystem;
        [SerializeField, Self] AudioSource aud;

        [Header("Menu Variables")]
        [SerializeField] GameObject menuActive;
        //[SerializeField] GameObject menuScore; // TODO: The UI for when Game Ends
        //[SerializeField] GameObject scoreDisplay;
        [SerializeField] TMP_Text score;
        [SerializeField] int maxScoreLength; //max length of score text

        [SerializeField] GameObject menuPause;
        //[SerializeField] GameObject menuControls;

        [Header("Menu First Selected Options")]
        //[SerializeField] GameObject mainMenuFirst;
        [SerializeField] GameObject pauseMenuFirst;
        //[SerializeField] GameObject settingsMenuFirst;
        //[SerializeField] GameObject controlsMenuFirst;

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip menuPopUP;
        [Range(0, 1)][SerializeField] float menuPopUPVol;
        [SerializeField] AudioClip menuOff;
        [Range(0, 1)][SerializeField] float menuOffVol;


        int currentScore;
        public bool isPaused;
        bool fullScreen;

        void Awake()
        {
            instance = this;

        }

        public void StatePause()
        {
            isPaused = !isPaused;
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void StateUnpause()
        {
            isPaused = !isPaused;
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            menuActive.SetActive(false);
            menuActive = null;
        }

        public void OnPause()
        {
            if (menuActive == null)
            {
                StatePause();
                aud.PlayOneShot(menuPopUP, menuPopUPVol);
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
                EventSystem.current.SetSelectedGameObject(pauseMenuFirst);
            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
                aud.PlayOneShot(menuOff, menuOffVol);
            }
        }

        public void UpdateScore(int amount)
        {
            currentScore += amount;
            score.text = currentScore.ToString();

            //while (score.text.Length < maxScoreLength)
            //{
            //    score.text.Insert(0, "0");
            //}
        }
    }
}
