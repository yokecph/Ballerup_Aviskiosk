using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;


public class PuzzleController : MonoBehaviour {

	public List<ImageLine> imageLines;
	bool imagesAreLoaded = false;
	public BrowserWrapper browserWrap;
	public CanvasGroup browserGroup;
	public GameObject browserGroupGO;
	public Image fullOverlay;
	public GameObject fullOverlayGO;
	private bool imagesLoaded = false;
	private Sequence magTransition;
	public MatchOverlay matchOverlay;
	private bool showingMatchOverlay = false;
	private ImagesAndUrl lastMatch = null;
	private bool matchSurpressed = false;
	private bool showingMag = false;
	private float lastTouchTime = 0;
	private Sequence autoDragSequence;
	public float autoDragPeriod = 5f;
	public LoadingBackground loadBckgrnd;
	public GameObject startupBackgroundGO;
	public SwipeIndicator swipeIndicatorPrefab;
	public RectTransform ownTransf;

	public SettingsPanel settingsPanel;


	// Use this for initialization
	void Start () {
		
		startupBackgroundGO.SetActive(true);
		ImageLoader.Instance.LoadImages(ImagesLoaded);

		matchOverlay.goReadBtn.onClick.AddListener(GoToLastMatchingMag);

		matchOverlay.goExploreBtn.onClick.AddListener(()=> {
			matchSurpressed = true;
			ToggleMatchOverlay(false, null);
			ScrambleLines();
		});

	}

	private void ImagesLoaded()
	{
		startupBackgroundGO.SetActive(false);
		foreach(ImageLine iL in imageLines)
		{
			iL.Setup(ImageLoader.Instance.images, ()=>{matchSurpressed = false;});
		}
		imagesLoaded = true;

		ScrambleLines();
	}

	private void ToggleMatchOverlay(bool toActive, ImagesAndUrl match)
	{
		if(match != null)
		{
			matchOverlay.ownGO.SetActive(false);
			matchOverlay.magazineTitelText.text = match.magazineName;
			matchOverlay.magazineTitelTextEng.text = "You have assembled " + match.magazineName;

			matchOverlay.ownGO.SetActive(true);
		}

		showingMatchOverlay = toActive;
		matchOverlay.ownGO.SetActive(true);
		matchOverlay.canvGroup.alpha = toActive ? 0f : 1f;
		Tweener t = matchOverlay.canvGroup.DOFade(toActive ? 1f: 0f, .5f);
		t.OnComplete(()=>{
			matchOverlay.ownGO.SetActive(toActive);
		});

		lastTouchTime = Time.time;
	}

	void ScrambleLines()
	{
		float speed = -300;
		foreach(ImageLine iL in imageLines)
		{
			iL.StartTravel(speed);
			speed += 500;
		}
	}

	void Update()
	{
		if(!imagesAreLoaded && !ImageLoader.Instance.IsLoading)
		{
			Setup();
			imagesAreLoaded = true;
		}

		if(imagesLoaded)
		{
			ImagesAndUrl firstImg = imageLines[0].CurrentSelected;

			bool imageLinesAreSteady = imageLines.Where((iL=>iL.IsMoving)).Count() == 0;

			bool isMatching = !matchSurpressed && firstImg == imageLines[1].CurrentSelected && firstImg == imageLines[2].CurrentSelected && imageLinesAreSteady;


			if(isMatching != showingMatchOverlay)
			{
				lastMatch = firstImg;
				ToggleMatchOverlay(isMatching, firstImg);
			}
		}

		if(Input.GetMouseButton(0))
		{
			lastTouchTime = Time.time;
		}

		if(!showingMag && !showingMatchOverlay && Time.time > lastTouchTime + autoDragPeriod && (autoDragSequence == null || !autoDragSequence.IsActive()))
		{
			autoDragSequence = PerformAutoDrags(1f);
		}
		//return from idle viewing in magazine
		if(showingMag && Time.time - lastTouchTime > 300f)
		{
			lastTouchTime = Time.time;
			browserWrap.ForceReturnFromMagazine();
		}

		if(Input.GetKeyDown(KeyCode.S))
		{
			settingsPanel.onOffGO.SetActive(!settingsPanel.onOffGO.activeSelf);
		}



	}

