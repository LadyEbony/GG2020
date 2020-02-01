using System;

namespace Code.Runtime.Game.Interfaces
{
    public class Monster : Player
    {
        public void Start()
        {
            DamageItem attack = gameObject.AddComponent<DamageItem>();
            attack.baseDamageStrength = 25;
            attack.useCooldown = TimeSpan.FromMilliseconds(1000);
            attack.range = 3;
            item = attack;
        }
    }
}