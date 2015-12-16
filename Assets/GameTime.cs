using UnityEngine;
using System.Collections;

public class GameTime : MonoBehaviour {
	public Transform[] sun;
	private Sun[] _sunScript;

	public float period ;

	public Material _nightMat;
	public Material _night2morningMat;
	public Material _morningMat;
	public Material _morning2noonMat;
	public Material _noonMat;
	public Material _noon2eveningMat;
	public Material _eveningMat;
	public Material _evening2nightMat;


	public float DayCycleInMinuites= 5;
	private float _dayCycleInSeconds;


	private const float SECOND = 1;
	private const float MINUITE = 60 * SECOND;
	private const float HOUR = 60 * MINUITE;
	private const float DAY = 24 * HOUR;
	private float temp;
	private float delta;
	private Quaternion sunInitialRot;


	private const float DEGREES_PER_SECOND = 360 / DAY;

	private float _degreeRotation;
	private float _timeinEachSkybox;
	private float _skyboxCycle;


	// Use this for initialization
	void Start () {

		sunInitialRot = sun[0].transform.rotation;

		//_dayCycleInSeconds = DayCycleInMinuites * 60;
		_degreeRotation = 360/(16*period);

		//RenderSettings.skybox = _sunRiseMat;
		//RenderSettings.skybox.SetFloat ("_Blend", 0);
		_timeinEachSkybox = 0f;
		temp = 0;



	
	}

	float factor = 1;
	
	// Update is called once per frame
	void Update () {

		float dt = Time.deltaTime * factor;
		_timeinEachSkybox += dt;
		sun [0].Rotate (new Vector3(_degreeRotation * dt, 0, 0));

	    if (_timeinEachSkybox < period) {
			RenderSettings.skybox = _night2morningMat ;
			_skyboxCycle= 0;
			factor = 1;
		} else if ((period < _timeinEachSkybox) && (_timeinEachSkybox < 2*period)) {
			RenderSettings.skybox = _morningMat;
			_skyboxCycle= 1;

		} else if ((2*period < _timeinEachSkybox) && (_timeinEachSkybox < 3*period)) {
			RenderSettings.skybox = _morning2noonMat;
			_skyboxCycle= 2;

		} else if ((3*period < _timeinEachSkybox) && (_timeinEachSkybox < 4*period)) {
			RenderSettings.skybox = _noonMat;
			_skyboxCycle= 3;

		} else if ((4*period < _timeinEachSkybox) && (_timeinEachSkybox < 5*period)) {
			RenderSettings.skybox = _noon2eveningMat;
			_skyboxCycle= 4;
			
		} else if ((5*period < _timeinEachSkybox) && (_timeinEachSkybox < 6*period)) {
			RenderSettings.skybox = _eveningMat;
			_skyboxCycle= 5;
			factor = 0.5f;

		} else if ((6*period < _timeinEachSkybox) && (_timeinEachSkybox < 7*period)) {
			RenderSettings.skybox = _evening2nightMat;
			_skyboxCycle= 6;
			factor = 0.5f;
			
		} else if ((7*period < _timeinEachSkybox) && (_timeinEachSkybox < 16*period)) {
			RenderSettings.skybox = _nightMat; 
			_skyboxCycle= 7; 

			factor = 20;
			//RenderSettings.fog = false;
			//RenderSettings.fogColor = new Color(1,1,1);
		}



		if (_timeinEachSkybox > 16 * period) {
			_timeinEachSkybox = 0;
			sun [0].transform.rotation = sunInitialRot;
		}
		BlendSkybox ();



	}

	private void BlendSkybox(){

		temp = (_timeinEachSkybox - (_skyboxCycle * period))/period;
	
		RenderSettings.skybox.SetFloat ("_Blend", temp);
	
	}
}