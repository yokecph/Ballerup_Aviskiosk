using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HorizontalNavWrapper<T> 
{
	public int showingIndex = 0;

	public HorizontalNav instantiatedHorNav;

	protected T ShowingContent { get { return showingIndex < navContent.Count ? navContent[showingIndex] : default(T); } }

	private System.Action<T, bool, int> onNewSelected;
	private System.Action<T, int> onSwipeEnter = null;
	private System.Action<float, int> onSwipeDrag = null;
	protected List<T> navContent = new List<T>();
	public List<NavDot> instantiatedNavDots = new List<NavDot>();

	private float lastInteractionTime = 0;

	private bool cyclic;
	public HorizontalNavWrapper(HorizontalNav horNav, System.Action<T, bool, int> onNewSelectedCB, List<T> content, bool isCyclic = true, System.Action<T, int> swipeEnter = null, System.Action<float, int> swipeDrag = null)
	{	
		cyclic = isCyclic;
		onNewSelected = onNewSelectedCB;
		instantiatedHorNav = horNav;
		onSwipeEnter = swipeEnter;
		onSwipeDrag = swipeDrag;
		instantiatedHorNav.Setup(ClickedLeft, ClickedRight, SwipeEnter, SwipeDrag);
		navContent = content;

		if(horNav.navDotPrefab != null && horNav.navDotsParent != null)
		{
			List<GameObject> gOsToDestroy = new List<GameObject>();

			foreach(Transform t in horNav.navDotsParent)
			{
				gOsToDestroy.Add(t.gameObject);
			}

			for(int i = 0; i < gOsToDestroy.Count; i++)
			{
				Object.Destroy(gOsToDestroy[i]);
			}

			instantiatedNavDots.Clear();
			
			for(int i = 0; i< navContent.Count; i++)
			{
				NavDot nD = Object.Instantiate(horNav.navDotPrefab, horNav.navDotsParent);
				instantiatedNavDots.Add(nD);
				nD.ownTransf.SetParent(horNav.navDotsParent);
			}
		}


		UpdateHorNav();

		instantiatedHorNav.SetUpdateCallback(this.Update);
	}

	private void Update()
	{
		bool navDotsShouldBeVisible = Time.time - lastInteractionTime < 2f;

		if(navDotsShouldBeVisible != instantiatedHorNav.NavDotsOn)
		{
			instantiatedHorNav.ShowOrHideDots(navDotsShouldBeVisible);
			//Debug.Log("Nav DOts visible: " + navDotsShouldBeVisible);
		}
	}
		
	protected void UpdateHorNav()
	{
		if(instantiatedHorNav != null)
		{
			instantiatedHorNav.SetBtnActive(cyclic || showingIndex > 0, cyclic || showingIndex < navContent.Count - 1);
			instantiatedHorNav.UpdateVisibility();

			for(int i = 0; i < instantiatedNavDots.Count; i++)
			{
				if(i == showingIndex)
					instantiatedNavDots[i].Enable();
				else
					instantiatedNavDots[i].Disable();
			}
		}
	}

	public void ForceShowIndex(int index)
	{
		int oldShowingIndex = showingIndex;
		if(index != oldShowingIndex)
		{
			showingIndex = index;
			UpdateHorNav();
			onNewSelected(ShowingContent, index > oldShowingIndex, showingIndex);
		}
	}

	private void ClickedLeft()
	{
		lastInteractionTime = Time.time;
		//Debug.Log("Clicked left");
		showingIndex -= 1;
		if(cyclic)
		{
			showingIndex = (showingIndex + navContent.Count) % navContent.Count;
		}

		UpdateHorNav();
		onNewSelected(ShowingContent, false, showingIndex);
		//FadeoutUpdateContentFadeIn();	
		//UpdateHorNav();
		//ToggleVisible(contentList[showingIndex], true);
	}

	private void ClickedRight()
	{
		lastInteractionTime = Time.time;
		//Debug.Log("Clicked right");
		showingIndex += 1;
		if(cyclic)
		{
			showingIndex = (showingIndex + navContent.Count) % navContent.Count;
		}

		UpdateHorNav();
		onNewSelected(ShowingContent, true, showingIndex);
		//FadeoutUpdateContentFadeIn();
		//UpdateHorNav();
		//ToggleVisible(contentList[showingIndex], true);
	}

	private void SwipeDrag(float offset, bool touchActive)
	{
		lastInteractionTime = Time.time;
		if(!touchActive)
		{
			if(Mathf.Abs(offset) > 0.2f)
			{
				if(offset < 0)
				{
					ClickedRight();
				}
				else
				{
					ClickedLeft();
				}
				/*
				int newIndex = (showingIndex + (offset < 0 ? 1 : -1)  + navContent.Count) % navContent.Count;
				onSwipeDrag(offset, newIndex);
				*/
			}
			else
			{
				onSwipeDrag(0, showingIndex);
			}
		}
		else
		{
			onSwipeDrag(Mathf.Abs(offset) > 0.2f ? offset * 1.5f : offset  , -1);
		}
	}

	private void SwipeEnter()
	{
		if(onSwipeEnter != null)
		{
			onSwipeEnter(navContent[showingIndex], showingIndex);
		}
	}

	void OnDestroy()
	{
		Debug.Log("Destroying hor nav");
	}

}

public class FadeWrapper
{
	private Tweener fadeTween;

	private bool isUp;
	public bool IsUp{get { return isUp; } }

	public FadeWrapper(Tweener t, bool fadeUp, float time)
	{
		fadeTween = t;

		isUp = fadeUp;
	}

	public void EnsureComplete()
	{
		if(!fadeTween.IsComplete())
		{
			fadeTween.Complete();
		}
	}

	public void EnsureCompleteOrKill(bool doComplete)
	{
		if(!fadeTween.IsComplete())
		{
			fadeTween.Kill(doComplete);
		}
	}
}