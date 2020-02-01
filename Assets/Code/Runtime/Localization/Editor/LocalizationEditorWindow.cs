using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LocalizationEditorWindow : EditorWindow {

  [MenuItem("Window/Localization")]
  static void Init() {
    var window = EditorWindow.GetWindow<LocalizationEditorWindow>();
    window.Show();
  }

  const int heightLimit = 15;

  Localization item;
  SerializedObject serializedObject;
  int page = 0;

  private void OnGUI() {
    GetLocalization();

    var halfwidth = position.width / 2f - 6f;

    serializedObject.Update();

    var englishProp = serializedObject.FindProperty("englishLocals");

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField("Keys", GUILayout.Width(halfwidth));
    EditorGUILayout.LabelField("Text", GUILayout.Width(halfwidth));
    EditorGUILayout.EndHorizontal();

    page = Mathf.Clamp(page, 0, englishProp.arraySize / 15);

    var h = 0;
    var i = page * heightLimit;
    while(i < englishProp.arraySize && h < heightLimit){
      var element = englishProp.GetArrayElementAtIndex(i);
      var key = element.FindPropertyRelative("key");
      var text = element.FindPropertyRelative("text");

      EditorGUILayout.BeginHorizontal();
      key.stringValue = EditorGUILayout.TextField(key.stringValue, GUILayout.Width(halfwidth));
      text.stringValue = EditorGUILayout.TextField(text.stringValue, GUILayout.Width(halfwidth));
      EditorGUILayout.EndHorizontal();

      i++;
      h++;
    }

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if (GUILayout.Button("Prev Page")){
      page--;
      GUIUtility.keyboardControl = 0;
    }
    GUILayout.Label(string.Format("{0}-{1}", page * heightLimit, Mathf.Min((page + 1) * heightLimit, englishProp.arraySize)));
    if (GUILayout.Button("Next Page")) {
      page++;
      GUIUtility.keyboardControl = 0;
    }
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if (GUILayout.Button("Add key")){
      englishProp.InsertArrayElementAtIndex(englishProp.arraySize);
    }
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(20f);
    if (GUILayout.Button("Push")){
      GoogleSheetsHelper.Push();
    }
    if (GUILayout.Button("Pull")){
      GoogleSheetsHelper.Pull();
    }

    serializedObject.ApplyModifiedProperties();
  }

  void GetLocalization(){
    if (item == null) {
      item = Localization.Instance;
    }

    if (serializedObject == null){
      serializedObject = new SerializedObject(item);
    }
  }
}
