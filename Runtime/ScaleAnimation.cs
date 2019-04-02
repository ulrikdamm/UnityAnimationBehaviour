using UnityEngine;
using System.Collections;

public class ScaleAnimation : AnimationBehaviour {
	public Vector3 fromValue = Vector3.zero;
	public Vector3 toValue = Vector3.one;
	
	public void perform(Vector3 fromValue, Vector3 toValue) {
		this.fromValue = fromValue;
		this.toValue = toValue;
		beginAnimation();
	}
	
	public IEnumerator performRoutine(Vector3 fromValue, Vector3 toValue) {
		this.fromValue = fromValue;
		this.toValue = toValue;
		yield return animationRoutine();
	}
	
	protected override void onAnimationDone() {
		transform.localScale = (playBack ? fromValue : toValue);
	}
	
	protected override void onAnimationProgress(float progress) {
		transform.localScale = Vector3.LerpUnclamped(fromValue, toValue, progress);
	}
}
