using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent( typeof(Canvas) )]
public class AdaptUISize : MonoBehaviour {

	[SerializeField]
	Rect initialRect;
	[SerializeField]
	Rect oldRect;
	[SerializeField]
	RectTransform rTransform;

	[Tooltip("set the default screen size")]
	public bool reset = false;
	
	void Start() {
		Update();
	}
	
	// Update is called once per frame
	void Update () {
		init ();
		apply ();
	}
	
	private void init() {
		if (!rTransform || reset) {
			rTransform = GetComponent<RectTransform>();
			initialRect = oldRect = rTransform.rect;
		}
	}
	
	private void apply() {
		if (!rTransform.rect.Equals(oldRect) || reset) {
			reset = false;
			oldRect = rTransform.rect;
			Vector2 t = Vector2.Scale(oldRect.size, new Vector2(1f/initialRect.width, 1f/initialRect.height));
			
			for (int i=0; i<transform.childCount; i++) {
				Transform child = transform.GetChild(i);
				child.localScale = t;
			}
			
		}
	}
}