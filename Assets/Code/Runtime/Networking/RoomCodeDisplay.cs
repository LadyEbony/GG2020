using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class RoomCodeDisplay : MonoBehaviour {

  public TextMeshProUGUI Text;

  void Start() {
    if (NetworkManager.inRoom){
      gameObject.SetActive(true);
      Text.text = NetworkManager.net.CurrentRoom.Name;
    } else {
      gameObject.SetActive(false);
    }
	}
}
