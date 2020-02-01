using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection;
using System.Linq;

using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.Photon;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using PConst = PhotonConstants;


using Attribute = System.Attribute;
using Type = System.Type;
using Action = System.Action;

using EntityNetwork;

public abstract class EntityBase : MonoBehaviour {

	/// <summary>
	/// Entity ID for the entity, all entities require a unique ID.
	/// </summary>
	//[System.NonSerialized]
	public int EntityID;

	/// <summary>
	/// Player ID number who is considered the authority for the object.
	/// isMine / isRemote / isUnclaimed are determined by the authority holder.
	/// Defaults to -1 if unassigned.
	/// </summary>
	//[System.NonSerialized]
	public int authorityID = -1;

	#region Helpers
	/// <summary>
	/// A helper that determines if the object is owned locally.
	/// </summary>
	/// <seealso cref="isRemote"/>
	public bool isMine {
		get {
		  // Everything is ours when we're not connected
		  if (NetworkManager.net == null) return true;

      // If we're the master and have the appropriate interface, ingore the Authority ID and use the master status
      if (this is IMasterOwnsUnclaimed && isUnclaimed) {
        return NetworkManager.isMaster;
      }

		  return NetworkManager.localID == authorityID;
		}
	}

	/// <summary>
	/// A helper to determine if the object is remote.
	/// Returns false if we're disconnected
	/// </summary>
	/// <seealso cref="isMine"/>
	public bool isRemote {
		get {
			if (NetworkManager.net == null) return false;

      // Similar to isMine, ignore the master status if unclaimed
      if (this is IMasterOwnsUnclaimed && isUnclaimed) {
        return !NetworkManager.isMaster;
      }

			return NetworkManager.localID != authorityID;
		}
	}

	/// <summary>
	/// Helper to evaluate our authority ID being -1. It should be -1 if unclaimed.
	/// </summary>
	public bool isUnclaimed {
		get {
			return authorityID == -1;
		}
	}

	/// <summary>
	/// Query to see if we're registered. This is slightly expensive.
	/// </summary>
	/// <value><c>true</c> if is registered; otherwise, <c>false</c>.</value>
	public bool isRegistered {
		get {
			return EntityManager.Entity(authorityID) != null;
		}
	}

	public void AppendIDs(Hashtable h) {
		h.Add(PConst.eidChar, EntityID);
		h.Add(PConst.athChar, authorityID);
	}

	public void Register() {
		EntityManager.Register(this);
	}

	public void RaiseEvent(char c, bool includeLocal, params object[] parameters) {
		var h = new Hashtable();

		AppendIDs(h);

		h.Add(0, c);
		if (parameters != null)
			h.Add(1, parameters);

		NetworkManager.netMessage(PhotonConstants.EntityEventCode, h, true);

		if (includeLocal) {
			InternallyInvokeEvent(c, parameters);
		}
	}

  public static void RaiseStaticEvent<T>(char c, bool includeLocal, params object[] parameters) where T : EntityBase{
    var h = new Hashtable();

    // Given we have no instance ID's, we don't append IDs

    h.Add(0, c);

    if (parameters != null)
      h.Add(1, parameters);

    //var name = typeof(T).
    h.Add(2,IDfromType(typeof(T)));

    NetworkManager.netMessage(PhotonConstants.EntityEventCode, h, true);

    if (includeLocal)
      InternallyInvokeStatic(typeof(T),c, parameters);
  }

  static Dictionary<int,Type> EBTypeIDs;
  static Dictionary<Type,int> IDToEBs;
  static void buildTypeIDs(){ // Build a bidirectional lookup of all EntityBase's in the assembly and assign them unique ID's
    EBTypeIDs = new Dictionary<int,Type>();
    IDToEBs = new Dictionary<Type,int>();

    var ebType = typeof(EntityBase);
    var derivedTypes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t=>ebType.IsAssignableFrom(t));

