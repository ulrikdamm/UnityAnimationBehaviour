using UnityEngine;

public class PositionAnimation : AnimationBehaviour {
	[Header("Between coordinates")]
	public Space space;
	public Vector3 fromPosition;
	public Vector3 toPosition;
	
	[Header("Between transforms")]
	public Transform fromTransform;
	public Transform toTransform;
	public Transform throughTransform;
	
	[ContextMenu("Set from position")] void setFromPosition() => fromPosition = currentPosition();
	[ContextMenu("Set to position")] void setToPosition() => toPosition = currentPosition();
	
	[ContextMenu("Restore from position")] void restoreFromPosition() => setCurrentPosition(fromPosition);
	[ContextMenu("Restore to position")] void restoreToPosition() => setCurrentPosition(toPosition);
	
	Vector3 currentPosition() {
		switch (space) {
			case Space.Self: return transform.localPosition;
			case Space.World: return transform.position;
			default: throw new System.Exception("Unknown space");
		}
	}
	
	void setCurrentPosition(Vector3 position) {
		switch (space) {
			case Space.Self: transform.localPosition = position; break;
			case Space.World: transform.position = position; break;
			default: throw new System.Exception("Unknown space");
		}
	}
	
	protected override void onAnimationDone() {
		if (toTransform != null) {
			transform.position = (playBack ? fromTransform : toTransform).position;
			return;
		}
		
		setCurrentPosition(playBack ? fromPosition : toPosition);
	}
	
	protected override void onAnimationProgress(float progress) {
		if (throughTransform != null) {
			var ab = Vector3.LerpUnclamped(fromTransform.position, throughTransform.position, progress);
			var bc = Vector3.LerpUnclamped(throughTransform.position, toTransform.position, progress);
			transform.position = Vector3.LerpUnclamped(ab, bc, progress);
			return;
		}
		
		if (toTransform != null) {
			transform.position = Vector3.LerpUnclamped(fromTransform.position, toTransform.position, progress);
			return;
		}
		
		var position = Vector3.LerpUnclamped(fromPosition, toPosition, progress);
		setCurrentPosition(position);
	}
}
