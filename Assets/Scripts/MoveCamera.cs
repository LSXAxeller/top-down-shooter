using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float dampTime = 0.15f;
    public float speed = 1.5f;
    private Vector3 velocity = Vector3.zero;
    private Camera cam;


    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MapEditorCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            cam.orthographicSize += Input.GetAxis("Vertical")*-0.5f;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 5, 40);
        }
        else if(Mathf.Abs(Input.GetAxis("Horizontal")) >= 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) >= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift)) speed = 4; else speed = 1.5f;
            Vector3 delta = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector3 destination = transform.position + delta * speed;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }

    }
}