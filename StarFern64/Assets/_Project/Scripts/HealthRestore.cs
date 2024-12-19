using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    [CreateAssetMenu(menuName = "PowerUps/HealthRestore")]
    public class HealthRestore : PowerUpEffect
    {
        public int HpRestoreAmount;
        public AudioClip pickupSFX;
        [Range(0, 1)][SerializeField] float audPickupVol;

        public override void ApplyBuff(GameObject player)
        {
            if (pickupSFX != null)
                GameManager.instance.player.GetComponent<AudioSource>().PlayOneShot(pickupSFX, audPickupVol);
            GameManager.instance.player.GetComponent<PlayerController>().HealthPickup(HpRestoreAmount);
        }
    }
}
