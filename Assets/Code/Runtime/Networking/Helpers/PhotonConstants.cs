using UnityEngine;
using System.Collections.Generic;

public static class PhotonConstants {
  public static readonly byte EntityUpdateCode = 110;
  public static readonly byte EntityEventCode = 105;
  public static readonly byte EntityInstantiateCode = 106;
  public static readonly char eidChar = (char)206; // 'Î'
  public static readonly char athChar = (char)238; // 'î'
  public static readonly char insChar = (char)207; // 'Ï'

  /// <summary>
  /// Region names strings
  /// </summary>
  public static readonly Dictionary<string,string> RegionNames = new Dictionary<string,string>() {
    {"asia","Signapore"},
    {"au","Australia"},
    {"cae","Montreal"},
    {"cn","Shanghai"},
    {"eu","Europe"},
    {"in","India"},
    {"jp","Japan"},
    {"ru","Moscow"},
    {"rue","East Russia"},
    {"sa","Brazil"},
    {"kr","South Korea"},
    {"us","Eastern US"},
    {"usw","Western US"}
  };
}
