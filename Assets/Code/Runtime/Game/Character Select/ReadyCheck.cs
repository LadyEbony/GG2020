using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReadyCheck : MonoBehaviour {

  public static bool Stop;

  private Canvas canvas;
  private TextMeshProUGUI textMesh;

  public float elasped;
  public float duration = 3.0f;

  private bool readyDirty;
  private bool notReadyDirty;

  private void Start() {
    canvas = GetComponent<Canvas>();      
    textMesh = GetComponentInChildren<TextMeshProUGUI>();

    readyDirty = true;
    notReadyDirty = true;
  }

  void Update(){
    var ready = PlayerProperties.GetAllLobbyStatus();

    if (Stop){
      ready = false;
      Stop = false;
    }

    var lastText = textMesh.text;

    if (ready){
      canvas.enabled = true;

      elasped += Time.deltaTime;

      // Actually switch scenes 1 second after countdown
      // It's to check if no weird bad connections happen.
      if (elasped >= 4f) {
        CharacterSelect.Instance.LoadGame();
      } 
      // Disable controls. It's too late. (Except for weird connections happen)
      else if (elasped >= duration && readyDirty) {
        textMesh.text = "LETS GO!";

        if (NetworkManager.inRoom && NetworkManager.isMaster){
          NetworkManager.net.CurrentRoom.IsOpen = false;
        }
        readyDirty = false;
        notReadyDirty = true;
      } 
      // Display
      else if (elasped < duration){
        textMesh.text = Mathf.CeilToInt(duration - elasped).ToString();

      }

    } else {
      elasped = 0f;
      
      if (PlayerProperties.lobbyStatus.GetLocal()){
        canvas.enabled = true;
        textMesh.text = "Waiting";
      } else {
        canvas.enabled = false;
      }

      if (notReadyDirty) {
        if (NetworkManager.inRoom && NetworkManager.isMaster) {
          NetworkManager.net.CurrentRoom.IsOpen = true;
        }
        notReadyDirty = false;
        readyDirty = true;
      }

    }

  }
}
