using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public class ItemPickUp : MonoBehaviour
    {
        [SerializeField] enum ItemType { HEALTH, SHIELD }
        [SerializeField] ItemType type;
        [SerializeField] float rotationSpeed;
        [SerializeField] int bobSpeed;
        [SerializeField] float bobHeight;
        [SerializeField] float travelSpeed;
        [SerializeField] float stoppingDistance = 10;
        [SerializeField] PowerUpEffect powerupEffect;

        [Header("----- Sounds -----")]
        [SerializeField] AudioClip pickUpSound;
        [SerializeField] AudioSource audSource;
        [Range(0, 1)][SerializeField] float volume;

        Vector3 startPos;

        // Start is called before the first frame update
        void Start()
        {
            startPos = transform.position;
            audSource = GameManager.instance.GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            //Spin();
            //Bob();

            float dist2Player = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);
            if (dist2Player > stoppingDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, GameManager.instance.player.transform.position, travelSpeed * Time.deltaTime);
            }
            transform.LookAt(Camera.main.transform.position);
            if (type == ItemType.HEALTH)
                transform.Rotate(90, 0, 0);
        }

        void Spin()
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }

        void Bob()
        {
            transform.position = new Vector3(startPos.x, startPos.y + (bobHeight * Mathf.Sin(Time.time * bobSpeed)), startPos.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                switch (type)
                {
                    case ItemType.HEALTH:
                        powerupEffect.ApplyBuff(other.gameObject);
                        if (pickUpSound != null)
                            audSource.PlayOneShot(pickUpSound, volume);
                        Destroy(gameObject);
                        break;
                    case ItemType.SHIELD:
                        powerupEffect.ApplyBuff(other.gameObject);
                        if (pickUpSound != null)
                            audSource.PlayOneShot(pickUpSound, volume);
                        Destroy(gameObject);
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
