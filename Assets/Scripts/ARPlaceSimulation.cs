using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

public class ARSimulationPlacer : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;

    private bool simulationPlaced = false;

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
        if (simulationPlaced) return;

        bool tapped = false;
        Vector2 tapPosition = default;

        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                tapped = true;
                tapPosition = primary.position.ReadValue();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tapped = true;
            tapPosition = Mouse.current.position.ReadValue();
        }

        if (tapped)
        {
            PlaceSimulationEnvironment(tapPosition);
        }
    }

    void PlaceSimulationEnvironment(Vector2 screenPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon);

        if (hits.Count > 0)
        {
            Pose pose = hits[0].pose;
            Instantiate(raycastManager.raycastPrefab, pose.position, pose.rotation);
            simulationPlaced = true; // lock after first placement
        }
    }
}
