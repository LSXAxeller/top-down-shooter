using UnityEngine;

[ExecuteInEditMode]
public class NavMeshRenderer : MonoBehaviour {
	
	/** Last level loaded. Used to check for scene switches */
	string lastLevel = "";
	
	/** Used to get rid of the compiler warning that #lastLevel is not used */
	public string SomeFunction () {
		return lastLevel;
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if (lastLevel == "") {
			lastLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		}
		
		if (lastLevel != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) {
			DestroyImmediate (gameObject);
		}
		#endif
	}
}