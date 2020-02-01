using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using System.Reflection;

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;

using Hashtable = ExitGames.Client.Photon.Hashtable;

using Type = System.Type;
using Action = System.Action;

namespace EntityNetwork {

	public static class EntityManager {
		public static Dictionary<int,EntityBase> entities = new Dictionary<int,EntityBase>();
  
		/// <summary>
		/// Get an unused EntityID for a given PlayerID.
		/// This ID is ensured to be unique for the client, assuming each client has a different PlayerID it will not collide.
		/// Each playerID's ID space is about two hundred and sixty eight million IDs. 
		/// </summary>
		/// <returns>An unused EntityBase ID number</returns>
		/// <param name="playerID">The player number, ranged [0,127]</param>
		public static int GetUnusedID(int playerID) {
			if (playerID > 127)
				throw new System.ArgumentOutOfRangeException("playerID cannot exceed 127");
			if (playerID < 0) 
				throw new System.ArgumentOutOfRangeException("playerID cannot be less than zero");
			

			// Fill all but the topmost byte randomly, then the topmost byte will be an sbyte for player id

			int player = playerID << 28;
			int randomInt = Random.Range(0, 0x0FFFFFFF);

			int proposedID = player | randomInt;

			// Recursively dig for new ID's on collision
			while (entities.ContainsKey(proposedID)) {
				proposedID = GetUnusedID(playerID);
			}

			return proposedID;
		}

		/// <summary>
		/// Get a reference to an entity of a given ID.
		/// There is a chance that this entity may have been deleted.
		/// </summary>
		/// <param name="id">Entity ID</param>
		public static EntityBase Entity(int id) {
			EntityBase eb;
			return entities.TryGetValue(id, out eb) ? eb : null;
		}

    /// <summary>
    /// Get a reference to an entity of a given ID.
    /// There is a chance that this entity may have been deleted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T Entity<T>(int id) where T : EntityBase{
      return Entity(id) as T;
    }

		private static void CleanEntities() {
			var toRemove = new List<int>(entities.Count);

			foreach(var pair in entities) 
				if (!pair.Value) toRemove.Add(pair.Key);
			
			foreach (var key in toRemove)
				entities.Remove(key);
		}

		/// <summary>
		/// Register an entity to receive network events/updates.
		/// This will fail if the EntityID is already in use.
		/// </summary>
		/// <remarks>Registering an entity validates all existing entities and will unsubcribe dead entities.</remarks>
		public static void Register(EntityBase eb) {
			CleanEntities();

          //Debug.LogFormat("Registered Entity {0} : {1}",eb.name,eb.EntityID);

          //Debug.LogFormat("{0} -> {1}", eb.name, string.Join(", ", eb.GetType().GetInterfaces().Select(t => t.Name).ToArray()));
			if (eb is IAutoSerialize) {
				eb.StartCoroutine(autoDispatchEntity(eb));
			}

			if (entities.ContainsKey(eb.EntityID)) {
				var otherEntity = Entity(eb.EntityID);
				Debug.LogErrorFormat(eb, "{0} has attempted to register over an existing ID {1}, Which belongs to {2}",
					eb.gameObject.name, eb.EntityID, otherEntity.gameObject.name);
				throw new System.Exception("Entity ID already in use!");
			}

			entities.Add(eb.EntityID, eb);
		}

		/// <summary>
		/// Deregister an EntityBase. This requires enumeraing all entities and is slower than just destroying the EntityBase
		/// However, in certain cases, like re-registering as a new pooled object or if the object must exist after being removed, it's worth using
		/// </summary>
		public static void DeRegister(EntityBase eb) {
			// Grab all keyvaluepairs where the entity is the entity base  being deregistered - ToArray is used to collapse the linq to avoid sync issues
			var toRemove = entities.Select(t => new {id = t.Key, Entity = t.Value}).Where(t => t.Entity == eb).ToArray();
			foreach(var removal in toRemove) {
				entities.Remove(removal.id);
			}

		}

