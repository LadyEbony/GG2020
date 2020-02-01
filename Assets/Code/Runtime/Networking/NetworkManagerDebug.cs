using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerDebug : MonoBehaviour {
  // Debug out the current state for visibility
  [Header("Current state")]
  public ExitGames.Client.Photon.LoadBalancing.ClientState currentState;

  private void Update() {
    if (NetworkManager.net != null) {
      currentState = NetworkManager.net.State;

      GetComponentInChildren<TMPro.TextMeshProUGUI>().text = currentState.ToString();
    }
  }
}
