using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

	public class HorizontalNav : MonoBehaviour {
	public Button leftBtn;
	public Button rightBtn;
	public Swiper swiper;
	public NavDot navDotPrefab;
	public RectTransform navDotsParent;
	public CanvasGroup navDotsCanv;
	private bool leftIsVisible, rightIsVisible;
	private bool leftBtnActive, rightBtnActive;
	private bool layerActive = true;
	private System.Action updateCB;
	private System.Action clickCallbackLeft;
	private System.Action clickCallbackRight;

	private Tweener navDotsTween = null;

	private bool navDotsOn = false;
	public bool NavDotsOn { get { return navDotsOn; } }

	private UnityEngine.Events.UnityAction clickLeftAct, clickedRightAct;

	void Awake()
	{
		leftBtn.gameObject.SetActive(false);
		rightBtn.gameObject.SetActive(false);

		leftBtn.image.color = new Color(1,1,1,0);
		rightBtn.image.color = new Color(1,1,1,0);
	}

	public void SetUpdateCallback(System.Action cB)
	{
		updateCB = cB;
	}

	public void Setup(UnityEngine.Events.UnityAction clickedLeft, UnityEngine.Events.UnityAction clickedRight, System.Action onSwipeSelection = null, System.Action<float, bool> onSwipeDrag = null)
	{
		clickLeftAct = clickedLeft;
		clickedRightAct = clickedRight;

		leftBtn.onClick.RemoveAllListeners();
		leftBtn.onClick.AddListener(()=>{
			if(leftBtnActive)
				clickedLeft();
		});

		rightBtn.onClick.RemoveAllListeners();
		rightBtn.onClick.AddListener(()=>{
			if(rightBtnActive)
				clickedRight();
		});

		if(onSwipeSelection != null && onSwipeDrag != null && swiper != null)
		{
			swiper.Setup(onSwipeSelection, onSwipeDrag);
		}

		if(navDotsCanv != null)
		{
			ShowOrHideDots(layerActive);
		}

		Debug.Log("hor nav setup");
	}

	void Update()
	{
		if(updateCB != null)
		{
			updateCB();
		}
		if(layerActive && leftBtnActive && Input.GetKeyDown(KeyCode.LeftArrow))
		{
			clickLeftAct();
		}

		if(layerActive && rightBtnActive && Input.GetKeyDown(KeyCode.RightArrow))
		{
			clickedRightAct();
		}
	}
		
	public void SetLayerActive(bool active)
	{

		if(swiper != null)
		{
			swiper.ownGO.SetActive(active);
		}
		layerActive = active;

		ShowOrHideDots(active);
	}

	public void SetBtnActive(bool left, bool right)
	{
		leftBtnActive = left;
		rightBtnActive = right;
	}

	public void UpdateVisibility()
	{
		bool leftToBeVisible = layerActive && leftBtnActive;
		if(leftToBeVisible != leftIsVisible)
		{
			SetBtnVisible(true, leftToBeVisible);
		}

		bool rightToBeVisible = layerActive && rightBtnActive;
		if(rightToBeVisible != rightIsVisible)
		{
			SetBtnVisible(false, rightToBeVisible);
		}
	}
		
	private Sequence SetBtnVisible(bool left, bool visible)
	{
		//Debug.Log("Setting nav btn " + (left ? "left" : "right") + " to visibility: " + visible);
		if(left)
			leftIsVisible = visible;
		else
			rightIsVisible = visible;
		
		Sequence seq = DOTween.Sequence();

		Button target = left ? leftBtn : rightBtn;

		target.gameObject.SetActive(true);

		Tweener fader = target.image.DOFade(visible ? 1 : 0, .5f);

		fader.OnComplete(()=>target.gameObject.SetActive(visible));

		seq.Append(fader);

		return seq;
	}

	public void ShowOrHideDots(bool show)
	{
		if(navDotsTween != null && !navDotsTween.IsComplete())
		{
			navDotsTween.Kill(false);
		}
		navDotsTween = navDotsCanv.DOFade(show ? 1f:0f, 1f);
		navDotsOn = show;
	}
}
