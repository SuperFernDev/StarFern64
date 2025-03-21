using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RailShooter
{
    public class WeaponSystem : ValidatedMonoBehaviour
    {
        [SerializeField, Self] InputReader input;
        [SerializeField, Self] AudioSource aud;

        [SerializeField] Transform targetPoint;
        [SerializeField] float targetDistance = 50f;
        [SerializeField] float smoothTime = 0.2f;
        [SerializeField] Vector2 aimLimit = new Vector2(50f, 20f); //How far across and up and down player is able to aim
        [SerializeField] float mouseSensitivity = 10f;
        [SerializeField] float controllerSensitivity = 70f;
        //[SerializeField] float aimSpeed = 10f; //How fast the reticle will move
        [SerializeField] float aimReturnSpeed = 5f; //How fast reticle moves back to neutral position

        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float projectileDuration = 5f;
        [SerializeField] Transform firePoint;

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip blasterFire;
        [Range(0, 1)][SerializeField] float blasterFireVol;

        Vector3 velocity;
        Vector2 aimOffset; //How far from the center we are currently aiming

        private void Awake()
        {
            input.Fire += OnFire;
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.isPaused)
            {
                //Set the targetPosition ahead of the player's local position by the target distance
                Vector3 targetPosition = transform.position + transform.forward * targetDistance;
                Vector3 localPos = transform.InverseTransformPoint(targetPosition);

                float sensitivity = 0;
                if (gameObject.GetComponent<PlayerInput>().currentControlScheme == "Keyboard&Mouse")
                    sensitivity = mouseSensitivity;
                else
                    sensitivity = controllerSensitivity;


                //If there is an Aim Input
                if (input.Aim != Vector2.zero) //&& gameObject.GetComponent<PlayerInput>().currentControlScheme == "Keyboard&Mouse"
                {
                    aimOffset += input.Aim * sensitivity * Time.deltaTime;

                    //Clamp the aim offset
                    aimOffset.x = Mathf.Clamp(aimOffset.x, -aimLimit.x, aimLimit.x);
                    aimOffset.y = Mathf.Clamp(aimOffset.y, -aimLimit.y, aimLimit.y);
                }
                else if(gameObject.GetComponent<PlayerInput>().currentControlScheme != "Keyboard&Mouse" && input.Aim == Vector2.zero)
                {
                    //Otherwise return the aim offset to zero
                    aimOffset = Vector2.Lerp(aimOffset, Vector2.zero, aimReturnSpeed * Time.deltaTime);
                }

                //Apply the aim offset to the local position
                localPos.x += aimOffset.x;
                localPos.y += aimOffset.y;
                print(aimOffset);

                Vector3 desiredPosition = transform.TransformPoint(localPos);

                //Smooth damp to the desired position
                targetPoint.position = Vector3.SmoothDamp(targetPoint.position, desiredPosition, ref velocity, smoothTime);
            }
        }

        void OnFire()
        {
            if (!GameManager.instance.isPaused && !this.GetComponent<PlayerController>().isBarrelRolling)
            {
                Vector3 direction = targetPoint.position - firePoint.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
                aud.PlayOneShot(blasterFire, blasterFireVol);
                if (projectile != null)
                    Destroy(projectile, projectileDuration);
            }
            
        }

        private void OnDestroy()
        {
            input.Fire -= OnFire;
        }
    }
}
