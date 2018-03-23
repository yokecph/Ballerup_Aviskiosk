using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingBackground : MonoBehaviour {

	public Image loadingImage;
	public GameObject loadingImgGO;
	public RectTransform loadingImgRect;
	public CanvasGroup loadingGroup;
	// Use this for initialization

	private bool activeDot = false;
	public void ToggleLoadingImg(bool on)
	{
		activeDot = false;
		loadingImage.color = Color.black;
		Debug.Log("Setting loading to: " + on);
		loadingGroup.alpha = on ? 0 : 1;
		loadingImgGO.SetActive(on);
		Sequence seq = DOTween.Sequence();
		seq.Insert(1f, loadingGroup.DOFade(on ? 1 : 0, 1f));
		seq.AppendCallback(()=>activeDot = true);

	}

	void Update()
	{
		if(activeDot)
		{
			float lerp = Mathf.Pow((Time.time % 2 - 1), 2);
			loadingImage.color = Color.Lerp(Color.white * 0.5f, Color.white * 0.8f, lerp);
			loadingImgRect.sizeDelta = Vector2.Lerp(new Vector2(16,16), new Vector2(32,32), lerp);
		}
	}
}
