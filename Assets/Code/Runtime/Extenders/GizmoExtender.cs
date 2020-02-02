using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GizmoExtender {

  public static void DrawWireCircle(Vector3 center, float radius){
    DrawWireCircle(center, radius, Vector3.up);
  }

  public static void DrawWireCircle(Vector3 center, float radius, Vector3 upVector) {
    Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one - upVector);
    Gizmos.DrawWireSphere(Vector3.zero, radius);
    Gizmos.matrix = Matrix4x4.identity;
  }

}
