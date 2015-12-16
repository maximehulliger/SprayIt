using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PaintingInput))]
public class Painting : MonoBehaviour {
	public enum BrushType { SimpleSpray, SimpleBrush, BlendBrush, BlendSpray };

	public BrushType brush = BrushType.SimpleSpray;
	[Range(0,1)]
	public float sprayProb = 0.5f;
	[Range(0,1)]
	public float blendFactor = 0.5f;
	[Space(15)]
	[Tooltip("brush radius in pixel")]
	public float radius = 9;


	private Texture2D paint = null;
	private int lastCenterX, lastCenterY, centerX, centerY;
	
	// Use this for initialization
	void Start () {
		lastCenterX = lastCenterY = -(int)(2*radius);
	}
	
	// Update is called once per frame
	void Update () {
		paint = PaintingInput.paint;

		if (PaintingInput.isPainting && PaintingInput.cursorOnCanvas) {
			Vector2 paintSize = new Vector2(PaintingInput.paint.width, PaintingInput.paint.height);
			Vector2 centerPixel = Vector2.Scale( PaintingInput.cursorPos, paintSize);
			//Rect paintingRect = new Rect(
			Vector2 brushDiag = new Vector2(radius, radius);
			Vector2 ulPix = centerPixel - brushDiag;	//upper left
			Vector2 brPix = centerPixel + brushDiag;	//bottom right
			centerX = Mathf.RoundToInt(centerPixel.x);
			centerY = Mathf.RoundToInt(centerPixel.y);
			paintIn(
				Mathf.RoundToInt(Mathf.Max(0, ulPix.x)),
				Mathf.RoundToInt(Mathf.Max(0, ulPix.y)),
				Mathf.RoundToInt(Mathf.Min(paintSize.x, brPix.x)),
				Mathf.RoundToInt(Mathf.Min(paintSize.y, brPix.y)));
			lastCenterX = centerX;
			lastCenterY = centerY;
		}
	}

	private void paintIn(int fromX, int fromY, int toX, int toY) {
		for (int x = fromX; x <= toX; x++) {
			for (int y = fromY; y <= toY; y++) {
				int dx = x - centerX;
				int dy = y - centerY;
				int odx = x - lastCenterX;
				int ody = y - lastCenterY;

				///////////////////
				/// painting :D ///
				///////////////////

				if (brush == BrushType.BlendSpray) {
					int sqrDist = sqr(dx) + sqr(dy);
					if (sqrDist <= sqr(radius)) {
						Color baseColor = paint.GetPixel(x,y);
						float relDist = 1 - Mathf.Sqrt(sqrDist) / radius;
						paint.SetPixel(x,y, blend(baseColor, PaintingInput.color, blendFactor * Random.Range(0.5f,1f) * relDist));
					}
				} else if (sqr(dx) + sqr(dy) <= sqr(radius) && sqr(odx) + sqr(ody) > sqr(radius)) {
					switch (brush) {
					case BrushType.SimpleBrush:
						paint.SetPixel (x,y, PaintingInput.color);
						break;
					case BrushType.SimpleSpray:
						if (Random.Range(0,1f) < sprayProb)
							paint.SetPixel (x,y, PaintingInput.color);
						break;
					case BrushType.BlendBrush:
						Color baseColor = paint.GetPixel(x,y);
						paint.SetPixel(x,y, blend(baseColor, PaintingInput.color, blendFactor));
						break;
					default:
						break;
					}
				}
			}
		}
		paint.Apply();
	}

	private int sqr(int i) {
		return i*i;
	}
	private float sqr(float i) {
		return i*i;
	}

	private Color blend(Color baseColor, Color blendColor, float blend) {
		return new Color(
			baseColor.r * (1-blend) + blend * blendColor.r,
			baseColor.g * (1-blend) + blend * blendColor.g,
			baseColor.b * (1-blend) + blend * blendColor.b);
	}
}
