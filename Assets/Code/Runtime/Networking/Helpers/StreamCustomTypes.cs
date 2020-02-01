using System;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class StreamCustomTypes {
	public static void Register() {
		PhotonPeer.RegisterType(typeof(Vector2), (byte)'V', SerializeVector2, DeserializeVector2);
		PhotonPeer.RegisterType(typeof(Vector3), (byte)'W', SerializeVector3, DeserializeVector3);
		PhotonPeer.RegisterType(typeof(Quaternion), (byte)'Q', SerializeQuaternion, DeserializeQuaternion);
		PhotonPeer.RegisterType(typeof(char), (byte)'c', SerializeChar, DeserializeChar);
	}

	private static short SerializeVector2(StreamBuffer outStream, object customObj) {
		var vo = (Vector2)customObj;

		var ms = new MemoryStream(2 * 4);

		ms.Write(BitConverter.GetBytes(vo.x), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.y), 0, 4);

		outStream.Write(ms.ToArray(), 0, 2 * 4);
		return 2 * 4;
	}

	private static object DeserializeVector2(StreamBuffer inStream, short length) {
		var bytes = new Byte[2 * 4];
		inStream.Read(bytes, 0, 2 * 4);
		return new 
			Vector2(
				BitConverter.ToSingle(bytes, 0),
				BitConverter.ToSingle(bytes, 4));

		// As best as I can tell, the new Protocol.Serialize/Deserialize are written around WP8 restrictions
		// It's not worth the pain.

		//int index = 0;
		//float x, y;
		//Protocol.Deserialize(out x, bytes, ref index);
		//Protocol.Deserialize(out y, bytes, ref index);

		//return new Vector2(x, y);
	}

	private static short SerializeVector3(StreamBuffer outStream, object customObj) {
		Vector3 vo = (Vector3)customObj;

		var ms = new MemoryStream(3 * 4);

		ms.Write(BitConverter.GetBytes(vo.x), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.y), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.z), 0, 4);

		outStream.Write(ms.ToArray(), 0, 3 * 4);
		return 3 * 4;
	}

	private static object DeserializeVector3(StreamBuffer inStream, short length) {
		var bytes = new byte[3 * 4];

		inStream.Read(bytes, 0, 3 * 4);

		return new 
			Vector3(
				BitConverter.ToSingle(bytes, 0),
				BitConverter.ToSingle(bytes, 4),
				BitConverter.ToSingle(bytes, 8));
	}

	private static short SerializeQuaternion(StreamBuffer outStream, object customObj) {
		Quaternion vo = (Quaternion)customObj;

		var ms = new MemoryStream(4 * 4);

		ms.Write(BitConverter.GetBytes(vo.x), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.y), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.z), 0, 4);
		ms.Write(BitConverter.GetBytes(vo.w), 0, 4);

		outStream.Write(ms.ToArray(), 0, 4 * 4);
		return 4 * 4;
	}

	private static object DeserializeQuaternion(StreamBuffer inStream, short length) {
		var bytes = new byte[4 * 4];

		inStream.Read(bytes, 0, 4 * 4);

		return new 
			Quaternion(
				BitConverter.ToSingle(bytes, 0),
				BitConverter.ToSingle(bytes, 4),
				BitConverter.ToSingle(bytes, 8),
				BitConverter.ToSingle(bytes, 12));
	}

	private static short SerializeChar(StreamBuffer outStream, object customObj) {
		outStream.Write(new[]{ (byte)((char)customObj) }, 0, 1);
		return 1;
	}

	private static object DeserializeChar(StreamBuffer inStream, short Length) {
		var bytes = new Byte[1];
		inStream.Read(bytes, 0, 1);

		return (char)bytes[0];
	}
}
