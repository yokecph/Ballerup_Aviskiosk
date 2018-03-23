using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// Class holding a fixed size queue of elements. Call T GetMean() to get the mean-value of elements in the queue.
/// </summary>
public class RunningMeaner<T>
{
	private T[] vals = null;
	private int currentIndex = 0;

	private bool isFull = false;
	public bool IsFull{get{return isFull; } }

	private int capacity = 0;
	private bool startSlow;
	System.Func<T,T,T> aggregateFunc = null;
	System.Func<T,int,T> divisionFunc = null;

	public RunningMeaner(int cap, System.Func<T,T,T> aggFunc, System.Func<T,int,T> divFunc, bool startSlow = true)
	{
		capacity = cap;
		vals = new T[capacity];
		aggregateFunc = aggFunc;
		divisionFunc = divFunc;
		this.startSlow = startSlow;
	}

	public void Clear()
	{
		isFull = false;
		currentIndex = 0;
		for(int i = 0; i < capacity; i++)
		{
			vals[i] = default(T);
		}
	}

	public T Push(T elem)
	{
		T oldElem = vals[currentIndex];

		vals[currentIndex++] = elem;
		if(currentIndex >= capacity)
		{
			currentIndex = 0;
			isFull = true;
		}

		return oldElem;
	}

	public T GetMean()
	{
		T[] meanVals;
		if(IsFull || startSlow)
		{
			meanVals = vals;
		}
		else
		{
			meanVals = new T[currentIndex];
			for(int i = 0; i < currentIndex; i++)
			{
				meanVals[i] = vals[i];
			}
		}
		return currentIndex == 0 ? default(T) : divisionFunc(meanVals.Aggregate(aggregateFunc), IsFull ? capacity : currentIndex);
	}
}