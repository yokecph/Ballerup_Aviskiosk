using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T:MonoBehaviour{

	private static T _instance = null;
	public static T Instance { get {
			_instance = _instance ?? Object.FindObjectOfType<T>();

			if(_instance == null)
			{
				#if UNITY_EDITOR
				if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
				{
					return null;
				}
				#endif
				GameObject newGO = new GameObject(typeof(T).ToString() + "_Singleton");
				_instance = newGO.AddComponent<T>();
			}
			return _instance;
		}
	}
}
