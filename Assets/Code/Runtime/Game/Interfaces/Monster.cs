using System;

namespace Code.Runtime.Game.Interfaces
{
    public class Monster : Player
    {
        public void Start()
        {
            DamageItem meleeAttack = ItemFactory.MeleeAttack(gameObject);
            Items.Add(meleeAttack);
            item = meleeAttack;
            currentRange = meleeAttack.range;
            DamageItem rangedAttack = ItemFactory.RangedAttack(gameObject);
            Items.Add(rangedAttack);
        }
    }
}