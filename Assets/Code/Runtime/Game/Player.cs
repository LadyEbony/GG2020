using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Player : EntityBase, ITargetable, IDamageable, IShowHealth
    {
        
        public List<Heldable> Items = new List<Heldable>();

        public int heldItemIndex;

        public ITargetable Target;

        public Heldable item;

        [HideInInspector]
        public BuilderDriver driver;

        public float currentRange;

        public GameManager gameManager;

        public float respawnTime;

        private float elapsedDeathTime;
        
        public enum PlayerState
        {
            Waiting,
            Playing
        }

        private PlayerState currentState;
        
        [SerializeField]
        [EntityBase.NetVar('c', true, false, 100)]
        private int _currentHealth;
        public int currentHealth{
            get{
                return _currentHealth;
            } set{
                if (value >= maxHealth) {
                    _currentHealth = maxHealth;
                } else if (value < 0) {
                    _currentHealth = 0;
                    Die();
                } else {
                    _currentHealth = value;
                }
            }
        }
        public int maxHealth;

        public void Start()
        {
            currentRange = 0;
            currentState = PlayerState.Waiting;
            elapsedDeathTime = 0;
            gameManager = GameObject.FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.Log("Cannot find game manager");
            }
        }

        public void Update()
        {
            switch (currentState)
            {
                case PlayerState.Waiting:
                    elapsedDeathTime += Time.deltaTime;
                    if (elapsedDeathTime >= respawnTime)
                    {
                        Respawn();
                        
                    }
                    break;
                case PlayerState.Playing:
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Target != null)
                        {
                            if (item is ITargeting)
                            {
                                (item as ITargeting).UseOn(Target);
                            }
                            else
                            {
                                item?.Use();
                            }
                        }
                        else
                        {
                            item?.Use();
                        }
                    }

                    if (Input.mouseScrollDelta.y > 0)
                    {
                        if (Items.Any())
                        {
                            heldItemIndex = (heldItemIndex + 1) % Items.Count;
                            item = Items[heldItemIndex];
                            if(item is IRanged)
                            {
                                currentRange = (item as IRanged).GetRange();
                            }
                        }
                    }
                    else if (Input.mouseScrollDelta.y < 0)
                    {
                        if (Items.Any())
                        {
                            heldItemIndex = (heldItemIndex - 1);
                            if (heldItemIndex < 0)
                            {
                                heldItemIndex = Items.Count - 1;
                            }
                            item = Items[heldItemIndex];
                            if(item is IRanged)
                            {
                                currentRange = (item as IRanged).GetRange();
                            }
                        }
                    }
                    break;
            }
            // items
        
        }

        void Die()
        {
            
        }

        void Respawn()
        {
            var spawnPoint = gameManager.GetAvailableSpawnPoint(this);
            if (spawnPoint.HasValue)
            {
                gameObject.transform.position = spawnPoint.Value;
                currentState = PlayerState.Playing;
            }
            else
            {
                Debug.Log("No spawn points available");
            }
        }
        public GameObject GetTarget() => gameObject;
        public void Damage(int damageAmount)
        {    
            RaiseEvent('d', true, damageAmount);    
        }

        public void ShowHealth(GameObject HealthBar, Camera camera)
        {
            throw new NotImplementedException();
        }
    }
}