    var sorted = derivedTypes.OrderBy(t => t.FullName);
    int newID = 0;
    foreach(var type in sorted) {
      EBTypeIDs.Add(newID, type);
      IDToEBs.Add(type, newID);
      ++newID;
    }

    if (Debug.isDebugBuild || Application.isEditor) {
      var debugString = new System.Text.StringBuilder();
      foreach(var pair in EBTypeIDs) {
        debugString.AppendFormat("{0} -> {1} \n", pair.Value, pair.Key);
      }

      Debug.Log(debugString.ToString());
    }
  }

  /// <summary>
  /// Get a unique ID for this objects class that dervives from EntityBase
  /// </summary>
  public int typeID {
    get {
      return IDfromType(this.GetType());
    }
  }

  public static int IDfromType(Type t) {
    if (IDToEBs != null)
      return IDToEBs[t];

    buildTypeIDs();

    return IDfromType(t);
  }
  public static Type TypeFromID(int id) {
    if (EBTypeIDs != null)
      return EBTypeIDs[id];
    
    buildTypeIDs();

    return TypeFromID(id); // Return the original request
  }
	#endregion

	#region Serializers

	/// <summary>
	/// Serialize all tokens with the given label into HashTable h
	/// Returns true if any contained token requires a reliable update
	/// If you use IAutoSerialize, this is only neccessary for manual tokens
	/// </summary>
	protected bool SerializeToken(Hashtable h, params char[] ca) {
		bool needsReliable = false;

		var tH = tokenHandler;
		foreach(char c in ca) {
			h.Add(c, tH.get(c, this));

			// If we're not already reliable, check if we need reliable
			if (!needsReliable)
				needsReliable = tH.alwaysReliable[c];
		}

		return needsReliable;
	}

	/// <summary>
	/// Internally used for building and dispatching entity updates, build a full serialization of auto tokens and ID's
	/// Due to inconsistent handling/calling contexts, ID's are added safely
	/// </summary>
	/// <returns>0 if nothing is sent, 1 if there is content to send, 2 if content should be sent reliably</returns>
	public int SerializeAuto(Hashtable h) {
		var tH = tokenHandler;

		var time = Time.realtimeSinceStartup;

		bool reliableFlag = false;
		bool isSending = false;

		foreach (var c in tH.autoTokens) {
			if (this[c] < time) {
				this[c] = time + tH.updateTimes[c];
				isSending = true;

				h.Add(c, tH.get(c, this));

				if (!reliableFlag)
					reliableFlag = tH.reliableTokens.Contains(c);
			}
		}

		if (isSending) {
			SerializeAlwaysTokensSafely(h);
			//toUpdate.AddRange(tH.alwaysSendTokens);
		}

		// If none of the tokens actually updated, return 0
		// Otherwise, return 1 for a normal update, 2 for a reliable update
		if (!isSending)
			return 0;
		
		h.AddOrSet(PConst.eidChar, EntityID);
		h.AddOrSet(PConst.athChar, authorityID);

		//SerializeToken(toUpdate.Distinct().ToArray());

		return reliableFlag ? 2 : 1;
	}

	/// <summary>
	/// Read specified values out of the hashtable
	/// In most cases, you'll want to use DeserializeFull instead
	/// </summary>
	protected void DeserializeToken(Hashtable h, params char[] ca) {
		var tH = tokenHandler;
		foreach(char c in ca) {
			object value;
			if (h.TryGetValue(c, out value))
				tH.set(c, this, value);
		}
	}

	/// <summary>
	/// Read all attributed tokens fields of the hashtable and update corresponding values.
	/// This will be called automatically if implementing IAutoDeserialize
	/// </summary>
	public void DeserializeFull(Hashtable h) {
		var tH = tokenHandler;
		foreach(char c in TokenList()) {
			object value;
			if (h.TryGetValue(c, out value))
				tH.set(c, this, value);
		}
	}

	/// <summary>
	/// Key function describing what to serialize. Be sure to call Base.Serialize(h)
	/// Helper SerializeToken will automatically write fields with matching tokens into the table
	/// </summary>
	public virtual void Serialize(Hashtable h) {
		AppendIDs(h);
	}

	/// <summary>
	/// Deserialize the entity out of the provided hashtable.
	/// Use helper function DeserializeToken automatically unpack any tokens
	/// </summary>
	public virtual void Deserialize(Hashtable h) {
		h.SetOnKey(PConst.eidChar, ref EntityID);
		h.SetOnKey(PConst.athChar, ref authorityID);
	}

	/// <summary>
	/// Check to see if the hashtable already contains each always send token, and if not, add it.
	/// </summary>
	private bool SerializeAlwaysTokensSafely(Hashtable h) {
		var tH = tokenHandler;
		foreach(var c in tH.alwaysSendTokens) {
			// If the hashtable doesn't contain our token, add it in
			if (!h.ContainsKey(c)) {
				h.Add(c, tH.get(c,this));
			}
		}
		return tH.alwaysIsRelaible;
	}

	#endregion

	/// <summary>
	/// Send a reliable update with <b>only</b> the provided tokens, immediately.
	/// This does not send the alwaysSend autotokens, and exists solely so that you can have a field update as soon as possible, 
	/// such as menu or input events
	/// </summary>
	public void UpdateExclusively(params char[] ca) {
		var h = new Hashtable();

		AppendIDs(h);

		SerializeToken(h, ca);
		NetworkManager.netMessage(PConst.EntityUpdateCode, h, true);
	}

	/// <summary>
	/// Immediately sent a network update with our current state. This includes auto tokens if IAutoSerialize is implemented.
	/// Reliable flag, though it defaults to false, may be forced true when sending always or reliable tokens.
	/// </summary>
	public void UpdateNow(bool reliable = false) {
		var h = new Hashtable();

		Serialize(h);

		if (this is IAutoSerialize) {
			int autoCode = SerializeAuto(h);
			if (autoCode == 2) reliable = true;
		} else {
			if (SerializeAlwaysTokensSafely(h))
				reliable = true;
		}
			

		NetworkManager.netMessage(PConst.EntityUpdateCode, h, reliable);
	}

	#region timing
	// updateTimers coordinates when each value's server representation 'expires' and should be resent
	// For values which didn't specify an update time, this value is set to +inf, so that it will always be greater than the current time

	private Dictionary<char,float> updateTimers = new Dictionary<char,float>();


	float this[char c] {
		get {
			float t;
			if(!updateTimers.TryGetValue(c, out t)) {
				var updateTime = tokenHandler.updateTimes[c];
				updateTimers.Add(c, updateTime >= 0 ? 0 : Mathf.Infinity);
			}
			return updateTimers[c];
		}
		set {
			updateTimers[c] = value;
		}
	}

	#endregion
	// Token management is a system that assigns a character token to each field for serialization, via attributes
	// This is used to automatically pull get/set for variables to assist in auto serializing as much as possible and reducing the amount of manual network messaging

	[ContextMenu("Claim as mine")]
	public bool ClaimAsMine() {
		if (!NetworkManager.inRoom && NetworkManager.isReady) return false;

		authorityID = NetworkManager.localID;

		UpdateNow(true);
		return true;
	}

	#region TokenManagement

	/// TODO: Modifying a token at this level needs to clone the TokenHandler specially for this EntityBase object so changes don't propegate to other entities

	/// <summary>
	/// Runtime modify the parameters of a token. Modifying the reliability of a token is slightly intensive.
	/// </summary>
	/// <param name="token">The token to be modified</param>
	/// <param name="updateMs">Milliseconds between updates. 0 is every frame, use cautiously. Set negative to unsubcribe automaic updates.</param>
	/// <param name="alwaysSend">If the token should always be sent with other tokens</param>
	/// <param name="isReliable">If the token needs to be sent reliably.</param>
	public void ModifyToken(char token, int? updateMs = null, bool? alwaysSend = null, bool? isReliable = null) {
		
		var tH = tokenHandler;
		if (tH.shared) {
			_tokenHandler = tH.DeepClone();
			tH = _tokenHandler;
		}


		// If we have a value for reliability
		if (isReliable.HasValue) {
			if (tH.reliableTokens.Contains(token)){
				if (!isReliable.Value)
					tH.reliableTokens.Remove(token);
			} else {
				if (isReliable.Value)
					tH.reliableTokens.Add(token);
			}
		}

		// If we have a value for always sending
		if (alwaysSend.HasValue) {
			if (tH.alwaysSend.ContainsKey(token)){
				if (!alwaysSend.Value)
					tH.alwaysSend.Remove(token);
			} else {
				if (alwaysSend.Value)
					tH.alwaysSend.Add(token,alwaysSend.Value);
			}
		}

		if (alwaysSend.HasValue || isReliable.HasValue) 
			tH.ReEvalAlwaysIsReliable();

		if (updateMs.HasValue) {
			float fUpdateTime = updateMs.Value / 1000f;
			tH.updateTimes[token] = fUpdateTime;

			// Unsubscribing
			if (fUpdateTime < 0) {
				if (tH.autoTokens.Contains(token))
					tH.autoTokens.Remove(token);
				if (!tH.manualTokens.Contains(token))
					tH.manualTokens.Add(token);

				this[token] = Mathf.Infinity; // Never auto-update
			} else {
				if (!tH.autoTokens.Contains(token))
					tH.autoTokens.Add(token);
				if (tH.manualTokens.Contains(token))
					tH.manualTokens.Remove(token);

				this[token] = 0; // Auto update next check
			}
		}
	}

	/// <summary>
	/// Invoke a labeled character event. You should never need to use this method manually.
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void InternallyInvokeEvent(char c, params object[] parameters) {
		tokenHandler.NetEvents[c].Invoke(this, parameters);
	}

  /// <summary>
  /// Invoke a labeled character event when the event is static. You should hopefully never need to use this method manually.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
  public static void InternallyInvokeStatic(Type T, char c, params object[] parameters) {
    if (!handlers.ContainsKey(T)) {
      BuildTokenList(T);
    }

    TokenHandler th = handlers[T];

    th.NetEvents[c].Invoke(null, parameters);
  }

	private static Dictionary<Type,List<char>> tokens = new Dictionary<Type,List<char>>();
	private static Dictionary<Type,TokenHandler> handlers = new Dictionary<Type,TokenHandler>();

	/// <summary>
	/// Cached token handler reference
	/// </summary>
	private TokenHandler _tokenHandler;
	/// <summary>
	/// Gets the token for class in question. Will generate the token list if it doesn't exist.
	/// If the object modifies it's tokens parameters, it will clone a new handler specific to the object
	/// </summary>
	protected TokenHandler tokenHandler {
		get {
			if (_tokenHandler != null) return _tokenHandler;

			var T = GetType();

			if (handlers.ContainsKey(T))
				return _tokenHandler = handlers[T];

			BuildTokenList(T);
			return _tokenHandler = handlers[T];
		}
	}

	protected class TokenHandler {
		private Dictionary<char,System.Action<EntityBase,object>> setters;
		private Dictionary<char,System.Func<EntityBase,object>> getters;

		public Dictionary<char,MethodInfo> NetEvents = new Dictionary<char,MethodInfo>();

		public TokenHandler() {
			setters = new Dictionary<char,System.Action<EntityBase,object>>();
			getters = new Dictionary<char,System.Func<EntityBase,object>>();
		}

		public TokenHandler DeepClone() {
			TokenHandler nTH = new TokenHandler();

			nTH.setters = new Dictionary<char,System.Action<EntityBase,object>>(this.setters);
			nTH.getters = new Dictionary<char,System.Func<EntityBase,object>>(this.getters);

			nTH.alwaysSend = new Dictionary<char,bool>(this.alwaysSend);
			nTH.alwaysReliable = new Dictionary<char,bool>(this.alwaysReliable);

			nTH.alwaysIsRelaible = this.alwaysIsRelaible;

			nTH.reliableTokens.AddRange(this.reliableTokens);
			nTH.alwaysSendTokens.AddRange(this.alwaysSendTokens);
			nTH.autoTokens.AddRange(this.autoTokens);
			nTH.manualTokens.AddRange(this.manualTokens);

			nTH.NetEvents = this.NetEvents; // This one we keep the reference for

			// Flag the new TokenHandler as not being shared
			nTH.shared = false;

			return nTH;
		}

		public bool shared = true;

		public object get(char c, EntityBase eb) {
			return getter(c)(eb);
		}
		public void set(char c, EntityBase eb, object o) {
			setter(c)(eb, o);
		}

		public System.Func<EntityBase,object> getter(char c){
			return getters[c];
		}
		public System.Action<EntityBase,object> setter(char c) {
			return setters[c];
		}

		public Dictionary<char,bool> 
			alwaysSend = new Dictionary<char,bool>(), 
			alwaysReliable = new Dictionary<char,bool>();

		/// <summary>
		/// If any always send tokens are reliable, implicity, all of them are.
		/// </summary>
		public bool alwaysIsRelaible = false;

		public Dictionary<char,float> updateTimes = new Dictionary<char,float>();
		/// <summary>
		/// Tokens that should always be sent reliably
		/// </summary>
		public List<char> reliableTokens = new List<char>();
		/// <summary>
		/// Tokens that should always be sent whenever another token is sent
		/// </summary>
		public List<char> alwaysSendTokens = new List<char>();
		/// <summary>
		/// Tokens that are automatically dispatched according to the update timer
		/// </summary>
		public List<char> autoTokens = new List<char>();
		/// <summary>
		/// Tokens that are not always send or auto tokens. Useful for getting the subsection of tokens to serialize manually
		/// </summary>
		public List<char> manualTokens = new List<char>();

		/// <summary>
		/// Check each reliable token for if it needs to always be sent, and if any do, always sent tokens require reliable updates.
		/// </summary>
		public void ReEvalAlwaysIsReliable() {
			alwaysIsRelaible = false;

			foreach(var token in reliableTokens) {
				if (alwaysSend.ContainsKey(token)) {
					alwaysIsRelaible = true;
					return;
				}
			}
		}

		public void RegisterField(char c, FieldInfo fi, NetVar nv) {
			getters.Add(c, (e)=> { return fi.GetValue(e); });
			setters.Add(c, (e,v)=> { fi.SetValue(e,v); });
			alwaysSend.Add(c,nv.alwaysSend);
			alwaysReliable.Add(c,nv.alwaysReliable);
			updateTimes.Add(c, nv.updateTime);

			if (nv.alwaysSend)
				alwaysSendTokens.Add(c);
			if (nv.alwaysReliable)
				reliableTokens.Add(c);
			if (nv.updateTime >= 0f) {
				autoTokens.Add(c);
			} else if (!nv.alwaysSend) {
				manualTokens.Add(c);
			}

			if(nv.alwaysSend && nv.alwaysReliable) {
				alwaysIsRelaible = true;
			}
		}
	}

	/// <summary>
	/// Get a list of all tokens used in this class.
	/// </summary>
	public List<char> TokenList() {
		var T = this.GetType();

		if (tokens.ContainsKey(T)) {
			return tokens[T];
		}

		BuildTokenList(T);
		return tokens[T];
	}

	static void BuildTokenList(Type T) {
		if (!T.IsSubclassOf(typeof(EntityBase)))
			throw new System.Exception("Cannot build a token list for a class that doesn't derive EntityBase");

		// Setup the char list
		List<char> charList;

		TokenHandler th;
		// Establish the token handler
		if (!tokens.ContainsKey(T)) {
			charList = new List<char>();
			th = new TokenHandler();
			tokens.Add(T,charList);

			handlers.Add(T,th);
		} else {
			charList = tokens[T];
			th = handlers[T];
		}

		var fields = T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach(FieldInfo fi in fields) {
			var fieldInfo = fi; // Closure fi to prevent variable capture in lambdas
			var netVar = fieldInfo.GetCustomAttributes(typeof(NetVar),true).FirstOrDefault() as NetVar;
			if (netVar == null) continue; // This field has no netvar associated, skip it

			//Debug.LogFormat("{0} Field {1} -> {2}",T.Name,fi.Name,netVar.token);

			charList.Add(netVar.token);
			th.RegisterField(netVar.token, fi, netVar);
		}

		var methods = T.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		// Store all event method infos for remote invocation
		foreach(MethodInfo mi in methods) {
			var methodInfo = mi; // Closure
			var netEvent = methodInfo.GetCustomAttributes(typeof(NetEvent),true).FirstOrDefault() as NetEvent;

			if (netEvent == null) {
				//Debug.LogFormat("Skipping {0}'s {1}", T.Name, methodInfo.Name);
				continue;
			}

			//Debug.LogFormat("EVENT {0}'s {1} -> {2}", T.Name, methodInfo.Name, netEvent.token);
			th.NetEvents.Add(netEvent.token, methodInfo);
		}

    // Search for all static events on this type; In theory this could be merged with the non-static search, but at time of implementing I thought I may process them seperately.
    var staticMethods = T.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
    foreach (MethodInfo mi in staticMethods) {
      var smi = mi; // Closure
      var netEvent = smi.GetCustomAttributes(typeof(NetEvent),true).FirstOrDefault() as NetEvent;

      if (netEvent == null) continue;

      th.NetEvents.Add(netEvent.token, smi);
    }

		var autoTok = string.Join(",", th.autoTokens.Select(t => t.ToString()).ToArray());
		//Debug.LogFormat("{0} Auto Tokens: {1}", T.Name, autoTok);
		var alTok = string.Join(",", th.alwaysSendTokens.Select(t => t.ToString()).ToArray());
		//Debug.LogFormat("{0} Alwy Tokens: {1}", T.Name, alTok);
	}

	public virtual void Awake() {
      if (this is IEarlyAutoRegister)
          Register();

		else if (this is IAutoRegister)
			StartCoroutine(DeferredRegister());
	}

	IEnumerator DeferredRegister() {
		while (!NetworkManager.inRoom)
			yield return null;
		Register();
	}

	/// <summary>
	/// Network variable attribute, specifying the desired token.
	/// Set alwaysReliable to hint that a reliable update is required
	/// Set alwaysSend to always include the variable in all dispatches
	/// </summary>
	/// <remarks>
	/// Always Reliable -> Token must be sent reliably every time
	/// Always Send -> Token will be sent whenever any other token is sent
	/// updateMs -> If set, the token will automatically dispatch every updateMs milliseconds
	/// </remarks>
	[System.AttributeUsage(System.AttributeTargets.Field,AllowMultiple=false)]
	public class NetVar : Attribute {
		public readonly char token;
		public readonly bool alwaysReliable, alwaysSend;
		public readonly float updateTime;

		public NetVar(char token, bool alwaysReliable = false, bool alwaysSend = false, int updateMs = -1) {
			this.token = token;
			this.alwaysReliable = alwaysReliable;
			this.alwaysSend = alwaysSend;
			this.updateTime = updateMs / 1000f; // Convert milliseconds to seconds
		}
	}

	/// <summary>
	/// This attribute describes a networked event function; This function must be non-static and is called on a specific entity.
	/// It may have any network serializable parameters
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method,AllowMultiple=false)]
	public class NetEvent : Attribute {
		public readonly char token;

		public NetEvent(char token) {
			this.token = token;
		}
	}


	/// <summary>
	/// Attach to a static field returning either a GameObject or EntityBase
	/// This function will be called to create a networked entity.
	/// The method may contain any number of parameters that are serializable
	/// The EntityID 
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class Instantiation : Attribute { }

	#endregion
}