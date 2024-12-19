using DG.Tweening;
using KBCore.Refs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RailShooter
{
    public class PlayerController : ValidatedMonoBehaviour, IDamage
    {
        [SerializeField, Self] InputReader input;
        [SerializeField] AudioSource aud;

        [Header("----- Player UI -----")]
        [SerializeField] GameObject[] hpChunks;
        [SerializeField] GameObject[] shieldChunks;

        [Header("----- Player Stats -----")]
        [SerializeField] int HP;
        [SerializeField] int criticalHP;
        [SerializeField] float movementRange = 5f;
        [SerializeField] float movementSpeed = 10f;
        [SerializeField] float smoothTime = 0.2f;
        [SerializeField] float maxRoll = 15f;
        [SerializeField] float rollSpeed = 2f;
        [SerializeField] float rollDuration = 1f;
        [SerializeField] float rotationSpeed = 10f;

        [Header("----- Player Effects -----")]
        [SerializeField] GameObject model;
        public GameObject[] trailEffects;
        [SerializeField] GameObject[] damageTrails;
        [SerializeField] Renderer[] shipParts;
        [SerializeField] Material damageMaterial;
        [SerializeField] GameObject explosionPrefab;
        [SerializeField] GameObject shield;
        [SerializeField] float explosionDuration;

        [Header("----- Path and Aim Variables -----")]
        [SerializeField] Transform modelParent;
        [SerializeField] Transform playerModel;
        [SerializeField] Transform aimTarget;
        [SerializeField] Transform followTarget;
        [SerializeField] float followDistance;
        [SerializeField] Vector2 movementLimit = new Vector2(2f, 2f);

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip criticalAlarm;
        [Range(0, 1)][SerializeField] float criticalAlarmVol;
        [SerializeField] AudioClip explosion;
        [Range(0, 1)][SerializeField] float explosionVol;
        [SerializeField] AudioClip shieldDeactivateSFX;
        [Range(0, 1)][SerializeField] float shieldDeactivateVol;

        Vector3 velocity;
        float roll;
        bool isAlarmPlaying;
        bool isShielded;
        Material originalMaterial;
        int maxHP;
        int hpChunkIndex;
        int shieldChunkIndex;
        float alarmRate;
        Color shieldColorOG;
        public bool isBarrelRolling;

        void Awake()
        {
            input.LeftTap += OnLeftTap;
            input.RightTap += OnRightTap;
            input.Roll += OnRollTap;
            input.Pause += OnPauseTap;
        }

        void OnDestroy()
        {
            input.LeftTap -= OnLeftTap;
            input.RightTap -= OnRightTap;
            input.Roll -= OnRollTap;
            input.Pause -= OnPauseTap;
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (var trail in trailEffects)
            {
                trail.GetComponent<TrailRenderer>().enabled = false;
            }
            foreach (var chunk in shieldChunks)
            {
                chunk.SetActive(false);
            }
            originalMaterial = shipParts[0].material;
            maxHP = hpChunks.Length;
            hpChunkIndex = maxHP - 1;
            shieldChunkIndex = 0;
            shieldColorOG = shield.GetComponent<Renderer>().material.color;
            isBarrelRolling = false;
            aud = this.GetComponent<AudioSource>();

            PlatformController.singleton.Init("COM9", 115200);
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.isPaused)
            {
                HandlePosition();
                HandleRoll();
                HandleRotation();


                PlatformController.singleton.Pitch = -Mathf.DeltaAngle(model.transform.localEulerAngles.x, 0) * 0.5f;
            }

        }

        void OnLeftTap() => BarrelRoll();
        void OnRightTap() => BarrelRoll(1);
        void OnRollTap()
        {
            int random = UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
            BarrelRoll(random);
        }
        public void OnPauseTap()
        {
            GameManager.instance.OnPause();
        }

        private void HandlePosition()
        {
            // Calculate the target pos based on follow distance and the target's position
            Vector3 targetPosition = followTarget.position + followTarget.forward * -followDistance;

            // Apply smooth damp to the player's position
            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Calculate the new local position 
            Vector3 localPos = transform.InverseTransformPoint(smoothedPos);
            localPos.x += input.Move.x * movementSpeed * Time.deltaTime * movementRange;
            localPos.y += input.Move.y * movementSpeed * Time.deltaTime * movementRange;


            // Clamp the local position
            localPos.x = Mathf.Clamp(localPos.x, -movementLimit.x, movementLimit.x);
            localPos.y = Mathf.Clamp(localPos.y, -movementLimit.y, movementLimit.y);



            // Update player's position
            transform.position = transform.TransformPoint(localPos);

            Vector3 v = Camera.main.WorldToScreenPoint(transform.position);
            v.x = (v.x - Screen.width / 2) / (Screen.width / 2);
            v.y = (v.y - Screen.height / 2) / (Screen.height / 2);

            //PlatformController.singleton.Heave = -dif.y;
            //PlatformController.singleton.Sway = dif.x;
            PlatformController.singleton.Heave = v.y * 5;
            PlatformController.singleton.Sway = v.x * 10;
        }

        private void HandleRoll()
        {
            // Match the player's rptation to the follow target's rotation 
            transform.rotation = followTarget.rotation;

            // Match the roll based on player input
            //roll = Mathf.Lerp(rollSpeed, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            roll = Mathf.Lerp(roll, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, roll);
            PlatformController.singleton.Roll = Mathf.DeltaAngle(model.transform.localEulerAngles.z, 0) * 0.5f;
        }
        private void HandleRotation()
        {
            //Determine direction to the target
            Vector3 direction = aimTarget.position - transform.position;

            //Calculate the rotation required to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            modelParent.rotation = Quaternion.Lerp(modelParent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void BarrelRoll(int direction = -1)
        {
            if (!DOTween.IsTweening(playerModel))
            {
                isBarrelRolling = true;

                StartCoroutine(ActivateTrails());

                playerModel.DOLocalRotate(
                    new Vector3(playerModel.localEulerAngles.x, playerModel.localEulerAngles.y, 360 * direction),
                    rollDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);

            }

        }

        private IEnumerator ActivateTrails()
        {
            foreach (var trail in trailEffects)
            {
                trail.GetComponent<TrailRenderer>().enabled = true;
            }
            yield return new WaitForSeconds(rollDuration);
            foreach (var trail in trailEffects)
            {
                trail.GetComponent<TrailRenderer>().enabled = false;
            }
            isBarrelRolling = false;
        }

        IEnumerator flashDamage()
        {
            if (!isShielded)
            {
                foreach (var part in shipParts)
                {
                    part.material = damageMaterial;
                }

                yield return new WaitForSeconds(0.1f);

                foreach (var part in shipParts)
                {
                    part.material = originalMaterial;
                }
            }
            else
            {
                shield.GetComponent<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                shield.GetComponent<Renderer>().material.color = shieldColorOG;
            }


        }

        public void PlayCriticalAlarm() // Not an IEnumerator because the clip is already a looping clip
        {
            aud.clip = criticalAlarm;
            aud.loop = true;
            aud.volume = criticalAlarmVol;
            aud.Play();
            isAlarmPlaying = true;
        }

        public void StopCriticalAlarm()
        {
            foreach (var trail in damageTrails)
            {
                trail.SetActive(false);
            }
            aud.loop = false;
            aud.Stop();
            isAlarmPlaying = false;
        }

        public void takeDamage(int amount)
        {
            if (!isBarrelRolling)
            {
                if (!isShielded)
                {
                    HP -= amount;
                    //UpdatePlayerHealth(-amount);

                    if (HP <= 0)
                    {
                        GameObject explosionVFX = Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
                        aud.PlayOneShot(explosion, explosionVol);
                        model.SetActive(false);
                        this.GetComponent<BoxCollider>().enabled = false;
                        StopCriticalAlarm();
                        Destroy(explosionVFX, explosionDuration);
                        GameManager.instance.ResultsScreen();

                    }
                    else if (HP <= criticalHP)
                    {
                        foreach (var trail in damageTrails)
                        {
                            trail.SetActive(true);
                        }

                        if (!isAlarmPlaying)
                            PlayCriticalAlarm();
                    }
                }
                StartCoroutine(flashDamage());
                UpdatePlayerHealth(-amount);
            }

        }

        public void ActivateShield()
        {
            foreach (var chunk in shieldChunks)
            {
                chunk.SetActive(true);
            }
            shieldChunkIndex = shieldChunks.Length - 1;
            shield.SetActive(true);
            isShielded = true;
        }

        public void HealthPickup(int amount)
        {
            HP += amount;
            if (HP > maxHP)
                HP = maxHP;
            UpdatePlayerHealth(amount);
        }

        public void UpdatePlayerHealth(int amount)
        {

            if (amount < 0) //Reduce health
            {
                for (int i = 0; i < Math.Abs(amount); i++) //Handles HP Bar UI
                {
                    if (isShielded && shieldChunkIndex >= 0)
                    {
                        shieldChunks[shieldChunkIndex].SetActive(false);
                        shieldChunkIndex--;
                        if (shieldChunkIndex < 0)
                        {
                            isShielded = false;
                            shield.SetActive(false);
                            if (shieldDeactivateSFX != null)
                                aud.PlayOneShot(shieldDeactivateSFX, shieldDeactivateVol);
                        }
                    }
                    else if (hpChunkIndex >= 0)
                    {
                        hpChunks[hpChunkIndex].SetActive(false);
                        hpChunkIndex--;
                    }
                }
            }
            else //Health Restore
            {
                for (int i = 0; i < Math.Abs(amount); i++) //Handles HP Bar UI
                {
                    if (hpChunkIndex < hpChunks.Length - 1)
                    {
                        hpChunkIndex++;
                        hpChunks[hpChunkIndex].SetActive(true);
                    }
                }
                if (HP > criticalHP)
                {
                    StopCriticalAlarm();
                }
            }

        }


    }
}
