using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ArrayElementTitleAttribute))]
public class ArrayElementTitleDrawer : PropertyDrawer {

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    return EditorGUI.GetPropertyHeight(property, label, true);
  }

  protected virtual ArrayElementTitleAttribute titleAttribute => (ArrayElementTitleAttribute)attribute;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    string fullpath = property.propertyPath + "." + titleAttribute.fieldName;
    var prop = property.serializedObject.FindProperty(fullpath);
    string newlabel = GetTitle(prop);
    if (string.IsNullOrEmpty(newlabel))
      newlabel = label.text;

    EditorGUI.PropertyField(position, property, new GUIContent(newlabel, label.tooltip), true);
  }


  private string GetTitle(SerializedProperty prop) {
    switch (prop.propertyType) {
      case SerializedPropertyType.Generic:
        break;
      case SerializedPropertyType.Integer:
        return prop.intValue.ToString();
      case SerializedPropertyType.Boolean:
        return prop.boolValue.ToString();
      case SerializedPropertyType.Float:
        return prop.floatValue.ToString();
      case SerializedPropertyType.String:
        return prop.stringValue;
      case SerializedPropertyType.Color:
        return prop.colorValue.ToString();
      case SerializedPropertyType.ObjectReference:
        return prop.objectReferenceValue.ToString();
      case SerializedPropertyType.LayerMask:
        break;
      case SerializedPropertyType.Enum:
        return prop.enumNames[prop.enumValueIndex];
      case SerializedPropertyType.Vector2:
        return prop.vector2Value.ToString();
      case SerializedPropertyType.Vector3:
        return prop.vector3Value.ToString();
      case SerializedPropertyType.Vector4:
        return prop.vector4Value.ToString();
      case SerializedPropertyType.Rect:
        break;
      case SerializedPropertyType.ArraySize:
        break;
      case SerializedPropertyType.Character:
        break;
      case SerializedPropertyType.AnimationCurve:
        break;
      case SerializedPropertyType.Bounds:
        break;
      case SerializedPropertyType.Gradient:
        break;
      case SerializedPropertyType.Quaternion:
        break;
      default:
        break;
    }
    return "";
  }
}