using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Swiper : MonoBehaviour {

	private bool isHorizontal;
	private System.Action clickedCB = null;
	private System.Action<float, bool> dragCB;
	public RectTransform ownTransf;
	public GameObject ownGO;
	private bool isDragging = false;
	private bool mouseIsDown = false;
	private Vector3 startDragPos;
	bool isRelative = true;

	// Use this for initialization
	void Start () {
		// delegate events
		EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
		// pointer up
		addEventTriggerListener(eventTrigger, EventTriggerType.BeginDrag,
			(BaseEventData data) => {
				Debug.Log("Begin Drag");
				if(clickedCB != null)
				{
					startDragPos = Input.mousePosition;
					mouseIsDown = true;
					isDragging = false;
					//clickedCB();
				}
				/*ExecuteEvents.Execute(clickReceiver, data,
				ExecuteEvents.pointerDownHandler);*/
				//Debug.Log("Down");
			}
		);


		// pointer down
		addEventTriggerListener(eventTrigger, EventTriggerType.PointerUp,
			(BaseEventData data) => {
				//Debug.Log("Up");
				if(clickedCB != null)
				{
					if(!isDragging)
					{
						clickedCB();
					}
					else
					{
						float offset = isHorizontal ? (Input.mousePosition.x - startDragPos.x) / ownTransf.rect.width : (Input.mousePosition.y - startDragPos.y) / ownTransf.rect.height;
						dragCB(offset, false);
					}

					mouseIsDown = false;
					isDragging = true;
				}
				/*ExecuteEvents.Execute(clickReceiver, data,
				ExecuteEvents.pointerUpHandler); Debug.Log("Up");}*/

			}
		);
		/*
		// pointer click
		addEventTriggerListener(eventTrigger, EventTriggerType.PointerClick,
			(BaseEventData data) => { ExecuteEvents.Execute(gameObject, data,
				ExecuteEvents.pointerClickHandler); });
		*/
	}
	public void Setup(System.Action clickedCallback, System.Action<float, bool> dragCallback, bool horizontal = true, bool relative = true)
	{
		isHorizontal = horizontal;
		clickedCB = clickedCallback;
		dragCB = dragCallback;
	}

	// Update is called once per frame
	void Update () {

		if(mouseIsDown)
		{
			if(!isDragging)
			{
				if(Vector3.Distance(Input.mousePosition, startDragPos) > 50)
				{
					isDragging = true;
				}
			}
			if(isDragging)
			{
				float offset = (Input.mousePosition.x - startDragPos.x) / ownTransf.rect.width;

				//offset = Mathf.Abs(offset) > 0.2f ? Mathf.Pow(Mathf.Abs(offset) * (offset > 0 ? 1f: -1f), .5f) : offset;
				//Debug.Log("Offset: " + offset + "width: " + ownTransf.rect.width + " delta: " + (Input.mousePosition.x - startDragPos.x));
				dragCB(offset, true);
				//Debug.Log("Drag pos: " + (Input.mousePosition - startDragPos));
			}
		}
	}

	void addEventTriggerListener(EventTrigger trigger, EventTriggerType eventType,
		System.Action<BaseEventData> method) {
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = eventType;
		entry.callback = new EventTrigger.TriggerEvent();
		entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(method));
		trigger.triggers.Add(entry);
	}
}
