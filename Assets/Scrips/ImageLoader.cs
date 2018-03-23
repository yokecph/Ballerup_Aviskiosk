using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class ImagesAndUrl
{
	public Sprite[] sprites;
	public string magazineId;
	public string magazineName;
}

public class ImageLoader : MonoBehaviourSingleton<ImageLoader> {

	private static string imgPath = null;

	private static int middleSectionHeight = 500;
	public List<ImagesAndUrl> images = new List<ImagesAndUrl>();

	private bool isLoading = false;
	public bool IsLoading { get { return isLoading; } }

	void Awake()
	{
		imgPath = Application.dataPath + Path.DirectorySeparatorChar + "MagazineCovers";
	}

	void Start()
	{
		//LoadImages();
	}

	public void LoadImages(System.Action doneCB)
	{
		if(!isLoading)
		{
			StartCoroutine(LoadImagesCorout(doneCB));
		}
	}

	private void AddImage(ImagesAndUrl iAU)
	{
		images.Add(iAU);
	}
	private IEnumerator LoadImagesCorout(System.Action doneCB)
	{
		isLoading = true;

		string[] files = Directory.GetFiles(imgPath).Where(f=>!f.EndsWith(".meta")).ToArray();

		int fileCount = files.Length;
		int iter = 0;
		foreach(string f in files)
		{
			iter += 1;
			yield return StartCoroutine(ParseFilePathToSprites(f, AddImage));
			Debug.Log(string.Format("successfully loadedd and parsed: {0} {1}/{2}", Path.GetFileName(f), iter, fileCount));
		}
		isLoading = false;

		if(doneCB != null)
			doneCB();

		yield  break;
	}

	private IEnumerator ParseFilePathToSprites(string filePath, System.Action<ImagesAndUrl> doneCB)
	{
		string wwwUrl = "file://" + filePath;
		WWW w = new WWW(wwwUrl);

		yield return w;
		if(!string.IsNullOrEmpty(w.error))
		{
			Debug.LogWarning("Error loading from path: " + wwwUrl);
			yield break;
		}

		var tempTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);

		w.LoadImageIntoTexture(tempTexture);

		int thisMiddleSectionHeight = middleSectionHeight * tempTexture.width / 1080;


		int section1 = (tempTexture.height - thisMiddleSectionHeight) / 2;
		int section2 = section1 + thisMiddleSectionHeight;


		Sprite sBottom = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, section1), new Vector2(0.5f, 0.5f));
		Sprite sMiddle = Sprite.Create(tempTexture, new Rect(0, section1, tempTexture.width, thisMiddleSectionHeight), new Vector2(0.5f, 0.5f));
		Sprite sTop = Sprite.Create(tempTexture, new Rect(0, section2, tempTexture.width, tempTexture.height - section2), new Vector2(0.5f, 0.5f));

		Debug.Log(string.Format("Cropping middle to: ({0},{1}", tempTexture.width, thisMiddleSectionHeight));
		ImagesAndUrl iAU = new ImagesAndUrl();
		iAU.sprites = new Sprite[3]{sTop, sMiddle, sBottom};

		string fileName = Path.GetFileName(filePath);

		int indexOf_ = fileName.IndexOf("_");
		iAU.magazineName = fileName.Substring(0, indexOf_);
		iAU.magazineId = fileName.Substring(indexOf_ + 1, fileName.Length - indexOf_ - 1);

		doneCB(iAU);
	}

}
