using UnityEngine;
using System.Collections;

public class FrameRateLocker : MonoBehaviour {
    [Range(1, 120)]
    public int frameRate = 60;

	void Awake ()
    {
        Application.targetFrameRate = frameRate;
    }
}
