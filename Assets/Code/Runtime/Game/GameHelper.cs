using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameHelper  {

  public static Vector3 GetDirectionInput{
    get{
      // Get the yaw headings for our camera
      var ct = Camera.main.transform;

      // Get the forward and right for the camera, flatten them on the XZ plane, then renormalize
      var fwd = ct.forward;
      var right = ct.right;

      fwd.y = 0;
      right.y = 0;

      fwd = fwd.normalized;
      right = right.normalized;

      var hor = Input.GetAxis("Horizontal");
      var ver = Input.GetAxis("Vertical");

      var delta = hor * right;
      delta += ver * fwd;

      if (delta != Vector3.zero) delta = delta.normalized;
      return delta;
    }
    
  }
  

}
