using System;
using System.Collections.Generic;
using Speckle.Core.Kits;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleUnity
	{
		public const string HostApp = HostApplications.Unity.Name;

		public const string NameSpace = "Speckle";

		#if UNITY_EDITOR
		public static List<T> GetAllInstances<T>() where T : ScriptableObject
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
			var items = new List<T>();
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				items.Add(AssetDatabase.LoadAssetAtPath<T>(path));
			}

			return items;
		}
		#endif

		public static class Console
		{
			public const string title = "speckle-connector:";

			public static void Log(string msg)
			{
				Debug.Log(title + " " + msg);
			}

			public static void Exception(Exception exception)
			{
				Debug.LogException(exception);
			}

			public static void Warn(string message)
			{
				Debug.LogWarning(title + message);
			}
		}
	}
}