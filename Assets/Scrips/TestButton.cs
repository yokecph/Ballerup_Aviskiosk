using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour {

	public Button btn;

	// Use this for initialization
	void Start () {
		btn.onClick.AddListener(()=>{Debug.Log("CLick");});
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
