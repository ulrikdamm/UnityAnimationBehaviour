using UnityEngine;

public class UIRotateAnimation : AnimationBehaviour {
	[System.Serializable] public enum Axis { x, y, z };
	
	[Header("Input")]
	public Axis axis = Axis.z;
	public float fromAngle = 0;
	public float toAngle = 180;
	
	Quaternion fromQuart;
	Quaternion toQuart;
	Vector3 angleAxis;
	
	protected override void onAnimationBegin() {
		var from = new Vector3();
		var to = new Vector3();
		
		switch (axis) {
			case Axis.x: from.x = fromAngle; to.x = toAngle; angleAxis = new Vector3(1, 0, 0); break;
			case Axis.y: from.y = fromAngle; to.y = toAngle; angleAxis = new Vector3(0, 1, 0); break;
			case Axis.z: from.z = fromAngle; to.z = toAngle; angleAxis = new Vector3(0, 0, 1); break;
		}
		
		fromQuart = Quaternion.Euler(from);
		toQuart = Quaternion.Euler(to);
	}
	
	protected override void onAnimationDone() {
		transform.rotation = toQuart;
	}
	
	protected override void onAnimationProgress(float progress) {
		transform.rotation = Quaternion.AngleAxis(Mathf.LerpUnclamped(fromAngle, toAngle, progress), angleAxis);
		// transform.rotation = Quaternion.SlerpUnclamped(fromQuart, toQuart, progress);
	}
}
