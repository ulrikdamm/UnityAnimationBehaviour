using UnityEngine;

public class UIRotateAnimation : AnimationBehaviour {
	[System.Serializable] public enum Axis { x, y, z };
	
	public Axis axis = Axis.z;
	public Space space = Space.World;
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
		var target = (playBack ? fromQuart : toQuart);
		
		switch (space) {
			case Space.World: transform.rotation = target; break;
			case Space.Self: transform.localRotation = target; break;
		}
	}
	
	protected override void onAnimationProgress(float progress) {
		#if UNITY_EDITOR
		onAnimationBegin();
		#endif
		
		var target = Quaternion.AngleAxis(Mathf.LerpUnclamped(fromAngle, toAngle, progress), angleAxis);
		
		switch (space) {
			case Space.World: transform.rotation = target; break;
			case Space.Self: transform.localRotation = target; break;
		}
	}
}
