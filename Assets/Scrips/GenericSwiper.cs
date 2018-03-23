using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GenericSwiper : MonoBehaviour {

	private Vector3 startDragPos;
	private Vector3 lastMousePos;

	System.Action<Vector2> dragCB = null;
	System.Action dragEndCB = null;

	private bool isDragging = false;
	private bool draggedSinceLastFrame = false;
	// Use this for initialization
	void Start () {
		// delegate events
		EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
		// pointer up
		addEventTriggerListener(eventTrigger, EventTriggerType.Drag,
			(BaseEventData data) => {
				PointerEventData pED = data as PointerEventData;

				if(pED != null && dragCB != null)
				{
					draggedSinceLastFrame = true;
					isDragging = true;
					dragCB(pED.delta);
				}
			}
		);

		addEventTriggerListener(eventTrigger, EventTriggerType.EndDrag,
			(BaseEventData data) => {
				PointerEventData pED = data as PointerEventData;

				if(pED != null && dragEndCB != null)
				{
					isDragging = false;
					dragEndCB();
				}
			}
		);
	}

	public void Setup(System.Action<Vector2> dragCallback, System.Action dragEndCallback)
	{
		dragEndCB = dragEndCallback;
		dragCB = dragCallback;
	}
	
	// Update is called once per frame
	void Update () {
		if(isDragging && !draggedSinceLastFrame)
		{
			dragCB(Vector2.zero);
		}

		draggedSinceLastFrame = false;
	}

	private void addEventTriggerListener(EventTrigger trigger, EventTriggerType eventType,
		System.Action<BaseEventData> method) {
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = eventType;
		entry.callback = new EventTrigger.TriggerEvent();
		entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(method));
		trigger.triggers.Add(entry);
	}
}