		public static IEnumerator autoDispatchEntity(EntityBase eb) {
			Debug.LogFormat("Creating Serial Dispatcher for {0} : {1}", eb.name,eb.EntityID);

			Hashtable h = new Hashtable();

			while (true) {
				h.Clear();

				if (eb.isMine) {
					int code = eb.SerializeAuto(h);

					if (code != 0) {
						// If code is 2, message should be reliable
						// Debug.LogFormat("Dispatching {0}/{2}: {1}", eb.name, h.ToStringFull(), PhotonConstants.EntityUpdateCode);
						NetworkManager.netMessage(PhotonConstants.EntityUpdateCode, h, code == 2);
					}
				}

				yield return null;
			}
		}

		static EntityManager() {
			NetworkManager.netHook += OnNet;
      NetworkManager.onLeave += AllowOrphanSuicidesAndCalls;
		}

		// Hook the main events
		static void OnNet(EventData ev) {
      if (ev.Code == PhotonConstants.EntityUpdateCode) {
          var h = (Hashtable)ev[ParameterCode.Data];

			 // Reject self-aimed events
			 if ((int)ev[ParameterCode.ActorNr] == NetworkManager.localID){
				  // Show a red particle for an outgoing signal, before rejecting the event
				  var ebs = Entity((int)h[PhotonConstants.eidChar]);
				  NetworkManager.netParticle(ebs, Color.red);
				  return;
			 } 

			 var eb = Entity((int)h[PhotonConstants.eidChar]);

			 if (eb) {
				  // Show a blue particle for an incoming singal
				  NetworkManager.netParticle(eb, Color.blue);

				  if (eb is IAutoDeserialize) {
					   eb.DeserializeFull(h);
				  }
				  eb.Deserialize(h);
			 }
			}

			if (ev.Code == PhotonConstants.EntityEventCode) {
				var h = (Hashtable)ev[ParameterCode.Data];

        //  --- Static Events ---
        // Param labeled 2 in the hashtable is the EntityBase's ID Type, if a static event call, so if the table contains key 2, run it as a static event
        object idObject;
        if (h.TryGetValue(2,out idObject)) {
          var typeID = (int)idObject;
          Type entityType;
          try {
            entityType = EntityBase.TypeFromID(typeID);
          } catch {
            throw new System.Exception("Attempting to call static event on a non-existant type");
          }

          var controlChar = (char)h[0];

          object paramObject;
          if (h.TryGetValue(1,out paramObject)) {
            EntityBase.InternallyInvokeStatic(entityType, controlChar,(object[])paramObject);
          } else {
            EntityBase.InternallyInvokeStatic(entityType, controlChar, null);
          }

          return;
        }

        // --- Instance Events ---
				var eb = Entity((int)h[PhotonConstants.eidChar]);

				if (eb) {
					var controlChar = (char)h[0];

					object paramObject;
					if (h.TryGetValue(1,out paramObject)) {
						eb.InternallyInvokeEvent(controlChar,(object[])paramObject);
					} else {
						eb.InternallyInvokeEvent(controlChar, null);
					}
				}
			}

			if (ev.Code == PhotonConstants.EntityInstantiateCode) {
				var h = (Hashtable)ev[ParameterCode.Data];
				DeserializeInstantiate(h);
			}
		}

		/// <summary>
		/// Generate a hashtable describing an object instantiaton for use with DeserializeInstantiate
		/// Use helper method Instantiate to automatically call and fire this as an event.
		/// </summary>
		/// <seealso cref="DeserializeInstantiate"/>
		public static Hashtable SerializeInstantiate<T>(int authID, Vector3 pos, Quaternion rot, params object[] param) {
			var H = new Hashtable();

			//H.Add('T', typeof(T).ToString());

			H.Add('O', authID);
			H.Add('I', GetUnusedID(authID));

			H.Add('T', typeof(T).FullName);

			H.Add('P', pos);
			H.Add('R', rot);
			H.Add('p', param);

			return H;
		}

		/// <summary>
		/// Locally creates the instantiated object described in Hashtable H.
		/// </summary>
		/// <seealso cref="Instantiate"/>
		public static void DeserializeInstantiate(Hashtable H) {
			CheckInstantiators();

			//Debug.Log(H.ToStringFull());
			var type = typeLookup[H['T'] as string];

			var eid = (int)H['I'];
			var ID = (int)H['O'];

			var pos = (Vector3)H['P'];
			var rot = (Quaternion)H['R'];

			var options = H['p'] as object[];

			ActionInstantiate(ID, eid, type, pos, rot, options);

			//Instantiate<type>(pos, rot, options);
		}