	private Sequence PerformAutoDrags(float factor)
	{
		bool useThree = Random.value >= 0.5f;

		float[] offsets = useThree ? new float[3] { 200, -300, 200} : new float[2] {200, -200};
		int[] indices = useThree ? new int[3] {0,1,2} : new int[2] {0,2};

		offsets = offsets.Select((f)=>f*factor).ToArray();

		Sequence seq = DOTween.Sequence();


		System.Action<int, float> insertSwipeIndicatorAndDrag = (offsetIndex, insertTime) => {
			int screenIndex = indices[offsetIndex];
			float duration = 3f - insertTime * 0.8f;

			//Setup drag swipe indicator
			SwipeIndicator sI = Object.Instantiate(swipeIndicatorPrefab);
			sI.ownTransf.SetParent(ownTransf);
			sI.ownTransf.anchorMin = Vector2.zero;
			sI.ownTransf.anchorMax = Vector2.zero;

			//float position = seq.Duration();

			seq.Insert(insertTime, sI.DoMoveAndFade(new Vector2(Screen.width * 0.5f, Screen.height * (0.5f - 0.25f * (screenIndex - 1))), offsets[offsetIndex], .7f, .5f, duration));
			seq.Insert(insertTime + 0f, imageLines[screenIndex].SimulateDrag(offsets[offsetIndex], .7f, duration, .4f));
		};

		for(int i = 0; i < offsets.Length; i++)
		{
			insertSwipeIndicatorAndDrag(i, 0.6f * i);
		}

		seq.AppendCallback(()=>lastTouchTime = Time.time);
		return seq;
	}

	private void GoToLastMatchingMag()
	{
		matchSurpressed = true;
		ToggleMatchOverlay(false, null);
		GoToMagazine(lastMatch.magazineId);
	}

	private void GoToMagazine(string magId)
	{
		showingMag = true;
		fullOverlay.color = new Color(0,0,0,0);
		fullOverlayGO.SetActive(true);
		fullOverlay.DOFade(1f, 1f);
		
		System.Action fromMag = ()=> {
			fullOverlayGO.SetActive(true);
			Sequence seq = DOTween.Sequence();
			seq.Append(fullOverlay.DOFade(1f, 1f));
			seq.AppendCallback(()=> {
				browserGroup.interactable = false;
				browserGroup.blocksRaycasts = false;
				browserGroup.alpha = 0;
				ScrambleLines();
			});
			seq.Append(fullOverlay.DOFade(0f, 1f));
			seq.AppendCallback(()=>{
				fullOverlayGO.SetActive(false);
				showingMag = false;

			});
		};

		System.Action<bool> toMag = (success)=> {
			if(success)
			{
				loadBckgrnd.ToggleLoadingImg(false);
				browserGroup.interactable = true;
				browserGroup.blocksRaycasts = true;
				browserGroup.alpha = 1f;
				fullOverlay.DOFade(0f, 1f).OnComplete(()=>{
					fullOverlayGO.SetActive(false);
				});
			}
			else
			{
				Debug.LogWarning("Going back from mag due to load error");
				fromMag();
			}
		};
		loadBckgrnd.ToggleLoadingImg(true);
		browserWrap.LoadMagId(magId, toMag, fromMag);
	}


	private void Setup()
	{
		foreach(ImageLine iL in imageLines)
		{
			iL.SetMiddleIndex(Random.Range(0, ImageLoader.Instance.images.Count));
			iL.SetImages();
			//iL.GoToIndex();
		}
		/*
		topNavWrapper = new HorizontalNavWrapper<ImagesAndUrl>(topNav,
			(iAU, b, i)=>{ClickedWrapper(iAU, i, b, 0);}, ImageLoader.Instance.images, true, null,
			(f,i)=>{SwipeDrag(f,i,0);});
			*/
	}

	/*
	private void SwipeDrag(float f, int i, int level)
	{
		ImageLine iL = imageLines[level];
		Vector2 anchorPos = iL.ownTransf.anchoredPosition;
		anchorPos.x = Screen.width * f;
		Debug.Log("Moving anch pos from " + 
		imageLines[level].ownTransf.anchoredPosition = anchorPos;
//		Debug.Log(string.Format("Swipe drag int {0} float {1}", i, f));
	}
		

	private void ClickedWrapper(ImagesAndUrl iAU, int index, bool clickedRight, int level)
	{
		Debug.Log("Index: " + index);
		//Make sure that the new image is correct
		ImageLine iL = imageLines[level];
		Image newImg = clickedRight ? iL.rightImg : iL.leftImg;
		newImg.sprite = iAU.sprites[level];

		Sequence moveSeq = DOTween.Sequence();

		Vector2 newAnchorPos = iL.ownTransf.anchoredPosition;

		newAnchorPos.x = clickedRight ? - Screen.width : Screen.width;

		Tweener t = iL.ownTransf.DOAnchorPos(newAnchorPos, 1f);

		moveSeq.Append(t);


		//topImage.img.sprite = iAU.sprites[0];
		Debug.Log(string.Format("navigated to: {0} on level {1} left {2}", iAU.url, level, clickedRight ));
	}
*/
	//private Tweener moveOldOut
	
}
