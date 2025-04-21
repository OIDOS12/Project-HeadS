using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public float offsetMultiplier = 1f;
    public float smoothTime = 0.3f;
    private Vector2 startPos;
    private Vector3 velocity;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Vector2 offset = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        transform.position = Vector3.SmoothDamp(transform.position, startPos + (offset * offsetMultiplier), ref velocity, smoothTime);
    }
}