using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute {
  public string fieldName;

  public ArrayElementTitleAttribute() {
    this.fieldName = "name";
  }

  public ArrayElementTitleAttribute(string fieldName) {
    this.fieldName = fieldName;
  }
}