		#region Instantiation
		// Instantiation uses InstanceGameObject / InstanceGameEntity attributes

		// Actually construct an instantiator object
		private static void ActionInstantiate(int authID, int entityID, Type T, Vector3 pos, Quaternion rot, object[] param) {
			MethodInfo mi;
			if (!InstantiateMethods.TryGetValue(T, out mi)) {
				throw new System.Exception(string.Format("Type {0} doesn't have an Instantiate Attributed method and isn't Instantiable.", T.Name));
			}

			if (typeof(GameObject).IsAssignableFrom(mi.ReturnType)) {
				var val = mi.Invoke(null, param) as GameObject;

				var go = Object.Instantiate<GameObject>(val, pos, rot);

				// Attempt to set the ID of the entitybase
				var eb = go.GetComponentInChildren<EntityBase>();

				if (eb) {
					eb.EntityID = entityID;
					eb.authorityID = authID;
				}

				go.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

				return;
			}

			var rt = mi.ReturnType;
			if (typeof(EntityBase).IsAssignableFrom(rt)) {
				var eb = mi.Invoke(null, param) as EntityBase;

				eb.authorityID = authID;
				eb.EntityID = entityID;

				var go = eb.gameObject;
				var t = eb.transform;

				if (pos != Vector3.zero)
					t.position = pos;
				if (rot != Quaternion.identity)
					t.rotation = rot;

				go.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

				return;
			}

			throw new System.Exception(string.Format("Type {0}'s Instantiate Method doesn't return an EntityBase or GameObject", T.Name));
		}

		// Helper dictionaries. typeLookup is to help us send types over the wire, InstantiateMethods stores each types instantiator
		static Dictionary<string,System.Type> typeLookup;
		static Dictionary<System.Type,MethodInfo> InstantiateMethods;


		// This is a mess of autodocumentation, mostly due to usage of params and overloads.

		/// <summary>
		/// Activate type T's EntityBase.Instantation attribute remotely with given parameters, Generating and assigning the appropriate actor ID
		/// This method returns the HashTable describing the instantation request that can be used to also create the object locally.
		/// </summary>
		/// <seealso cref="DeserializeInstantiate"/>
		public static Hashtable Instantiate<T>(int authID, params object[] param) { return Instantiate<T>(authID, Vector3.zero, Quaternion.identity, param); }
		/// <summary>
		/// Activate type T's EntityBase.Instantation attribute remotely with given parameters, Generating and assigning the appropriate actor ID
		/// This method returns the HashTable describing the instantation request that can be used to also create the object locally.
		/// </summary>
		/// <seealso cref="DeserializeInstantiate"/>
		public static Hashtable Instantiate<T>(int authID, Vector3 pos, params object[] param) { return Instantiate<T>(authID, pos, Quaternion.identity, param); }
		/// <summary>
		/// Activate type T's EntityBase.Instantation attribute remotely with given parameters, Generating and assigning the appropriate actor ID
		/// This method returns the HashTable describing the instantation request that can be used to also create the object locally.
		/// </summary>
		/// <seealso cref="DeserializeInstantiate"/>
		public static Hashtable Instantiate<T>(int authID, Vector3 pos, Quaternion rot) { return Instantiate<T>(authID, pos, rot, null); }
		/// <summary>
		/// Activate type T's EntityBase.Instantation attribute remotely with given parameters, Generating and assigning the appropriate actor ID
		/// This method returns the HashTable describing the instantation request that can be used to also create the object locally.
		/// </summary>
		/// <seealso cref="DeserializeInstantiate"/>
		public static Hashtable Instantiate<T>(int authID, Vector3 pos, Quaternion rot,params object[] param) {
			var table = SerializeInstantiate<T>(authID, pos, rot, param);

			if (NetworkManager.isReady) {
				NetworkManager.net.OpRaiseEvent(PhotonConstants.EntityInstantiateCode, table, true, RaiseEventOptions.Default);
			}

			return table;
		}

