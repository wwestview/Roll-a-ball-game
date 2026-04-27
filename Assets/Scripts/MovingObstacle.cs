using UnityEngine;

/// <summary>
/// Moves an obstacle back and forth between two points (ping-pong motion).
/// Attach to a cube/obstacle GameObject with a collider.
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    public Vector3 pointA = Vector3.zero;
    public Vector3 pointB = Vector3.right * 5f;
    public float speed = 2f;
    public bool useLocalSpace = false;

    private float t = 0f;
    private int direction = 1;

    void Update()
    {
        t += direction * speed * Time.deltaTime;

        if (t >= 1f)
        {
            t = 1f;
            direction = -1;
        }
        else if (t <= 0f)
        {
            t = 0f;
            direction = 1;
        }

        Vector3 newPos = Vector3.Lerp(pointA, pointB, Mathf.SmoothStep(0f, 1f, t));

        if (useLocalSpace)
            transform.localPosition = newPos;
        else
            transform.position = newPos;
    }
}
