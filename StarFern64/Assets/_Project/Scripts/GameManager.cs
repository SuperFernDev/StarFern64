using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace RailShooter
{
    public class GameManager : ValidatedMonoBehaviour
    {
        public static GameManager instance;
        public enum gameMode { TITLE, STAR_FERN }
        public static gameMode currentMode;

        [Header("----- Game Mode -----")]
        [SerializeField] gameMode modeSelection;

        [Header("----- Title Player Reference -----")]
        public GameObject player;
        [SerializeField] Transform launchTarget;
        [SerializeField] float launchTime;
        [SerializeField] float smoothTime = 0.2f;
        Vector3 velocity;

        [Header("----- EventSystem -----")]
        public GameObject eventSystem;
        [SerializeField, Self] AudioSource aud;

        [Header("----- Main Menu -----")]
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject mainMenuFirst;

        [Header("-----Menu Variables -----")]
        [SerializeField] GameObject menuActive;
        [SerializeField] GameObject pauseMenuFirst;
        [SerializeField] float menuTimeDelay;        

        [Header("----- Pause Menu and HUD -----")]
        [SerializeField] GameObject menuPause;
        [SerializeField] TMP_Text score;
        [SerializeField] TMP_Text highScore;

        [Header("----- Results Screen -----")]
        [SerializeField] GameObject menuResults;
        [SerializeField] GameObject resultsMenuFirst;
        [SerializeField] Image[] stars;
        [SerializeField] TMP_Text resultsScore;
        [SerializeField] TMP_Text resultsEliminations;
        [SerializeField] int starThresh1;
        [SerializeField] int starThresh2;
        [SerializeField] int starThresh3;
        [SerializeField] float alphaIncrease;
        [SerializeField] float starAppearanceRate;

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip menuPopUP;
        [Range(0, 1)][SerializeField] float menuPopUPVol;
        [SerializeField] AudioClip menuOff;
        [Range(0, 1)][SerializeField] float menuOffVol;

        int currentScore;
        public int eliminations;
        public bool isPaused;
        bool fullScreen;

        void Awake()
        {
            instance = this;
            currentMode = modeSelection;
            if (currentMode == GameManager.gameMode.TITLE)
                EventSystem.current.SetSelectedGameObject(mainMenuFirst);
            else
                UpdateHighScore();
        }

        private void Start()
        {
            eliminations = 0;
            PlatformController.singleton.Init("COM9", 115200);
        }

        private void Update()
        {
            if (currentMode == GameManager.gameMode.TITLE)
                PlatformController.singleton.Heave = player.transform.position.y * 5;
            print(PlatformController.singleton.Heave);
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
            if (menuActive != null)
                menuActive.SetActive(false);
            menuActive = null;
        }

        public void OnPause()
        {
            if (currentMode == GameManager.gameMode.TITLE) //Added to stop cursor from locking. InputReader was somehow bypassing the checks and running StateUnpause
                return;
            if (menuActive == null)
            {
                StatePause();
                aud.PlayOneShot(menuPopUP, menuPopUPVol);
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
                StartCoroutine(SetFirstSelected(pauseMenuFirst));
            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
                aud.PlayOneShot(menuOff, menuOffVol);
            }
        }

        public void UpdateScore(int scoreVal, int elimination = 0)
        {
            currentScore += scoreVal;
            score.text = currentScore.ToString();
            eliminations += elimination;
            CheckHighScore();
        }

         void CheckHighScore()
        {
            if (currentScore > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", currentScore);    
                UpdateHighScore();
            }
        }

        void UpdateHighScore()
        {
            highScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        }

        // RESULTS SCREEN //
        public void ResultsScreen()
        {
            if(menuActive != null)
                menuActive.SetActive(false);
            isPaused = !isPaused;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            //StatePause();
            menuActive = menuResults;
            menuActive.SetActive(true);
            FinalScore();
            StartCoroutine(SetFirstSelected(resultsMenuFirst));
        }

        void FinalScore()
        {
            resultsScore.text = currentScore.ToString();
            resultsEliminations.text = eliminations.ToString();
            StarResults();
        }

        void StarResults()
        {
            int starsToAdd = 0;
            if (currentScore >= starThresh3)
                starsToAdd = 3;
            else if (currentScore >= starThresh2)
                starsToAdd = 2;
            else if (currentScore >= starThresh1)
                starsToAdd = 1;

            for (int i = 0; i < starsToAdd; i++)
            {
                stars[i].enabled = true;
            }
        }

        public IEnumerator MainMenuShipLaunch()
        {
            mainMenu.SetActive(false);
            player.GetComponent<SpinAndBob>().enabled = false;
            foreach (var trail in player.GetComponent<PlayerController>().trailEffects)
            {
                trail.GetComponent<TrailRenderer>().enabled = true;
            }

            Vector3 velocity = Vector3.zero; // Ensure velocity is initialized
            Vector3 targetPosition = launchTarget.position + launchTarget.forward;

            while (Vector3.Distance(player.transform.position, targetPosition) > 0.01f) // Check if we're close to the target
            {
                // Smoothly move towards the target
                player.transform.position = Vector3.SmoothDamp(player.transform.position, targetPosition, ref velocity, smoothTime);
                // Wait for the next frame
                yield return null;
            }

            // Ensure the player snaps exactly to the target at the end
            player.transform.position = targetPosition;

            SceneManager.LoadScene("StarFern64");
            currentMode = gameMode.STAR_FERN;
            StateUnpause();
        }

        public IEnumerator SetFirstSelected(GameObject selection)
        {
            if (player.GetComponent<PlayerInput>().currentControlScheme == "Keyboard&Mouse")
                menuTimeDelay = 0;
            yield return new WaitForSeconds(menuTimeDelay);
            EventSystem.current.SetSelectedGameObject(selection);
        }
    }
}
