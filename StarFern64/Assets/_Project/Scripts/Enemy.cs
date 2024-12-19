using DG.Tweening.Core.Easing;
using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace RailShooter
{
    public partial class Enemy : ValidatedMonoBehaviour, IDamage
    {
        [SerializeField, Self] SplineAnimate splineAnimate;
        [SerializeField, Self] AudioSource aud;

        [SerializeField] GameObject explosionPrefab;
        [SerializeField] float explosionDuration;

        [SerializeField] int HP;
        [SerializeField] int criticalHP;
        [SerializeField] int damageValue;
        [SerializeField] int eliminationValue;
        [SerializeField] GameObject[] damageTrails;
        [SerializeField] Renderer[] models;
        [SerializeField] Material damageMaterial;
        [SerializeField] Transform player;
        [SerializeField] Transform enemyModel;
        [SerializeField] float rotationSpeed = 10f;

        [Header("----- Enemy Weapon System -----")]
        [SerializeField] int faceTargetSpeed;
        [SerializeField] float fireRate = 0.25f;
        //[SerializeField] float smoothTime = 0.2f;
        [SerializeField] Vector2 aimLimit = new Vector2(50f, 20f); //How far across and up and down enemy is able to aim
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float projectileDuration = 5f;
        [SerializeField] Transform firePoint;
        [SerializeField] int maxTargetDist;

        [Header("----- Item Drops -----")]
        [SerializeField] GameObject healthPackPrefab;
        [SerializeField] GameObject shieldItemPrefab;

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip blasterFire;
        [Range(0, 1)][SerializeField] float blasterFireVol;
        [SerializeField] AudioClip explosion;
        [Range(0, 1)][SerializeField] float explosionVol;

        Material originalMaterial;

        SplineContainer flightPath;


        bool isShooting;
        bool isDead;
        float rotX;
        float rotY;
        Vector3 directionToPlayer;

        public SplineContainer FlightPath
        {
            get => flightPath;
            set => flightPath = value;
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            originalMaterial = models[0].material;
            splineAnimate.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (splineAnimate != null && splineAnimate.ElapsedTime >= splineAnimate.Duration)
            {
                Destroy(gameObject);
            }
            HandleRotation();
        }

        private void HandleRotation()
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < maxTargetDist) //Player in range
            {
                //Debug.DrawLine(transform.position, player.position, Color.cyan);
                Vector3 dir = player.position - transform.position;
                directionToPlayer = dir;
                float dot = Vector3.Dot(player.forward, dir.normalized);
                print(dot);
                if (dot <= 0.1)
                {
                    //Calculate the rotation required to look at the target
                    Quaternion targetRotation = Quaternion.LookRotation(dir);
                    rotX = targetRotation.eulerAngles.x;
                    rotY = targetRotation.eulerAngles.y;
                    targetRotation.eulerAngles.Set(rotX, rotY, targetRotation.eulerAngles.z);
                    enemyModel.rotation = Quaternion.Lerp(enemyModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    if (!isShooting && !isDead) // TODO: enemy rotation check needs to be fixed
                        StartCoroutine(shoot());
                }
            }
            //else
            //{
            //    enemyModel.rotation = Quaternion.Lerp(enemyModel.rotation, Quaternion.identity, rotationSpeed);
            //}
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

        IEnumerator shoot()
        {
            isShooting = true;
            Vector3 direction = player.position - firePoint.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
            aud.PlayOneShot(blasterFire, blasterFireVol);

            if (projectile != null)
                Destroy(projectile, projectileDuration);

            yield return new WaitForSeconds(fireRate);
            isShooting = false;
        }

        public void takeDamage(int amount)
        {
            HP -= amount;

            StartCoroutine(flashDamage());
            if (HP > 0)
            {
                GameManager.instance.UpdateScore(damageValue * amount);
                if (HP <= criticalHP)
                {
                    foreach (var trail in damageTrails)
                    {
                        trail.SetActive(true);
                    }
                }
            }
            else // HP <= 0
            {
                isDead = true;
                GameObject explosionVFX = Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
                aud.PlayOneShot(explosion, explosionVol);
                int random = UnityEngine.Random.Range(-1, 100);
                if (random < 40)
                    Instantiate(healthPackPrefab, this.transform.position, healthPackPrefab.transform.rotation);
                else if (random < 71)
                    Instantiate(shieldItemPrefab, this.transform.position, shieldItemPrefab.transform.rotation);
                foreach (var model in models)
                {
                    model.enabled = false;
                }
                gameObject.GetComponent<BoxCollider>().enabled = false;
                Destroy(gameObject, 2);
                Destroy(explosionVFX, explosionDuration);
                GameManager.instance.UpdateScore(eliminationValue, 1);

                if (GameManager.instance.eliminations >= 15)
                {
                    fireRate = 0.1f;
                }
            }

        }

        private void OnDestroy()
        {
            if (flightPath != null)
            {
                Destroy(flightPath.gameObject);
            }

        }
    }
}
