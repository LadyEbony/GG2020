using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LocalizationLanguage { English, Japanese, Spanish };

public class Localization : ScriptableObject {

  const string path = "Assets/Resources/Localization.asset";

  public static LocalizationLanguage SelectedLanguage = LocalizationLanguage.English;

  public static readonly int LanguageCount = System.Enum.GetNames(typeof(LocalizationLanguage)).Length;
  public static readonly System.Array Langauges = typeof(LocalizationLanguage).GetEnumValues();

  private static Localization _Instance;
  public static Localization Instance{
    get {

      if (_Instance == null) {
#if UNITY_EDITOR
        _Instance = AssetDatabase.LoadAssetAtPath<Localization>(path);

        if (_Instance == null) {
          _Instance = ScriptableObject.CreateInstance<Localization>();
          AssetDatabase.CreateAsset(_Instance, path);
        }
#else
      _Instance = Resources.Load<Localization>("Localization");
#endif
      }

      return _Instance;
    }
  }

  private static Dictionary<string, string> keyDictionary;

  /// <summary>
  /// Get localization string from <paramref name="key"/>.
  /// Returns E-R-R-O-R if the key doesn't exist.
  /// </summary>
  /// <param name="key"></param>
  /// <returns></returns>
  public static string GetString(string key){
    if (keyDictionary == null){
      keyDictionary = new Dictionary<string, string>();

      var locals = Instance.GetLocals();
      foreach (var l in locals){
        keyDictionary.Add(l.key, l.text);
      }
    }

    if (keyDictionary.ContainsKey(key)) return keyDictionary[key];
    else return "E-R-R-O-R";
  }

  [System.Serializable]
  public struct Local{
    public string key;
    public string text;
  }

  public Local[] englishLocals;
  public Local[] japaneseLocals;
  public Local[] spanishLocals;

  /// <summary>
  /// Gets localization of <paramref name="language"/>.
  /// </summary>
  /// <param name="language"></param>
  /// <returns></returns>
  public Local[] GetLocals(LocalizationLanguage language){
    switch (language) {
      case LocalizationLanguage.English:
        return englishLocals;
      case LocalizationLanguage.Japanese:
        return japaneseLocals;
      case LocalizationLanguage.Spanish:
        return spanishLocals;

    }
    return englishLocals;
  }

  /// <summary>
  /// Gets localization of <see cref="SelectedLanguage"/>.
  /// </summary>
  /// <returns></returns>
  public Local[] GetLocals() => GetLocals(SelectedLanguage);
}
