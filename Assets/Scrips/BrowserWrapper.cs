using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using System.Threading;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class LoginCredentials
{
	public string username = "none";
	public string password = "none";
}

public class BrowserWrapper : MonoBehaviour {

	private const string loginCredsFileName = "login.json";

	public static LoginCredentials loginCreds;

	public Browser browser;
	private IPromiseTimer promTimer = new PromiseTimer();

	public Button closeBtn;

	public RectTransform closeBtnTransf;

	private System.Action viewDoneCB = null;
		void Awake()
		{
		string loginFilePath = Application.dataPath + Path.DirectorySeparatorChar + loginCredsFileName; 
		if(File.Exists(loginFilePath))
		{
			string loginCredsJSon = File.ReadAllText(loginFilePath);

			LoginCredentials lC = JsonUtility.FromJson<LoginCredentials>(loginCredsJSon);

			loginCreds = lC;
		}
	}

	// Use this for initialization
	void Start () {
		promTimer.Update(Time.deltaTime);

		if(closeBtn != null)
		{
			closeBtn.onClick.AddListener(()=>{
				if(viewDoneCB != null)
					//Run Daniels erase numbers from localstorage to prevent returning to the same page when coming back.
					browser.EvalJS("(new Array(localStorage.length)).fill(0).map((_,i)=>" +
						"localStorage.key(i)).filter(k=>/^\\d+$/.test(k)).forEach(k=>localStorage.removeItem(k));");
					viewDoneCB();
			});
		}

	}
	
	// Update is called once per frame
	void Update () {
		promTimer.Update(Time.deltaTime); 

	}

	public void ForceReturnFromMagazine()
	{
		Debug.Log("Forcing Return");
		viewDoneCB();
	}
	public void LoadMagId(string magId, System.Action<bool> loadingDoneCB, System.Action viewingDoneCB)
	{
		viewDoneCB = viewingDoneCB;
		StartCoroutine(WaitForMagazineLoad(magId, loadingDoneCB));
	}

	private IEnumerator WaitForMagazineLoad(string magId, System.Action<bool> loadDoneCB)
	{
		bool abort = false;
		System.Action<System.Exception, string>  handleErr = (exc, msg) => {
			UnityEngine.Debug.LogWarningFormat("Exception custom msg: {0} exception Msg: {1}", msg, exc.Message);
		};

		//Load the rbDigital page for the relevant magazine
		browser.LoadURL("https://www.rbdigital.com/ddb/service/magazines/landing?mag_id= " + magId, true);

		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();

		while(!browser.IsLoaded)
		{
			//Debug.Log("Browser not loaded");
			yield return new WaitForSeconds(.1f);
			if(timer.ElapsedMilliseconds > 10000)
			{
				Debug.LogWarning("Exiting load loop due to timeout on waiting for initial page-load");
				loadDoneCB(false);
				yield break;
			}
		} 	

		timer.Reset();

		Debug.Log("Browser IS loaded");

		//Determine if you are logged in

		var isLoggedIn = browser.EvalJS("document.getElementById('profile')").Catch((e)=>{
			handleErr(e, "Looking for .proile");
		});

		yield return isLoggedIn.ToWaitFor();

		if(isLoggedIn.Value.IsNull)
		{
			//Perform login
			Debug.Log("Not logged in - logging in");
			string findLoginBlockCMD = @"document.querySelector('.login_block')";

			var loginBtnPresent = WaitUntilReadyProm(findLoginBlockCMD, 40, .25f)
				.Then((res)=> {
					if(res.IsNull)
					{
						throw new System.Exception("Login block never appearing");
					}
					return browser.CallFunction("ToLogin");
				})
				.Then((res)=>{
					return WaitUntilReadyProm("document.getElementById('pl_login')", 40, .1f);
				})
				.Then((res)=> {
					if(res.IsNull)
					{
						throw new System.Exception("pl_login id never appearing");
					}
					return browser.CallFunction("ToLogin");
				})
				.Then(res => WaitUntilReadyProm("document.getElementById('username')", 20, .1f))
				.Then(res => WaitUntilReadyProm("document.getElementById('password')", 20, .1f))
				.Then((res)=> browser.EvalJS(string.Format("document.getElementById('username').value = '{0}'", loginCreds.username)))
				.Then((res)=> browser.EvalJS(string.Format("document.getElementById('password').value = '{0}'", loginCreds.password)))
				.Then((res)=> browser.CallFunction("OnLogIn"))
				.Then(res=> WaitUntilReadyProm("document.getElementById('pl_login')", 40, .1f))
				.Catch(exc=>{
					handleErr(exc, "Logging in");
					abort = true;
				});

			yield return loginBtnPresent.ToWaitFor();

			if(abort)
			{
				loadDoneCB(false);
				yield break;
			}
		}
		else
		{
			Debug.Log("Is Already logged in");
		}




		string getLinkCMD = @"document.querySelector(""a[href*='magazine-reader'"")";
		string getLinkCMD2 = @"document.querySelector(""a[href*='magazine-reader'"").getAttribute('href')";

		var doLoadMagazine = browser.CallFunction("OnCompleteCheckout", magId)
			.Then((res)=>WaitUntilReadyProm(getLinkCMD, 100, .1f))
			.Then((res)=> {
				if(res.IsNull)
				{
					throw new System.Exception("magazine-reader href never appearing");
				}})
			.Then(res=>browser.EvalJS(getLinkCMD2))
			.Then(res=>{
				browser.CallFunction("CloseZinioCheckoutDialog");
				Debug.Log("Got attribute res: " + res.Value);
				browser.LoadURL("www.rbdigital.com" + res.Value.ToString() + "/Cover", false);
			})
			.Catch(exc=>{
				abort = true;
				handleErr(exc, "Loading Magazine");
			});

		yield return doLoadMagazine.ToWaitFor();

		if(abort)
		{
			loadDoneCB(false);
			yield break;
		}

		yield return new WaitForSeconds(1f);

		var waitingForMag = WaitUntilReadyProm("document.getElementById('Magazine') ? {} : null", 20, .5f)
			.Then((res)=>
				{
					if(res.IsNull)
					{
						abort = true;
					}
					Debug.Log("Magazine Loaded! with res state: " + res.Value);
				})
			.Catch((exc)=> {
				Debug.LogWarning("exception: " + exc.Message);
				abort = true;
			});

		yield return waitingForMag.ToWaitFor(false);

		Debug.Log("Done Waiting");

		/*
		IPromise<JSONNode> magazineRect = browser.EvalJS("document.getElementsByClassName('svg-pan-zoom_viewport')[0].getBoundingClientRect()")
			.Then((rect)=>{
				closeBtnTransf.anchoredPosition = new Vector2(rect["right"], rect["top"]);
				Debug.Log("set position to: " + closeBtnTransf.anchoredPosition);
			}).Catch(exc=>Debug.LogWarningFormat("Got exception in getting position: {0}", exc.Message));

		yield return magazineRect.ToWaitFor();
		*/
		if(loadDoneCB != null)
			loadDoneCB(!abort);
	}

