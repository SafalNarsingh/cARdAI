using UnityEngine;
using System.Collections;
using TMPro; // Only if using TextMeshPro

public class TrafficLightSimulator : MonoBehaviour
{
    // Sphere renderers
    private Renderer greenRenderer;
    private Renderer yellowRenderer;
    private Renderer redRenderer;

    // UI Text (can be outside prefab)
    public TextMeshProUGUI lightText;

    // Audio
    private AudioSource audioSource;
    public AudioClip greenSound;
    public AudioClip yellowSound;
    public AudioClip redSound;

    // Light durations
    public float greenTime = 5f;
    public float yellowTime = 2f;
    public float redTime = 5f;

    void Awake()
    {
        // Find child spheres
        greenRenderer = transform.Find("Green").GetComponent<Renderer>();
        yellowRenderer = transform.Find("Yellow").GetComponent<Renderer>();
        redRenderer = transform.Find("Red").GetComponent<Renderer>();

        // Use prefab's AudioSource
        audioSource = GetComponent<AudioSource>();

        // Dynamically find text if not assigned
        if (lightText == null)
        {
            GameObject textObj = GameObject.Find("TrafficLightText"); // put your text object name here
            if (textObj != null)
                lightText = textObj.GetComponent<TextMeshProUGUI>();
        }

        if (lightText == null)
        {
            GameObject textObj = GameObject.Find("TrafficLightText"); // replace with your Text object name
            if (textObj != null)
            {
                lightText = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("TrafficLightText not found in scene!");
            }
        }
    }

    void Start()
    {
        StartCoroutine(TrafficLightCycle());
    }

    IEnumerator TrafficLightCycle()
    {
        while (true)
        {
            SetLight("Green");
            yield return new WaitForSeconds(greenTime);

            SetLight("Yellow");
            yield return new WaitForSeconds(yellowTime);

            SetLight("Red");
            yield return new WaitForSeconds(redTime);
        }
    }

    void SetLight(string color)
    {

        Debug.Log("Setting Light: " + color);

        switch (color)
        {
            case "Green":
                audioSource.clip = greenSound;
                break;
            case "Yellow":
                audioSource.clip = yellowSound;
                break;
            case "Red":
                audioSource.clip = redSound;
                break;
        }

        if (audioSource.clip != null)
        {
            Debug.Log("Playing sound: " + audioSource.clip.name);
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioClip is null for " + color);
        }
        // Change sphere colors
        greenRenderer.material.color = (color == "Green") ? Color.green : Color.white;
        yellowRenderer.material.color = (color == "Yellow") ? Color.yellow : Color.white;
        redRenderer.material.color = (color == "Red") ? Color.red : Color.white;

        // Set message and assign corresponding sound
        if (lightText != null)
        {
            switch (color)
            {
                case "Green":
                    lightText.text = "Go! You can now Cross the Road";
                    lightText.color = Color.green;
                    audioSource.clip = greenSound;
                    break;
                case "Yellow":
                    lightText.text = "Get Ready to Move";
                    lightText.color = Color.yellow;
                    audioSource.clip = yellowSound;
                    break;
                case "Red":
                    lightText.text = "Stop! Wait for the Green Light";
                    lightText.color = Color.red;
                    audioSource.clip = redSound;
                    break;
            }
        }

        // Play sound if assigned
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }
}
