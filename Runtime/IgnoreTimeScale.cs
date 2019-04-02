using UnityEngine;

public interface IgnoreTimeScale {
	void setIgnoreTimeScale(bool ignore);
}

public static class GameObjectIgnoreTimeScaleExtension {
	public static void setIgnoreTimeScale(this GameObject gameObject, bool ignore) {
		var components = gameObject.GetComponentsInChildren<IgnoreTimeScale>(includeInactive: true);
		for (var i = 0; i < components.Length; i++) {
			components[i].setIgnoreTimeScale(ignore);
		}
		
		var particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (var i = 0; i < particleSystems.Length; i++) {
			var settings = particleSystems[i].main;
			settings.useUnscaledTime = ignore;
		}
		
		var animators = gameObject.GetComponentsInChildren<Animator>(includeInactive: true);
		for (var i = 0; i < animators.Length; i++) {
			animators[i].updateMode = (ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal);
		}
	}
}
