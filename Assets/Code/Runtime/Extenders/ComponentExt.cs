using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentExt{

  /// <summary>
  /// Get all components in parent and chilren one deep. Includes itself.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="parent"></param>
  /// <returns></returns>
  public static List<T> GetComponentsInChild<T>(Transform parent) where T : Component{
    var list = new List<T>();

    foreach(var comp in parent.GetComponents<T>()){
      list.Add(comp);
    }

    foreach(Transform t in parent){
      foreach(var comp in t.GetComponents<T>()){
        list.Add(comp);
      }
    }

    return list;
  }

  /// <summary>
  /// Gets all children of parent. Includes self.
  /// </summary>
  /// <param name="parent"></param>
  /// <returns></returns>
  public static List<Transform> GetAllChildren(Transform parent){
    var list = new List<Transform>();
    GetAllChildren(parent, list);
    return list;
  }

  private static void GetAllChildren(Transform parent, List<Transform> list){
    list.Add(parent);
    foreach(Transform child in parent){
      GetAllChildren(child, list);
    }
  }

}
