using UnityEngine;
using System.Collections;

public class MoveArms : MonoBehaviour {

	public Transform right;
	public Transform left;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		right.rotation = Quaternion.FromToRotation(Vector3.up, KinectInput.rightArm);
		left.rotation = Quaternion.FromToRotation(Vector3.up, KinectInput.leftArm);
	}
}
