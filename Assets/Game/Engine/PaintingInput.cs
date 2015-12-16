using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent (typeof(KinectListener))]
/// get painting parameters from kinect input
public class PaintingInput : MonoBehaviour {
	private static Vector3 reference = new Vector3(0,0,1);
	private static Vector3 hueBasicDir = new Vector3(1,0,0);
	
	public Canvas canvas;
	public RawImage canvasCursor;
	public RawImage painting;
	public RawImage hueWheel;
	public RawImage hueCursor;
	public RawImage brightnessBar;
	public RawImage brightnessCursor;
	public Texture2D startImg;
	public bool takingFromComputer = false;
	public Color colorIn = Color.black;


	public float maxRadius = 0.5f;
	public Vector2 distRange = new Vector2(0.3f, 0.8f);

	/// true if the user is painting
	public static bool isPainting { get; private set; }
	/// true if the user aim on the canvas (-> cursorPos valid).
	public static bool cursorOnCanvas { get; private set; }
	/// cursor position on the image, between (0,0) and (1,1).
	public static Vector2 cursorPos { get; private set; }
	/// the color choosed to paint
	public static Color color { get; private set; }
	/// the texture we are painting on
	public static Texture2D paint { get; private set; }

	private Vector3 canvasBase, canvasX, canvasY;
	private float canvasXsize, canvasYsize;
	private Vector2 cursorHalfSize;
	private float marginPourCentX, marginPourCentY;


	void Start () {
		initCursor();
		color = Color.black;
		paint = (Texture2D)painting.texture;
		reference = Camera.main.transform.rotation * reference;
		hueBasicDir = Camera.main.transform.rotation * hueBasicDir;
		reloadImage();
	}
	
	void Update () {
		updateCursor();
		if (takingFromComputer)
			color = colorIn;
		else
			updateColor();

		isPainting = KinectListener.isPainting || (Input.GetMouseButton(0));// || Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.PageUp));

		if (Input.GetKeyDown(KeyCode.R) ||  Input.GetKeyDown(KeyCode.Space))
			reloadImage();
	}


	private void updateColor() {
		// compute dist, radius and angle
		Vector3 vec = KinectListener.leftArm;
		float dist = Vector3.Dot( vec, reference );
		Vector3 hueSecondDir = Vector3.Cross( reference, hueBasicDir );
		Vector2 onHuePlan = new Vector2(Vector3.Dot( vec, hueBasicDir ), Vector3.Dot( vec, hueSecondDir ));
		float radius = onHuePlan.magnitude;
		float angle = 0;
		if (radius != 0) {
			onHuePlan.Normalize();
			angle = Mathf.Acos(onHuePlan.x);
			if (onHuePlan.y < 0) {
				angle = -angle + 2*Mathf.PI;
			}
		}

		// compute hue, saturation, brightness => color;
		float hue = map01(angle, 0, 2*Mathf.PI, true);
		float saturation = map01(radius, 0, maxRadius, true);
		float brightness = map01(dist, distRange.x, distRange.y, true);
		color = HSVtoRGB.ToColor(hue, saturation, brightness);
		Color brightColor = HSVtoRGB.ToColor(hue, saturation, 1);

		// update hue pointer position
		onHuePlan *= saturation;
		Vector2 hueHalfSize = hueWheel.rectTransform.rect.size / 2;
		Vector2 huePointerPos = Vector2.Scale( onHuePlan, hueHalfSize);
		hueCursor.rectTransform.localPosition = huePointerPos;

		// brightness pointer position
		Vector3 cursorPos = brightnessCursor.rectTransform.localPosition;
		cursorPos.x = map (brightness, 0, 1, -hueHalfSize.x, hueHalfSize.x, true);
		brightnessCursor.rectTransform.localPosition = cursorPos;

		// update colors
		brightnessBar.color = brightColor;
		brightnessCursor.color = color;
	}

	// get canvas axis & lenght
	private void initCursor() {
		// canvas world values
		RectTransform rt = canvas.GetComponent<RectTransform>();
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		canvasBase = corners[0];
		canvasX = corners[3] - corners[0];
		canvasY = corners[1] - corners[0];
		canvasXsize = canvasX.magnitude;
		canvasYsize = canvasY.magnitude;
		canvasX.Normalize();
		canvasY.Normalize();

		// get relative cursor / pointer size ratio
		Rect canRect = rt.rect;
		Rect cursRect = canvasCursor.rectTransform.rect;
		marginPourCentX = cursRect.width / canRect.width * 0.5f;
		marginPourCentY = cursRect.height / canRect.height * 0.5f;
	}

	// set the cursor at the drawing pos from kinect input
	private void updateCursor() {
		//find collision pos on the canvas
		RaycastHit hit;
		if (!(takingFromComputer && Physics.Raycast(Camera.main.ScreenPointToRay( Input.mousePosition ), out hit, 10) ||
		      !takingFromComputer && Physics.Raycast(Camera.main.transform.position, KinectListener.rightArm, out hit, 10))) {
			cursorOnCanvas = false;
			canvasCursor.enabled = false;
		} else {
			canvasCursor.enabled = true;
			Vector2 cursorPos;
			cursorOnCanvas = projectOnCanvasPlan(hit.point, out cursorPos);
			if (cursorOnCanvas) {
				canvasCursor.color = Color.white;
			} else {
				canvasCursor.color = Color.gray;
			}
			Rect canvasRect = ((RectTransform)canvas.transform).rect;
			canvasCursor.rectTransform.localPosition = new Vector3(
				map (cursorPos.x, -marginPourCentX, 1+marginPourCentX, -canvasRect.width*(marginPourCentX+1)*0.5f, canvasRect.width*(marginPourCentX+1)*0.5f, true),
				map (cursorPos.y, -marginPourCentY, 1+marginPourCentX, -canvasRect.height*(marginPourCentY+1)*0.5f, canvasRect.height*(marginPourCentX+1)*0.5f, true));
			PaintingInput.cursorPos = cursorPos;
		}
	}

	private bool projectOnCanvasPlan(Vector3 point, out Vector2 canvasPos) {
		Vector3 vec = point - canvasBase;
		float xFactor = Vector3.Dot( vec, canvasX );
		float yFactor = Vector3.Dot( vec, canvasY );
		canvasPos = new Vector2(xFactor / canvasXsize, yFactor / canvasYsize);
		return isAPeuPresClamped0(xFactor, canvasXsize, marginPourCentX) && isAPeuPresClamped0(yFactor, canvasYsize, marginPourCentY);
	}

	private bool isAPeuPresClamped0(float val, float max, float margin) {
		return val > -margin && val < (1+margin/2)*max;
	}

	private float map01(float v, float min, float max, bool clamp) {
		if (clamp)
			v = Mathf.Clamp(v, min, max);
		return (v - min) / (max - min);
	}

	private float map(float c, float min, float max, float toMin, float toMax, bool clamp) {
		return map01(c, min, max, clamp) * (toMax - toMin) + toMin;
	}

	private void reloadImage() {
		//string filePath = Application.dataPath + "/Game/Engine/Img/painting.png";
		//paint.LoadImage(System.IO.File.ReadAllBytes(filePath));
		//string filePath2 = Application.dataPath + "sprayIt1_Data/painting.png";
		Resources.UnloadAsset(startImg);
		startImg = (Resources.Load("painting") as Texture2D);
		painting.texture = startImg;
		painting.enabled = false;
		painting.enabled = true;
		//paint.LoadImage();
	}
}
