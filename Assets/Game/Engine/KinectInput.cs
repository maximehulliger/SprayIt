using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class KinectInput: MonoBehaviour {
	private const int rightHandId = (int)KinectWrapper.NuiSkeletonPositionIndex.HandRight;
	private const int leftHandId = (int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft;
	private const int rightShoulderId = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight;
	private const int leftShoulderId = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft;
	private const float smoothFactor = 5;

	public int k = 4;
	//public bool kinectInclined = false;
	public float incamInclY = 0;
	private float incamInclYLast = 0;
	public float inclCorrX = 0;
	private float inclCorrXLast = 0;
	public Vector2 leftHueCorr = Vector2.zero;
	private Vector2 leftHueCorrLast = Vector2.zero;


	public static Vector3 leftArm { get; private set; }
	public static Vector3 rightArm { get; private set; }

	private Vector3 rightShoulderPos, rightHandPos, leftShoulderPos, leftHandPos;
	private Quaternion leftCorr, rightCorr;
	private List<Vector3> lastRight = new List<Vector3>(), lastLeft = new List<Vector3>();

	private void setCorrQuat() {
		leftCorr = Quaternion.Euler(0, incamInclY, 0) * Quaternion.Euler(inclCorrX+leftHueCorr.x, leftHueCorr.y, 0);
		rightCorr = Quaternion.Euler(0, incamInclY, 0) * Quaternion.Euler(inclCorrX, 0, 0);
	}

	// Use this for initialization
	void Start () {
		setCorrQuat();
	}
	
	void Update() 
	{
		KinectManager manager = KinectManager.Instance;

		if (inclCorrX != inclCorrXLast || incamInclY != incamInclYLast || leftHueCorr != leftHueCorrLast) {
			inclCorrXLast = inclCorrX;
			incamInclYLast = incamInclY;
			leftHueCorrLast = leftHueCorr;
			setCorrQuat();
		}

		if(manager && manager.IsInitialized() && manager.IsUserDetected())
		{
			// update raw point position
			uint userId = manager.GetPlayer1ID();
			if(manager.IsJointTracked(userId, rightHandId))
				rightHandPos = Vector3.Lerp(rightHandPos, manager.GetRawSkeletonJointPos(userId, rightHandId), smoothFactor * Time.deltaTime);
			if(manager.IsJointTracked(userId, rightShoulderId))
				rightShoulderPos = Vector3.Lerp(rightShoulderPos, manager.GetRawSkeletonJointPos(userId, rightShoulderId), smoothFactor * Time.deltaTime);
			if(manager.IsJointTracked(userId, leftHandId))
				leftHandPos = Vector3.Lerp(leftHandPos, manager.GetRawSkeletonJointPos(userId, leftHandId), smoothFactor * Time.deltaTime);
			if(manager.IsJointTracked(userId, leftShoulderId))
				leftShoulderPos = Vector3.Lerp(leftShoulderPos, manager.GetRawSkeletonJointPos(userId, leftShoulderId), smoothFactor * Time.deltaTime);

			//update arm vector
			Vector3 left = leftHandPos - leftShoulderPos;
			Vector3 right = rightHandPos - rightShoulderPos;
			left.z = -left.z;
			right.z = -right.z;
			left = leftCorr * left;
			right = rightCorr * right;

			leftArm = addAndGetAverage(lastLeft, left, k);
			rightArm = addAndGetAverage(lastRight, right, k);
		}
	}

	Vector3 addAndGetAverage(List<Vector3> list, Vector3 newVec, int maxNum) {
		list.Insert(0, newVec);
		while (list.Count > k)
			list.RemoveAt(maxNum);
		Vector3 av = Vector3.zero;
		foreach(Vector3 v in list)
			av += v;
		return av / list.Count;
	}
}