	/// <summary>
	/// Returns promise that is resolved when jsSearchString evaluates to a non-null value or rejected if it evaluates to null after n iterations.
	/// </summary>
	/// <returns>The until ready prom.</returns>
	/// <param name="jsSearchString">Js search string.</param>
	/// <param name="iterations">Iterations.</param>
	/// <param name="iterInterval">Iter interval.</param>
	private IPromise<JSONNode> WaitUntilReadyProm(string jsSearchString = "", int iterations = 10, float iterInterval = 0.25f)
	{
		Debug.Log("Looking for js: " + jsSearchString);
		Promise<JSONNode> returnProm = new Promise<JSONNode>();

		IPromise waitProm = Promise.Resolved();

		for(int i = 0; i < iterations; i++)
		{
			int iter = i;
			waitProm =  waitProm.Then(()=> {
				//Debug.Log(string.Format("Going through iter {0} in search {1}", iter, jsSearchString));

				if(returnProm.CurState != PromiseState.Pending)
				{
					//Debug.Log("Return empty promise from 1");
					return Promise.Resolved();
				}
				else
				{
					
					return browser.EvalJS(jsSearchString)
					.Then((res)=>{
							//Debug.LogFormat("Checking js {0} with result {1} in iter {2}", jsSearchString, res.Value, iter);
						if(!res.IsNull)
						{
							Debug.Log(string.Format("{0} is Found after {1} iterations", jsSearchString, iter));
							returnProm.Resolve(res);
						}
						else if(iter== iterations -1)
						{
							Debug.Log(string.Format("NOT Found after {0} iterations", iter));
							returnProm.Resolve(null);
						}

						//Debug.Log("Return empty promise from 2");
						return promTimer.WaitFor(iterInterval);
						})
					.Catch((exc)=>{
						Debug.LogWarningFormat("Sequence exception: {0}", exc.Message);
						returnProm.Reject(new System.Exception("Sequence exprerienced exception: " + exc.Message));
					});
				
				}
			});
		}
		waitProm.Catch((exc)=>Debug.LogWarningFormat("Caught exception in waiting promise: {1}", exc.Message));
		//Debug.Log("Returning prom");
		return returnProm;
	}
}
