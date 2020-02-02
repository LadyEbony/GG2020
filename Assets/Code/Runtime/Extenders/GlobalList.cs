using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;

using Type = System.Type;

namespace GameSpawn {

  /// <summary>
  /// Generic static list of <typeparamref name="T"/>. Add and remove the contents on your accord.
  /// </summary>
  /// <remarks>
  /// GlobalList provides ease of use when you need to track every to most
  /// references of the same type.
  /// 
  /// You must specify the exact Type. As such, this is not friendly towards inheritance classes.
  /// </remarks>
  /// <typeparam name="T"></typeparam>
  public static class GlobalList<T> {

    private static List<T> list;

    static GlobalList() {
      list = new List<T>();
    }

    /// <summary>
    /// The unmodifyable list of <see cref="GlobalList{T}"/>.
    /// </summary>
    public static IReadOnlyCollection<T> GetList {
      get {
        return list.AsReadOnly();
      }
    }

    public static List<T> GetListUnsafe{
      get {
        return list;
      }
    }

    public static void Add(T item){
      list.Add(item);
    }

    public static bool Remove(T item) {
      return list.Remove(item);
    }
  }

  /// <summary>
  /// Generic static list of <typeparamref name="T"/>. Uses mask to seperate Add and remove the contents on your accord.
  /// </summary>
  /// <remarks>
  /// GlobalList provides ease of use when you need to track every to most
  /// references of the same type.
  /// 
  /// You must specify the exact Type. As such, this is not friendly towards inheritance classes.
  /// </remarks>
  /// <typeparam name="T"></typeparam>
  public static class GlobalMaskList<T> {
    private static List<T>[] maskedLists;

    static GlobalMaskList() {
      maskedLists = new List<T>[8];
      for (int i = 0; i < maskedLists.Length; i++)
        maskedLists[i] = new List<T>();
    }

    /// <summary>
    /// The unmodifyable masked list of <see cref="GlobalMaskList{T}"/> at <paramref name="mask"/>.
    /// </summary>
    public static IReadOnlyCollection<T> GetMaskedList(int mask) {
      return maskedLists[mask];
    }

    public static void Add(T item, int mask) {
      GlobalList<T>.Add(item);
      maskedLists[mask].Add(item);
    }

    public static bool Remove(T item, int mask) {
      var r1 = GlobalList<T>.Remove(item);
      var r2 = maskedLists[mask].Remove(item);
      return r1 || r2;
    }

  }

  public static class GlobalTypeList<T> {
    private static Dictionary<Type, List<T>> typeDictionary;

    static GlobalTypeList() {
      typeDictionary = new Dictionary<Type, List<T>>();
    }

    /// <summary>
    /// The unmodifyable masked list of <see cref="GlobalTypeList{T}"/> at <paramref name="type"/>.
    /// </summary>
    public static IReadOnlyCollection<T> GetTypeList(Type type) {
      List<T> value;
      return typeDictionary.TryGetValue(type, out value) ? value : null;
    }

    /// <summary>
    /// The unmodifyable masked list of <see cref="GlobalTypeList{T}"/> at <paramref name="types"/>.
    /// </summary>
    public static IReadOnlyCollection<T> GetTypeList(params Type[] types) {
      List<T> value = new List<T>();
      IReadOnlyCollection<T> result;
      foreach(var type in types){
        result = GetTypeList(type);
        if (result != null) value.Concat(result);
      }
      return value;
    }

    public static void Add(T item) {
      var type = item.GetType();
      List<T> value;
      if (!typeDictionary.TryGetValue(type, out value)){
        value = new List<T>();
        typeDictionary.Add(type, value);
      }
      value.Add(item);
    }

    public static bool Remove(T item) {
      var type = item.GetType();
      List<T> value;
      if (!typeDictionary.TryGetValue(type, out value)) return false;
      return value.Remove(item);
    }

  }

  /// <summary>
  /// Wrapper class that adds and removes items from <see cref="GlobalList{T}"/> 
  /// using <see cref="object.GetType()"/> as the type. 
  /// Less effecient than using <see cref="GlobalList{T}"/> directly.
  /// </summary>
  /// <remarks>
  /// Uses cached reflection to get the relevent GlobalList.Add()
  /// and GlobalList.Remove() methods.
  /// 
  /// This wrapper class is much more friendly to inheritance.
  /// However, GlobalList can cause long pauses at the beginning if you use these methods extensively.
  /// </remarks>
  public static class GlobalList{
    
    private struct MethodInfoStruct{
      public MethodInfo Add;
      public MethodInfo Remove;
    }

    private static Dictionary<Type, MethodInfoStruct> methodInfo;

    static GlobalList(){
      methodInfo = new Dictionary<Type, MethodInfoStruct>();
    }

    private static MethodInfoStruct GetMethodInfo(Type type){
      MethodInfoStruct info;

      if (!methodInfo.TryGetValue(type, out info)){
        var genericType = typeof(GlobalList<>).MakeGenericType(type);
        var add = genericType.GetMethod("Add");
        var remove = genericType.GetMethod("Remove");

        info = new MethodInfoStruct() { Add = add, Remove = remove };
      }

      return info;
    }

    /// <summary>
    /// Adds the <paramref name="item"/> to the <see cref="GlobalList{T}"/> according to its <see cref="object.GetType()"/>.
    /// </summary>
    /// <param name="item"></param>
    public static void AddReference(object item){
      GetMethodInfo(item.GetType()).Add.Invoke(null, new object[] { item });
    }

    /// <summary>
    /// Removes the <paramref name="item"/> to the <see cref="GlobalList{T}"/> according to its <see cref="object.GetType()"/>.
    /// </summary>
    /// <param name="item"></param>
    public static void RemoveReference(object item) {
      GetMethodInfo(item.GetType()).Remove.Invoke(null, new object[] { item });
    }

  }

}
