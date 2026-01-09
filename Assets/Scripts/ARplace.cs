using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

public class ARplace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlacementUIHandler uiHandler; // <--- Drag UI Handler Here

    bool isPlacing = false;
    bool hasPlaced = false;   // 🔒 PLACE ONLY ONCE

    GameObject placedObject;

    float initialDistance;
    Vector3 initialScale;

    float initialAngle;
    float currentYRotation;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (!raycastManager) return;

        HandlePlacement();
        HandleScaleAndRotation();
    }

    // --- NEW METHOD: Called by UI Handler when close button is clicked ---
    public void ResetPlacement()
    {
        hasPlaced = false;
        placedObject = null;
    }

    void HandlePlacement()
    {
        if (isPlacing) return;
        if (hasPlaced) return;   // 🔒 block future placements

        bool pressed = false;
        Vector2 screenPosition = default;

        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPosition = primary.position.ReadValue();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        if (pressed)
        {
            isPlacing = true;
            PlaceObject(screenPosition);
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        var rayHits = new List<ARRaycastHit>();
        raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon);

        if (rayHits.Count > 0)
        {
            Pose hitPose = rayHits[0].pose;

            placedObject = Instantiate(
                raycastManager.raycastPrefab,
                hitPose.position,
                hitPose.rotation
            );

            hasPlaced = true;   // 🔒 permanently lock placement

            // --- TRIGGER UI HERE ---
            if (uiHandler != null)
            {
                uiHandler.ShowUIForModel(placedObject);
            }
        }

        StartCoroutine(SetIsPlacingToFalseWithDelay());
    }

    IEnumerator SetIsPlacingToFalseWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        isPlacing = false;
    }

    void HandleScaleAndRotation()
    {
        if (!hasPlaced) return;
        if (placedObject == null) return;
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.touches.Count < 2) return;

        var touch1 = Touchscreen.current.touches[0];
        var touch2 = Touchscreen.current.touches[1];

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began ||
            touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            initialDistance = Vector2.Distance(
                touch1.position.ReadValue(),
                touch2.position.ReadValue()
            );

            initialScale = placedObject.transform.localScale;

            Vector2 dir = touch2.position.ReadValue() - touch1.position.ReadValue();
            initialAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            currentYRotation = placedObject.transform.eulerAngles.y;
        }

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
            touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(
                touch1.position.ReadValue(),
                touch2.position.ReadValue()
            );

            float scaleFactor = currentDistance / initialDistance;
            placedObject.transform.localScale = initialScale * scaleFactor;

            Vector2 dir = touch2.position.ReadValue() - touch1.position.ReadValue();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float angleDelta = angle - initialAngle;

            placedObject.transform.rotation =
                Quaternion.Euler(0, currentYRotation - angleDelta, 0);
        }
    }
}