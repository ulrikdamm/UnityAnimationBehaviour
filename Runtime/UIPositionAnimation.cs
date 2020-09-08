using UnityEngine;

public class UIPositionAnimation : AnimationBehaviour {
	public RectTransform rectTransform;
	
	[Space]
	public bool readInitialPositionOnAnimation = true;
	public Vector3 toPosition;
	public Vector3 initialPosition;
	
	[Space]
	public bool animatePivotAndAnchor;
	public Vector2 targetPivot;
	public Vector2 targetAnchorMin;
	public Vector2 targetAnchorMax;
	
	Vector2 initialPivot;
	Vector2 initialAnchorMin;
	Vector2 initialAnchorMax;
	
	[System.Serializable]
	public struct Target {
		public Vector3 position;
		public Vector2 pivot;
		public Vector2 anchorMin;
		public Vector2 anchorMax;
		
		public static Target zero => new Target();
		public static Target center => new Target {
			pivot = new Vector2(0.5f, 0.5f),
			anchorMax = new Vector2(0.5f, 0.5f),
			anchorMin = new Vector2(0.5f, 0.5f)
		};
	}
	
	public void animate(RectTransform to) {
		toPosition = to.anchoredPosition;
		targetPivot = to.pivot;
		targetAnchorMin = to.anchorMin;
		targetAnchorMax = to.anchorMax;
		beginAnimation();
	}
	
	public void animate(Target to) {
		toPosition = to.position;
		targetPivot = to.pivot;
		targetAnchorMin = to.anchorMin;
		targetAnchorMax = to.anchorMax;
		beginAnimation();
	}
	
	public void performImmediately(Target to) {
		toPosition = to.position;
		targetPivot = to.pivot;
		targetAnchorMin = to.anchorMin;
		targetAnchorMax = to.anchorMax;
		onAnimationDone();
	}
	
	protected override void onAnimationBegin() {
		if (readInitialPositionOnAnimation) { initialPosition = rectTransform.anchoredPosition; }
		initialPivot = rectTransform.pivot;
		initialAnchorMin = rectTransform.anchorMin;
		initialAnchorMax = rectTransform.anchorMax;
	}
	
	protected override void onAnimationDone() {
		rectTransform.anchoredPosition = (playBack ? initialPosition : toPosition);
		
		if (animatePivotAndAnchor) {
			rectTransform.pivot = targetPivot;
			rectTransform.anchorMin = targetAnchorMin;
			rectTransform.anchorMax = targetAnchorMax;
		}
	}
	
	protected override void onAnimationProgress(float progress) {
		rectTransform.anchoredPosition = Vector3.LerpUnclamped(initialPosition, toPosition, progress);
		
		if (animatePivotAndAnchor) {
			rectTransform.pivot = Vector2.LerpUnclamped(initialPivot, targetPivot, progress);
			rectTransform.anchorMin = Vector2.LerpUnclamped(initialAnchorMin, targetAnchorMin, progress);
			rectTransform.anchorMax = Vector2.LerpUnclamped(initialAnchorMax, targetAnchorMax, progress);
		}
	}
}
