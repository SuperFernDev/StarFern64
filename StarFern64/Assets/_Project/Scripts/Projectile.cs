using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 50f;
        [SerializeField] int damageAmount; 
        [SerializeField] float destroyTime;


        // Start is called before the first frame update
        void Start()
        {
            Destroy(gameObject, destroyTime);
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            IDamage dmg = other.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);
                Destroy(gameObject);
            }

        }
    }
}
