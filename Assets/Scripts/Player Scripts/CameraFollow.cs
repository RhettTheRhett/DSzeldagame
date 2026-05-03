using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4, -5f);
    public Quaternion rotation = Quaternion.Euler(45, 0, 0);
    public float cameraFollowSpeed = 10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, cameraFollowSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
