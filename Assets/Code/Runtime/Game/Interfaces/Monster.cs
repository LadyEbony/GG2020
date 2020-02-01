using System;

namespace Code.Runtime.Game.Interfaces
{
    public class Monster : Player
    {
        public void Start()
        {
            DamageItem attack = ItemFactory.Attack(gameObject);
            item = attack;
        }
    }
}