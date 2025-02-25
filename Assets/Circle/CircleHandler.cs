using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(LineRenderer))]
public class CircleHandler : NetworkBehaviour
{
	[Range(0, 360)]
	public int Segments;
	[Range(0, 5000)]
	public float XRadius;
	//[Range(0, 5000)]
	public float YRadius;  //THIS IS NOT USED - SHOULD BE ELIMINATED

	[Range(10, 100)]
	public int ZoneRadiusFactor = 50; //default to 50%

	[Header("Shrinking Zones")]
	public List<int> ZoneTimes;

	#region Private Members
	private bool Shrinking;  // this can be set to PUBLIC in order to troubleshoot.  It will show a checkbox in the Inspector
	private int countdownPrecall = 10;  //this MIGHT be public, but it should not need to be changed
	private int timeToShrink = 30; //seconds
	private int count = 0;
	private bool newCenterObtained = false;
	private Vector3 centerPoint = new Vector3(0, -100, 0);
	private float distanceToMoveCenter;
	private WorldCircle circle;
	private LineRenderer _renderer;
	private GameObject ZoneWall;
	private float[] radii = new float[2];
	private float shrinkRadius;
	private int zoneRadiusIndex = 0;
	private int zoneTimesIndex = 0;
	private float timePassed;
	#endregion

	void Start()
	{
		_renderer = gameObject.GetComponent<LineRenderer>();
		radii[0] = XRadius; radii[1] = YRadius;
		circle = new WorldCircle(ref _renderer, Segments, radii);
		ZoneWall = GameObject.FindGameObjectWithTag("ZoneWall");

		timePassed = Time.deltaTime;
	}


	void Update()
	{
		if (!Object.HasStateAuthority) return;
		ZoneWall.transform.localScale = new Vector3((XRadius * 0.01f), 1, (XRadius * 0.01f));

		if (Shrinking)
		{
			// we need a new center point (that is within the bounds of the current zone)
			if (!newCenterObtained)
			{
				centerPoint = NewCenterPoint(transform.position, XRadius, shrinkRadius);
				distanceToMoveCenter = Vector3.Distance(transform.position, centerPoint); //this is used in the Lerp (below)
				newCenterObtained = (centerPoint != new Vector3(0, -100, 0));
			}

			Debug.Log("New Center Point is " + centerPoint);

			// move the center point, over time
			transform.position = Vector3.Lerp(transform.position, centerPoint, (distanceToMoveCenter / timeToShrink) * Time.deltaTime);
			// shrink the zone diameter, over time
			XRadius = Mathf.MoveTowards(XRadius, shrinkRadius, (shrinkRadius / timeToShrink) * Time.deltaTime);
			circle.Draw(Segments, XRadius, XRadius);

			// MoveTowards will continue infinitum, so we must test that we have gotten close enough to be DONE
			if (1 > (XRadius - shrinkRadius))
			{
				timePassed = Time.deltaTime;
				Shrinking = false;
				newCenterObtained = false;
			}
		}
		else
		{
			timePassed += Time.deltaTime; // increment clock time
		}

		// have we passed the next threshold for time delay?
		if (((int)timePassed) > ZoneTimes[zoneTimesIndex])
		{
			shrinkRadius = ShrinkCircle((float)(XRadius * (ZoneRadiusFactor * 0.01)))[1];  //use the ZoneRadiusFactor as a percentage
			Shrinking = true;
			timePassed = Time.deltaTime;  //reset the time so other operations are halted.
			NextZoneTime();
		}

		// COUNT DOWN
		if (timePassed > (ZoneTimes[zoneTimesIndex] - countdownPrecall))
		{  // we need to begin counting down
			if (ZoneTimes[zoneTimesIndex] - (int)timePassed != count)
			{
				count = Mathf.Clamp(ZoneTimes[zoneTimesIndex] - (int)timePassed, 1, 1000);  // this ensures our value never falls below zero

				//FILL IN APPROPRIATE UI CALLS HERE FOR THE COUNTDOWN
				Debug.Log("Shrinking in " + count + " seconds.");
			}
		}
	}

	// ***********************************
	// PRIVATE (helper) FUNCTIONS
	// ***********************************
	private Vector3 NewCenterPoint(Vector3 currentCenter, float currentRadius, float newRadius)
	{
		Vector3 newPoint = Vector3.zero;

		var totalCountDown = 30000; //prevent endless loop which will kill Unity
		var foundSuitable = false;
		while (!foundSuitable)
		{
			totalCountDown--;
			Vector2 randPoint = Random.insideUnitCircle * (currentRadius * 2.0f);
			newPoint = new Vector3(randPoint.x, 0, randPoint.y);
			foundSuitable = (Vector3.Distance(currentCenter, newPoint) < currentRadius);
			if (totalCountDown < 1)
				return new Vector3(0, -100, 0);  //explicitly define an error has occured.  In this case we did not locate a reasonable point
		}
		return newPoint;
	}

	private int NextZoneTime()
	{
		//if we have exceeded the count, just start over
		if (zoneTimesIndex >= ZoneTimes.Count - 1) // Lists are zero-indexed
			zoneTimesIndex = -1;  // the fall-through (below) will increment this

		// next time to wait
		return ZoneTimes[++zoneTimesIndex];
	}

	// This is a general purpose method
	private float[] ShrinkCircle(float amount)
	{
		float newXR = circle.radii[0] - amount;
		float newYR = circle.radii[1] - amount;
		float[] retVal = new float[2];
		retVal[0] = newXR;
		retVal[1] = newYR;
		return retVal;
	}
}
