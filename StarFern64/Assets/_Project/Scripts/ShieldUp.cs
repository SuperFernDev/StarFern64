using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    [CreateAssetMenu(menuName = "PowerUps/Shield")]
    public class ShieldUp : PowerUpEffect
    {        
        public AudioClip pickupSFX;
        [Range(0, 1)][SerializeField] float audPickupVol;

        public override void ApplyBuff(GameObject player)
        {
            if (pickupSFX != null)
                GameManager.instance.player.GetComponent<AudioSource>().PlayOneShot(pickupSFX, audPickupVol);
            GameManager.instance.player.GetComponent<PlayerController>().ActivateShield();
        }

    }
}
