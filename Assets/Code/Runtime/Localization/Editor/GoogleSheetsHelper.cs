using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

using UnityEditor;

public static class GoogleSheetsHelper {

  static readonly string JsonLocation = "Assets/Code/Runtime/Localization/Editor/";
  static readonly string JsonFile = "clients_secrets.json"; // No use in actual build. This shit has owner permissions.

  static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
  static readonly string ApplicationName = "Localization";
  static readonly string SpreadsheetId = "1EOP2RpRtULjJyN6jHPu6VACiPVxQHPxpxkki1W4IMI0";
  static readonly string cutscenesheet = "mainsheet";
  static SheetsService service;

  static char LanguageRange = (char)('A' + Localization.LanguageCount);

  static GoogleSheetsHelper() {
    GoogleCredential credential;
    using (var stream = new FileStream(System.IO.Path.Combine(JsonLocation, JsonFile), FileMode.Open, FileAccess.Read)) {
      credential = GoogleCredential.FromStream(stream)
        .CreateScoped(Scopes);
    }

    service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer() {
      HttpClientInitializer = credential,
      ApplicationName = ApplicationName
    }

    );
  }

  #region Push

  /// <summary>
  /// Pushes local localization to the google sheets.
  /// </summary>
  public static void Push() {
    var locals = ReadLocalizaiton();
    var sheets = ReadGoogleSheets();

    // If key is new, add to sheets
    // If key isn't new, add new english value to sheets.
    foreach(var item in locals){
      string[] list;
      if (!sheets.TryGetValue(item.Key, out list)){
        list = new string[Localization.LanguageCount];
        sheets.Add(item.Key, list);
      }
      list[0] = item.Value;
    }

    string message = string.Format("Sending {0} keys.", sheets.Keys.Count);

    if (EditorUtility.DisplayDialog("Confirm Push?", message, "Push", "Do Not Push")) {
      // Sort based on key name
      var parse = sheets.ToList().Select(s => (s.Key, s.Value)).ToList();
      parse.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

      // Create groups of keys based on the first word before a _
      // CHARACTERSELECT_...
      // CHARACTERSELECT_..
      //
      // MAINMENU_..
      // like above
      // Makes it cleaner is all
      string prev = string.Empty;
      for(var i = 0; i < parse.Count; i++){
        var n = parse[i].Key;

        if (string.IsNullOrWhiteSpace(n)) continue;

        var t = n.Substring(0, n.IndexOf('_'));

        if (!string.IsNullOrWhiteSpace(prev) && t != prev){
          parse.Insert(i, (" ", new string[Localization.LanguageCount]));
          i++;
        }
        prev = t; 
      }

      SendToGoogleSheets(parse);
    }
  }

  /// <summary>
  /// Pulls google sheets to local localization. Overwrites all local.
  /// </summary>
  public static void Pull(){
    var sheets = ReadGoogleSheets();

    if (EditorUtility.DisplayDialog("Confirm Pull?", "", "Pull", "Do not Pull")){
      // Write with serialized objects :D
      var serializedObject = new SerializedObject(Localization.Instance);
      serializedObject.Update();

      var langauges = Localization.Langauges;
      for (var i = 0; i < langauges.Length; i++){
        var langauge = (LocalizationLanguage)langauges.GetValue(i);
        var field = string.Format("{0}Locals", langauge.ToString().ToLowerInvariant());
        var prop = serializedObject.FindProperty(field);

        var s = 0;
        foreach(var set in sheets){
          if (s >= prop.arraySize){
            prop.InsertArrayElementAtIndex(s);
          }
          SerializedProperty element = prop.GetArrayElementAtIndex(s);
          SerializedProperty keyProp = element.FindPropertyRelative("key");
          SerializedProperty textProp = element.FindPropertyRelative("text");
          keyProp.stringValue = set.Key;
          textProp.stringValue = set.Value[i];
          s++;
        }
      }

      serializedObject.ApplyModifiedProperties();
    }
  }

  #endregion

  #region Assets

  /// <summary>
  /// Reads and returns the english (key, value) from <see cref="Localization.Instance"/>.
  /// </summary>
  /// <returns></returns>
  public static Dictionary<string, string> ReadLocalizaiton(){
    var dict = new Dictionary<string, string>();

    var l = Localization.Instance;
    var locals = l.englishLocals;

    for(var j = 0; j < locals.Length; j++){
      var item = locals[j];
      dict.Add(item.key, item.text);
    }

    return dict;
  }

  #endregion

  #region Google Sheets Read

  /// <summary>
  /// Reads and returns all (key, value) from the google sheets.
  /// </summary>
  /// <returns></returns>
  public static Dictionary<string, string[]> ReadGoogleSheets() {
    var dict = new Dictionary<string, string[]>();

    // Ask google sheets
    var range = string.Format("{0}!A2:{1}", cutscenesheet, LanguageRange);
    var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

    var response = request.Execute();
    var values = response.Values;

    // If received valid response
    if (values != null && values.Count > 0) {
      foreach (var row in values) {
        if (row.Count > 0) {
          // get key
          var key = row[0] as string;
          if (string.IsNullOrWhiteSpace(key)) continue;

          // get all language values
          var list = new string[Localization.LanguageCount];
          for (var i = 0; i < list.Length; i++){
            string text;
            if (i + 1 < row.Count) text = row[i + 1] as string;
            else text = string.Empty;

            list[i] = text;
          }

          dict.Add(key, list);
        }
      }
    }

    return dict;
  }

  #endregion

  #region Google Sheets Write

  /// <summary>
  /// Writes <paramref name="googleLocal"/> to google sheets.
  /// </summary>
  /// <param name="googleLocal"></param>
  public static void SendToGoogleSheets(List<(string key, string[] texts)> locals) {
    var columns = CreateGoogleSheetsColumns(locals);
    ClearGoogleSheets();
    WriteToGoogleSheets(columns);
  }

  /// <summary>
  /// Creates a tabled list to be send with <see cref="WriteToGoogleSheets(List{IList{object}})"/>.
  /// </summary>
  /// <param name="googleLocal"></param>
  /// <returns></returns>
  public static List<IList<object>> CreateGoogleSheetsColumns(List<(string key, string[] texts)> locals) {
    // Create list to send to google sheets
    var columns = new List<IList<object>>();

    // Top row that displays "Keys" and all languages
    var header = new List<object>();
    header.Add("Keys");
    var langauges = Localization.Langauges;
    foreach(LocalizationLanguage e in langauges) {
      header.Add(e.ToString());
    }
    columns.Add(header);

    // Add each row with their key and values
    foreach (var pair in locals) {
      var key = pair.key;
      var texts = pair.texts;

      var row = new List<object>();
      row.Add(key);
      foreach(var t in texts){
        if (t != null) row.Add(t);
      }

      columns.Add(row);
    }

    return columns;
  }

  /// <summary>
  /// Wipes it clean.
  /// </summary>
  public static void ClearGoogleSheets() {
    var clearRange = new ClearValuesRequest();
    var clearRequest = service.Spreadsheets.Values.Clear(clearRange, SpreadsheetId, cutscenesheet);
    var clearReponse = clearRequest.Execute();
    Debug.Log("Clearing google sheets");
  }

  /// <summary>
  /// Overwrites the sheet with <paramref name="columns"/>.
  /// </summary>
  /// <param name="columns"></param>
  public static void WriteToGoogleSheets(List<IList<object>> columns) {
    // Create range
    var range = string.Format("{0}!A1:{1}", cutscenesheet, LanguageRange);
    var valueRange = new ValueRange();
    valueRange.Values = columns;

    // Send to Google Sheets
    var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
    var updateReponse = updateRequest.Execute();
    Debug.Log("Finished syncing to google sheets");
  }

  #endregion
}
