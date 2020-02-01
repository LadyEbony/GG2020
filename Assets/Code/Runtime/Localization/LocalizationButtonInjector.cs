using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalizationButtonInjector : MonoBehaviour {
  public string key;

  private void Awake() {
    GetComponent<TMP_Text>().text = Localization.GetString(key);
  }
}