		static bool InstantiatorsBuilt = false;
		static void CheckInstantiators() {
			if (InstantiatorsBuilt) return;

			BuildInstantiators();
			InstantiatorsBuilt = true;
		}

		// Gather all the instantiaton attributes on entity classes
		static void BuildInstantiators() {
			Debug.Log("Buiding Instantiator cache");

			InstantiateMethods = new Dictionary<System.Type,MethodInfo>();
			typeLookup = new Dictionary<string,System.Type>();

			var ebT = typeof(EntityBase);

			var AllEntityTypes = 
				System.AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(t => ebT.IsAssignableFrom(t));
			
			//var AllEntityTypes = Assembly.GetTypes().Where(t => ebT.IsAssignableFrom(t));

			foreach(var entityType in AllEntityTypes) {
				var methods = entityType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				typeLookup.Add(entityType.FullName, entityType);
				//Debug.LogFormat("Scanning Type {0}", entityType);

				foreach(var method in methods) {
					//Debug.LogFormat("Scanning Method {0}", method.Name);
					// First look for a GameObject instantiator
					var ia = method.GetCustomAttributes(typeof(EntityBase.Instantiation),true).FirstOrDefault() as EntityBase.Instantiation;
					if (ia != null) {
						InstantiateMethods.Add(entityType, method);
						Debug.LogFormat("Registering Instantiator {0} for {1} (R: {2})",method.Name,entityType.FullName,method.ReturnType.ToString());
					}
				}
			}
		}

    static void AllowOrphanSuicidesAndCalls(EventData ev) {
      var toKill = new List<int>();
      var players = NetworkManager.net.CurrentRoom.Players.Select(t => t.Value.ID);

      foreach(var pair in entities) {
        var e = pair.Value;

        if (e is IAutoKillOrphans) {
          if (e.authorityID == -1) continue;
          if (players.Contains(e.authorityID)) continue;

          toKill.Add(e.EntityID);
        }
        // Send out the orphan callbacks
        if (e is IOrphanCallback) {
          if (e.authorityID == -1) return;
          if (players.Contains(e.authorityID)) continue;

          (e as IOrphanCallback).OnOrphaned();
        }
      }

      // Kill the orphans
      foreach(var killable in toKill) {
        if (Application.isEditor || Debug.isDebugBuild) {
          var killEntity = Entity(killable);

          Debug.LogFormat("Destroying orphaned entity {0} as it's owner {1} has left the room.",killEntity.gameObject.name,killEntity.authorityID);
        }
        Object.Destroy(Entity(killable).gameObject);
      }
    }

		#endregion
	}
	/// <summary>
	/// Specify that deserialization should be automaticly handled.
	/// All registered field tokens will be automaticly set using cached setters
	/// This is not appropriate if you have custom serialization/deserialization logic
	/// </summary>
	public interface IAutoDeserialize {}

	/// <summary>
	/// Specify that automatic token handling should be performed on the entity.
	/// In most cases, this should remove the need to write custom serializers
	/// This only applies to NetVar's with alwaysSend or updateTime set
	/// </summary>
	public interface IAutoSerialize {}

	/// <summary>
	/// Only appropriate for Entities with fixed, pre-determined ID's. 
	/// The entity will attempt to register itself on Awake()
	/// </summary>
	public interface IAutoRegister {}

  /// <summary>
  /// Only appropriate for Entities with fixed, pre-determined ID's. 
  /// The entity will absolutely to register itself on Awake()
  /// </summary>
  public interface IEarlyAutoRegister {}

  /// <summary>
  /// Assign to an EntityBase so that any time an AuthorityID would be checked we instead check if we're the room master
  /// Used to clarify network ownership for objects that aren't owned by a player but instead by the room itself
  /// </summary>
  public interface IMasterOwnsUnclaimed {}

  /// <summary>
  /// When the authority player disconnects, destroy the entity and attached gameobject that aren't owned by players (EXCEPT with AuthID -1)
  /// </summary>
  public interface IAutoKillOrphans {}

  /// <summary>
  /// Adds an OnOrphaned callback - Note this is run whenever a player quits and we are unclaimed without a -1 authority, not just when our authority quits.
  /// </summary>
  public interface IOrphanCallback {
    void OnOrphaned();
  }
}