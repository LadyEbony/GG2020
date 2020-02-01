using UnityEngine;
using ExitGames.Client.Photon.LoadBalancing;

using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Extensions of Exitgames Hashtable to allow for concise conditional setting logic
/// </summary>
public static class HashtableExtension {
	/// <summary>
	/// Checks if the hashtable contains key, if so, it will update toSet. Struct version
	/// </summary>
	/// <param name="key">Key to check for</param>
	/// <param name="toSet">Reference to the variable to set</param>
	/// <typeparam name="T">Type to cast toSet to</typeparam>
	public static void SetOnKey<T>(this Hashtable h, object key, ref T toSet) where T : struct {
		if (h.ContainsKey(key))
			toSet = (T)h[key];
	}

	public static void AddOrSet<T>(this Hashtable h, object key, T val) where T : struct {
		if (h.ContainsKey(key)) {
			h[key] = val;
		} else {
			h.Add(key, val);
		}
	}

	/// <summary>
	/// Add a value to the hashtable if and only if it mismatches the previous provided
	/// Returns true if the replacement was made
	/// </summary>
	public static bool AddWithDirty<T>(this Hashtable h, char key, T tracked, ref T previous) {
		if (tracked.Equals(previous)) return false;
		
		h.Add (key,tracked);
		previous = tracked;
		return true;
	}

  /// <summary>
  /// Adds and updates the keys/value based on <paramref name="propertiesToSet"/>.
  /// Any other keys are uneffected.
  /// </summary>
  /// <param name="h"></param>
  /// <param name="propertiesToSet"></param>
  public static void SetHashtable(this Hashtable h, Hashtable propertiesToSet){
    var customProps = propertiesToSet.StripToStringKeys() as Hashtable;
    h.Merge(customProps);
    h.StripKeysWithNullValues();
  }
}
