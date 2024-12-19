using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public abstract class PowerUpEffect : ScriptableObject
    {
        public abstract void ApplyBuff(GameObject player);
    }
}
