using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwipeIndicator : MonoBehaviour {

	public RectTransform ownTransf;
	public GameObject ownGO;
	public CanvasGroup ownGroup;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Sequence DoMoveAndFade(Vector2 startPos, float xOffset, float moveDuration, float fadeDuration, float stayOnDuration = 0)
	{
		ownGroup.alpha = 0;
		ownTransf.anchoredPosition = startPos;
		Sequence seq = DOTween.Sequence();
		seq.Insert(0, ownGroup.DOFade(1f, fadeDuration));
		seq.Insert(0, ownTransf.DOAnchorPos(new Vector2(startPos.x + xOffset, startPos.y), moveDuration));
		seq.Insert(moveDuration + stayOnDuration - fadeDuration, ownGroup.DOFade(0f, fadeDuration));
		seq.AppendCallback(()=>Object.Destroy(ownGO));
		return seq;
	}
}
