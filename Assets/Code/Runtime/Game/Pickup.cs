using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Pickup : MonoBehaviour
    {
        public bool fixerCanPickUp;
        public bool monsterCanPickup;
        public string ItemName;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Pickup Entered");
            var player = other.gameObject.transform?.parent.GetComponent<BuilderDriver>()?.player;
            if (player != null)
            {
                Debug.Log(player);
                if (monsterCanPickup && player is Monster || fixerCanPickUp && player is Fixer)
                {
                    player.Items.Add(ItemFactory.SelectByName(other.gameObject, ItemName));
                    Destroy(gameObject);
                }
            }
        }
    }
}