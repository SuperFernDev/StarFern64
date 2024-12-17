using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace RailShooter
{
    public class GameManager : ValidatedMonoBehaviour
    {
        public static GameManager instance;
        public enum gameMode { TITLE, STAR_FERN }
        public static gameMode currentMode;

        [Header("----- Player Reference -----")]
        [SerializeField] Transform player;
        [SerializeField] Transform launchTarget;
        [SerializeField] float launchTime;
        [SerializeField] float movementSpeed = 10f;
        Vector3 velocity;
        float smoothTime = 0.2f;

        [Header("EventSystem")]
        public GameObject eventSystem;
        [SerializeField, Self] AudioSource aud;

        [Header("Menu Variables")]
        [SerializeField] gameMode modeSelection;
        [SerializeField] GameObject menuActive;
        //[SerializeField] GameObject menuScore; // TODO: The UI for when Game Ends
        //[SerializeField] GameObject scoreDisplay;
        [SerializeField] TMP_Text score;
        [SerializeField] int maxScoreLength; //max length of score text

        [SerializeField] GameObject menuPause;
        //[SerializeField] GameObject menuControls;

        [Header("Menu First Selected Options")]
        [SerializeField] GameObject mainMenuFirst;
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
            if(menuActive != null)
                menuActive.SetActive(false);
            menuActive = null;
        }

        public void OnPause()
        {
            if (currentMode == GameManager.gameMode.TITLE)
                return;
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

        public IEnumerator MainMenuShipLaunch()
        {
            // Calculate the target pos based on follow distance and the target's position
            Vector3 targetPosition = launchTarget.position + launchTarget.forward; //* -followDistance;

            // Apply smooth damp to the player's position
            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Calculate the new local position 
            Vector3 localPos = transform.InverseTransformPoint(smoothedPos);
            localPos.x += movementSpeed * Time.deltaTime; //* movementRange;
            localPos.y += movementSpeed * Time.deltaTime; //* movementRange;

            // Update player's position
            player.transform.position = transform.TransformPoint(localPos);
            yield return new WaitForSeconds(launchTime);
            //SceneManager.LoadScene("StarFern64");
            //StateUnpause();

        }
    }
}
