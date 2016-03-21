using UnityEngine;
using System.Collections;

public class MiniMapCameraMovement : MonoBehaviour {

    public float size;
    private Transform followObj;

    void Start()
    {
        
        followObj = Camera.main.transform;
    }

    void Update()
    {
        GetComponent<Camera>().orthographicSize = size;
        transform.position = new Vector3(followObj.position.x, followObj.position.y, -10f);
    }
}
