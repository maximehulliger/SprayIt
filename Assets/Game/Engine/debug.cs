using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class debug : MonoBehaviour {
	public RawImage debugImg;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (debugImg.enabled)
			debugImg.texture = KinectManager.Instance.GetUsersClrTex();
	}
}
