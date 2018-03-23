using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowserWrapperTester : MonoBehaviour {

	public BrowserWrapper wrapper;
	// Use this for initialization
	void Start () {

		System.Action<bool> onLoadDone = (success)=> {
			Debug.Log("Load success: " + success);
		};

		System.Action onReturn = ()=> {
			Debug.Log("On return");
		};

		wrapper.LoadMagId("941", onLoadDone, onReturn);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
