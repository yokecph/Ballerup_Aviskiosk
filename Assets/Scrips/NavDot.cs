using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class NavDot : MonoBehaviour {

	public Graphic activeGraphic;
	public Graphic inactiveGraphic;
	public RectTransform ownTransf;

	public void Enable()
	{
		Toggle(activeGraphic, inactiveGraphic);
	}

	public void Disable()
	{
		Toggle(inactiveGraphic, activeGraphic);
	}

	private void Toggle(Graphic inGraphic, Graphic outGraphic)
	{
		inGraphic.DOFade(1f, 1f);
		outGraphic.DOFade(0f, 1f);
	}
}
