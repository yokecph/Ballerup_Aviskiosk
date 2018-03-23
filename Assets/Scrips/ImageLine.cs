using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class ImageLine : MonoBehaviour {

	public Image leftImg;
	public Image middleImg;
	public Image rightImg;
	public RectTransform imageLineTransf;
	public GenericSwiper swiper;
	private int middleIndex = 0;
	public int level = 0;
	private float lastinertiaMean = 0;
	private bool isDragging = false;
	private bool isInertiaTraveling = false;
	private bool showDebug = false;

	public bool IsMoving { get { return isDragging || isInertiaTraveling || snapSeq != null && snapSeq.IsActive(); } }

	private IEnumerator inertiaRoutine;

	private RunningMeaner<float> dragMean = new RunningMeaner<float>(10, (f1,f2)=>{return f1+f2; }, (f,i)=>{return f/i;}, true);
	private Sequence snapSeq;

	public ImagesAndUrl CurrentSelected { get { return isSetup ? myIAUs[middleIndex] : null; } } 

	private ImagesAndUrl[] myIAUs;
	private bool isSetup = false;
	// Use this for initialization

	private System.Action updatedCB;
	void Start () {
		swiper.Setup(OnDrag, OnDragEnd);
		Debug.Log("modulo: " + (2%3));
		
	}
	public void Setup(List<ImagesAndUrl> images, System.Action updatedCallback)
	{
		updatedCB = updatedCallback;
		myIAUs = images.ToArray();
		isSetup = true;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateLine();
		if(Input.GetKeyDown(KeyCode.D))
		{
			showDebug = !showDebug;
		}
	}

	public Sequence SimulateDrag(float xOffset, float moveTime, float steadyTime, float moveBackTime)
	{
		Sequence seq = DOTween.Sequence();

		Tween tIn = imageLineTransf.DOAnchorPosX(xOffset, moveTime);
		tIn.SetEase(Ease.InOutCirc);
		seq.Append(tIn);
		seq.AppendInterval(steadyTime);
		Tween tOut = imageLineTransf.DOAnchorPosX(0, moveBackTime);
		tOut.SetEase(Ease.InOutCirc);
		seq.Append(tOut);

		return seq;
	}

	private void OnDrag(Vector2 delta)
	{
		isDragging = true;
		dragMean.Push(delta.x);
		//Debug.Log("Drag mean: " + dragMean.GetMean());
		EnsureSnapDead();
		Vector2 linePos = imageLineTransf.anchoredPosition;
		linePos.x += delta.x;
		imageLineTransf.anchoredPosition = linePos;
	}


	private void OnDragEnd()
	{
		isDragging = false;
		float mean = dragMean.GetMean();
		lastinertiaMean = mean;
		dragMean.Clear();
		StartCoroutine(StartInertiaTravel(mean * SettingsPanel.Instance.DragSensitivity));
		//SnapToNearest();
	}

	const float framesPrMean = .7f;

	public void StartTravel(float speed)
	{
		StartCoroutine(StartInertiaTravel(speed));
	}

	/// <summary>
	/// This function makes the Imageline travel with a start speed reducing gradually over a given fremcount.
	/// The reduction of speed is matched in such a way, that the imageline will always end up in an aligned position.
	/// </summary>
	/// <returns>The inertia travel.</returns>
	/// <param name="mean">Mean.</param>
	private IEnumerator StartInertiaTravel(float mean)
	{
		//Debug.Log("mean: " + mean);
		if(mean * mean < 5f)
		{
			//Debug.Log("Small step");
			SnapToNearest();
			yield break;
		}

		isInertiaTraveling = true;
		int numFrames = (int) Mathf.Min(30, Mathf.Abs(mean * framesPrMean));

		float totalTravelFirstAssesed = mean * 0.5f * numFrames;

		float firstAssessedOffset = totalTravelFirstAssesed + imageLineTransf.anchoredPosition.x;

		//
		float steps = firstAssessedOffset / Screen.width;

		//If step is between 0.25 and 0.5 multiply it so it rounds to 1. This is to favour non-0 steps
		steps = Mathf.Abs(steps) < 0.51f ? steps * 2f : steps;

		float stepsRounded = Mathf.Round(steps);

		float targetTransport = stepsRounded * Screen.width - imageLineTransf.anchoredPosition.x;

		float startDelta = targetTransport /numFrames * 2f;

		float deduction = startDelta / numFrames;

		float currentDelta = startDelta - deduction * 0.5f;

		float deltaSum = 0;

		for(int i = 0; i < numFrames; i++)
		{
			Vector2 linePos = imageLineTransf.anchoredPosition;
			linePos.x += currentDelta;
			deltaSum += currentDelta;

			imageLineTransf.anchoredPosition = linePos;
			currentDelta -= deduction;
			yield return new WaitForEndOfFrame();

			if(isDragging)
			{
				isInertiaTraveling = false;
				yield break;
			}
		}
		//Debug.LogFormat("Delta sum: {0} predicted: {1} startDelta: {2}", deltaSum, targetTransport, startDelta); 
		//SnapToNearest();
		isInertiaTraveling = false;
	}

	private void SnapToNearest()
	{
		EnsureSnapDead();
		snapSeq = DOTween.Sequence();

		Vector2 linePos = imageLineTransf.anchoredPosition;
		linePos.x = 0;
		Tweener t = imageLineTransf.DOAnchorPos(linePos, .3f);
		snapSeq.Append(t);
		//if(snapSeq !=
		//Debug.Log("End drag");

	}



	private void EnsureSnapDead()
	{
		if(snapSeq != null && !snapSeq.IsComplete())
		{
			snapSeq.Kill(true);
		}
	}

	public void SetMiddleIndex(int index)
	{
		middleIndex = index;
	}

	public void GoToIndex(int index)
	{
		EnsureSnapDead();
		snapSeq = DOTween.Sequence();
		Vector2 linePos = imageLineTransf.anchoredPosition;
		linePos.x = Screen.width * index;
		Tweener t = imageLineTransf.DOAnchorPos(linePos, 3f);
		snapSeq.Append(t);
	}

	//This Function ensures that the position of the line is always near the centre.
	//When swiping images, the line would otherwise move its center accordingly and could end up in a position very far to away.
	//This function moves the centre and exchanges the images accordingly, so it seems you are swiping through an endless row of images,
	//though there are alwas only three images in a single ImageLine.
	public void UpdateLine()
	{
		if(snapSeq != null && !snapSeq.IsComplete())
		{
			//return;
		}

		//Do we step the images left or rigth?
		int stepDirection = 0;

		if(imageLineTransf.anchoredPosition.x < -Screen.width * 0.5f)
		{
			stepDirection = 1;
		}
		else if(imageLineTransf.anchoredPosition.x > Screen.width * 0.5f)
		{
			stepDirection = -1;
		}

		if(stepDirection != 0)
		{
			middleIndex += stepDirection;

			//Debug.Log(string.Format("Step direction: {0} middleIndex: {1}", stepDirection, middleIndex));
			imageLineTransf.anchoredPosition += Vector2.right * stepDirection * Screen.width;
			SetImages();
		}


	}

	public void SetImages()
	{
		updatedCB();
		int imageCount = ImageLoader.Instance.images.Count;

		if(middleIndex < 0)
		{
			middleIndex = imageCount -1;
		}
		else if(middleIndex >= imageCount)
		{
			middleIndex = 0;
		}

		int lowerIndex = middleIndex - 1;
		int higherIndex = middleIndex + 1;


		if(lowerIndex < 0)
		{
			lowerIndex = imageCount - 1;
		}

		if(higherIndex >= imageCount)
		{
			higherIndex = 0;
		}

		leftImg.sprite = ImageLoader.Instance.images[lowerIndex].sprites[level];
		middleImg.sprite = ImageLoader.Instance.images[middleIndex].sprites[level];
		rightImg.sprite = ImageLoader.Instance.images[higherIndex].sprites[level];
	}

	void OnGUI()
	{
		if(showDebug && level == 0)
		{
		GUILayout.Label("Current mean: " + dragMean.GetMean());
		GUILayout.Label("Last Inertia mean: " + lastinertiaMean);
		}
	}
}
