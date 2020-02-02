using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Code.Runtime.Game.Interfaces;
using EntityNetwork; 

public class Structure : EntityBase, IAutoSerialize, IAutoDeserialize, IEarlyAutoRegister, IMasterOwnsUnclaimed, IRepairable, IDamageable, IShowHealth {
  
  [Header("Stuff")]
  public Transform shakeContainer;
  public Transform HealthBarOffset;
  public GameObject intactModel;
  public GameObject brokenModel;
  public float damageShakeAmount;   // Amount of translational shake applied
  public float damageShakeDuration; // Time of translational shake
  private float damageShakeTimeLeft;// Time before shaking stops
  public AudioSource repairAudio;
  public AudioSource damageAudio;
  public AudioSource deathAudio;

  [Header("Structure Stats")]
  public int startingHealth;
  public int maxHealth;

  [SerializeField]
  [NetVar('c', true, false, 100)]
  private int _currentHealth;
  public int currentHealth{
     get{
      return _currentHealth;
     } set{
      if (value >= maxHealth) {
        _currentHealth = maxHealth;
      } else if (value < 0) {
        _currentHealth = 0;
      } else {
        _currentHealth = value;
      }
     }
  }

  public GameObject GetTarget() => gameObject;

  private bool prevStructureState;
  [NetVar('s', false, false, 100)]
  public bool currStructureState = true;

  // Start is called before the first frame update
  void Start() {
    currentHealth = startingHealth;
  }

  void Update(){
    // Because this has to be networked, 
    // we gotta make it a big update
    UpdateVisualDamage();

    DamageShake();

    if (prevStructureState != currStructureState)
    {
      if(currStructureState == false)
      {
        Die();
      }
    }
    prevStructureState = currStructureState;
  }

  public void Repair(int repairAmount) {
    RaiseEvent('r', true, repairAmount); 
  }

  [NetEvent('r')]
  private void NetRepair(int repairAmount){
    repairAudio.Play();
    if (NetworkManager.isMaster)
    {
      currentHealth += repairAmount;
      if (currentHealth >= maxHealth)
        currStructureState = true;
    }
  }

  public void Damage(int damageAmount){
    RaiseEvent('d', true, damageAmount);    
  }

  [NetEvent('d')]
  private void NetDamage(int damageAmount){
    damageAudio.Play();
    ResetDamageShakeTimer();
    if (NetworkManager.isMaster)
    {
      currentHealth -= damageAmount;
      if (currentHealth <= 0)
        currStructureState = false;
    }
  }

  void Die(){
    deathAudio.Play();
    Debug.Log($"{name} broke.");
    GameObject brokenInstance = Instantiate(brokenModel, intactModel.transform.position, intactModel.transform.rotation);
    brokenInstance.SetActive(true);
    Destroy(brokenInstance, 5);
  }

  /// <summary>
  /// Reset damageShakeTimeLeft to damageShakeDuration
  /// </summary>
  void ResetDamageShakeTimer()
  {
    damageShakeTimeLeft = damageShakeDuration;
  }

  /// <summary>
  /// Shake the intact model if damageShakeTimeLeft > 0
  /// </summary>
  void DamageShake()
  {
    if (damageShakeTimeLeft > 0)
    {
      damageShakeTimeLeft -= Time.deltaTime;
      shakeContainer.localPosition = Random.insideUnitSphere * damageShakeAmount;
    }
    else
    {
      // If damage shake time has passed, stop shaking the intact model
      shakeContainer.localPosition = Vector3.zero;
    }
  }

  /// <summary>
  /// Update the material color of the intact model to correspond to its currentHealth
  /// </summary>
  void UpdateVisualDamage(){
    Material material = intactModel.GetComponent<Renderer>().material;
    Color color = Color.Lerp(Color.red, Color.white, (float)currentHealth / maxHealth);
    color.a = currStructureState ? 1 : 0.5f;
    material.SetColor("_Color", color);
  }

    public void ShowHealth(GameObject HealthBar, Camera camera)
    {
        Slider bar = HealthBar.GetComponent<Slider>();
        bar.minValue = 0;
        bar.maxValue = maxHealth;
        bar.value = currentHealth;
        HealthBar.GetComponent<RectTransform>().anchoredPosition = camera.WorldToScreenPoint(HealthBarOffset.position);
    }
}
