using DG.Tweening;
using KBCore.Refs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RailShooter
{
    public class PlayerController : ValidatedMonoBehaviour, IDamage
    {
        [SerializeField, Self] InputReader input;
        

        [Header("----- Player UI -----")]
        [SerializeField] GameObject[] hpChunks;

        [Header("----- Player Stats -----")]
        [SerializeField] int HP = 10;
        [SerializeField] float movementRange = 5f;
        [SerializeField] float movementSpeed = 10f;
        [SerializeField] float smoothTime = 0.2f;
        [SerializeField] float maxRoll = 15f;
        [SerializeField] float rollSpeed = 2f;
        [SerializeField] float rollDuration = 1f;
        [SerializeField] float rotationSpeed = 10f;

        [SerializeField] GameObject[] damageTrails;
        [SerializeField] Renderer[] models;
        [SerializeField] Material damageMaterial;
        [SerializeField] GameObject explosionPrefab;
        [SerializeField] float explosionDuration;

        [SerializeField] Transform followTarget;
        [SerializeField] Transform aimTarget;

        [SerializeField] Transform playerModel;
        [SerializeField] float followDistance;
        [SerializeField] Vector2 movementLimit = new Vector2(2f, 2f);

        [SerializeField] Transform modelParent;

        [SerializeField] GameObject[] trailEffects;

        Vector3 velocity;
        [SerializeField] float roll;



        Material originalMaterial;
        int maxHP;
        int hpChunkIndex;


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
            originalMaterial = models[0].material;
            maxHP = HP;
            hpChunkIndex = hpChunks.Length - 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.isPaused)
            {
                HandlePosition();
                HandleRoll();
                HandleRotation();
            }
        }

        void OnLeftTap() => BarrelRoll();
        void OnRightTap() => BarrelRoll(1);
        void OnRollTap()
        {
            int random = UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
            BarrelRoll(random);
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
        }

        private void HandleRoll()
        {
            // Match the player's rptation to the follow target's rotation 
            transform.rotation = followTarget.rotation;

            // Match the roll based on player input
            //roll = Mathf.Lerp(rollSpeed, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            roll = Mathf.Lerp(roll, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, roll);
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
        }

        IEnumerator flashDamage()
        {
            foreach (var model in models)
            {
                model.material = damageMaterial;
            }

            yield return new WaitForSeconds(0.1f);

            foreach (var model in models)
            {
                model.material = originalMaterial;
            }

        }

        public void takeDamage(int amount)
        {
            HP -= amount;

            for (int i = 0; i < amount; i++) //Handles HP Bar UI
            {
                if (hpChunkIndex >= 0)
                {
                    hpChunks[hpChunkIndex].SetActive(false);
                    hpChunkIndex--;
                }
            }
            StartCoroutine(flashDamage());

            if (HP <= 0)
            {
                GameObject explosion = Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
                Destroy(gameObject);
                Destroy(explosion, explosionDuration);
            }
            else if (HP < maxHP / 2)
            {
                foreach (var trail in damageTrails)
                {
                    trail.SetActive(true);
                }
            }
        }

        public void OnPauseTap()
        {
            GameManager.instance.OnPause();
        }
    }
}
