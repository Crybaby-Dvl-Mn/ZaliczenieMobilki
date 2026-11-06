using UnityEngine;

/// <summary>
/// Dynamiczna kamera sledzaca obu graczy i dopasowujaca zoom ortograficzny.
/// </summary>
[RequireComponent(typeof(Camera))]
public class DynamicCamera : MonoBehaviour
{
    [Header("Gracze")]
    public Transform player1;
    public Transform player2;

    [Header("Ruch kamery")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.1f;

    [Header("Zoom")]
    public float minSize = 5f;
    public float maxSize = 12f;
    public float zoomFactor = 1.5f;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 targetPos = (player1.position + player2.position) * 0.5f;
        targetPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);

        float distance = Vector3.Distance(player1.position, player2.position);
        float desiredSize = Mathf.Clamp(distance * zoomFactor, minSize, maxSize);
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, desiredSize, smoothSpeed);
    }
}
