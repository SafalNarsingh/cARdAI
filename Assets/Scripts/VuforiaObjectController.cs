using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System;

public class ARCollectionManager : MonoBehaviour
{
    [System.Serializable]
    public struct ARItem
    {
        public string name;           // Optional: For your organization (e.g., "Card 1")
        public GameObject model;      // The 3D Model in the scene
        public AudioClip audioClip;   // The audio for this specific model
    }

    [Header("Collection Setup")]
    [Tooltip("Drag your 52 Model/Audio pairs here")]
    public List<ARItem> arItems;

    [Header("Global Settings")]
    [SerializeField] private AudioSource audioSource; // Single AudioSource to play all clips
    [SerializeField] private float rotationSpeed = 0.2f;

    // Internal tracker for which object is currently being controlled
    private GameObject currentSelectedModel = null;

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
        HandleInput();
    }


    void HandleInput()
    {
        // 1. TOUCH INPUT
        if (Touch.activeTouches.Count > 0)
        {
            Touch primaryTouch = Touch.activeTouches[0];

            if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // On tap, try to find an object
                SelectObject(primaryTouch.screenPosition);
            }
            else if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                // If we have an object selected, rotate it
                if (currentSelectedModel != null)
                {
                    RotateSelected(primaryTouch.delta);
                }
            }
            else if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                // Optional: Stop rotating when letting go
                currentSelectedModel = null;
            }
        }
        // 2. MOUSE INPUT (For Editor Testing)
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                SelectObject(Mouse.current.position.ReadValue());
            }
            else if (Mouse.current.leftButton.isPressed && currentSelectedModel != null)
            {
                RotateSelected(Mouse.current.delta.ReadValue());
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                currentSelectedModel = null;
            }
        }
    }

    void SelectObject(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // We hit something! Now check if it matches one of our 52 items.
            // We check if the hit object is the model OR a child of the model.
            foreach (var item in arItems)
            {
                if (item.model == null) continue;

                // Check: Is the hit object the model itself? OR Is the hit object a child of the model?
                if (hit.transform == item.model.transform || hit.transform.IsChildOf(item.model.transform))
                {
                    // FOUND IT!
                    currentSelectedModel = item.model;
                    PlayAudio(item.audioClip);
                    return; // Stop searching
                }
            }
        }
    }

    //private void PlayAudio(AudioClip audioClip)
    //{
    //    throw new NotImplementedException();
    //}

    void RotateSelected(Vector2 delta)
    {
        if (currentSelectedModel != null)
        {
            float rotationAmount = -delta.x * rotationSpeed;
            currentSelectedModel.transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Stop current sound and play the new one
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}