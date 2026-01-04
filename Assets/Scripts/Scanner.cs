using UnityEngine;

public class ARObjectInteraction : MonoBehaviour
{
    private float minScale;
    private float maxScale;
    private float rotationSpeed;

    private Vector3 initialScale;
    private Quaternion initialRotation;

    private Vector2 lastTouchPosition;
    private bool isRotating;

    public void Initialize(float min, float max, float rotSpeed)
    {
        minScale = min;
        maxScale = max;
        rotationSpeed = rotSpeed;

        initialScale = transform.localScale;
        initialRotation = transform.rotation;
    }

    public void ResetTransform()
    {
        transform.localScale = initialScale;
        transform.rotation = initialRotation;
    }

    void Update()
    {
        // ROTATION ONLY — no translation, no scaling
        if (Input.touchCount != 1)
        {
            isRotating = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPosition = touch.position;
            isRotating = true;
            return;
        }

        if (touch.phase == TouchPhase.Moved && isRotating)
        {
            Vector2 delta = touch.position - lastTouchPosition;

            // World-space Y rotation (correct for AR)
            float rotationY = -delta.x * rotationSpeed;
            transform.Rotate(Vector3.up, rotationY, Space.World);

            lastTouchPosition = touch.position;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isRotating = false;
        }
    }

    // ---------------- VISUAL FEEDBACK ----------------

    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public void Highlight(bool enable)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = enable
                    ? originalColors[i] * 1.3f
                    : originalColors[i];
            }
        }
    }
}
