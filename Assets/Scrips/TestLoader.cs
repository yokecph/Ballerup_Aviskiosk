using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoader : MonoBehaviour {

	public ImageLoader loader;
	public RectTransform testMagazineParent;
	public MagazineTest testPrefab;

	private bool testIsSetup = false;
	// Use this for initialization
	void Start () {

		loader.LoadImages(null);
	}
	
	// Update is called once per frame
	void Update () {
		if(!testIsSetup && !loader.IsLoading)
		{
			foreach(ImagesAndUrl iAU in loader.images)
			{
				MagazineTest mT = Object.Instantiate(testPrefab);
				mT.ownTransf.SetParent(testMagazineParent);
				mT.Setup(iAU);
			}
			testIsSetup = true;
		}
	}
}
