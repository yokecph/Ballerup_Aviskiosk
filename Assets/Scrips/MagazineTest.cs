using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagazineTest : MonoBehaviour {

	public Image gather1, gather2, gather3;
	public Image split1, split2, split3;
	public RectTransform ownTransf;
	public void Setup(ImagesAndUrl iAU)
	{
		gather1.sprite = iAU.sprites[0];
		split1.sprite = iAU.sprites[0];

		gather2.sprite = iAU.sprites[1];
		split2.sprite = iAU.sprites[1];

		gather3.sprite = iAU.sprites[2];
		split3.sprite = iAU.sprites[2];
	}
}
