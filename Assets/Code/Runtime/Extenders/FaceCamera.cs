using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Texel

[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour {

  void LateUpdate() {
    var activeCam = Camera.main;

    if (activeCam) { // Ignore if the camera is null (No camera assigned)
      var camT = activeCam.transform;
      var camP = camT.position;
      var myT = transform; // Cache transform references as the getter is annoyingly slow
      var myPos = myT.position;

      var diff = myPos - camP;
      // Zero the Y, normalize
      diff.y = 0;
      diff = diff.normalized;

      myT.rotation = Quaternion.LookRotation(diff, Vector3.up);
    }
  }
}
