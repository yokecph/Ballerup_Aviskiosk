using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviourSingleton<SettingsPanel> {
	public Slider dragSensSlider;
	public GameObject onOffGO;
	const string kSensitivityKey = "dragSensitivity";

	private float dragSensitivity = -1;
	public float DragSensitivity { get { return dragSensitivity; } }



	// Use this for initialization
	void Start () {

		dragSensitivity = PlayerPrefs.HasKey(kSensitivityKey) ? PlayerPrefs.GetFloat(kSensitivityKey) : 1;


		dragSensSlider.value = dragSensitivity;

		dragSensSlider.onValueChanged.AddListener((f)=>
			{
				dragSensitivity = f;
				PlayerPrefs.SetFloat(kSensitivityKey, f);
			});
		
	}
}
