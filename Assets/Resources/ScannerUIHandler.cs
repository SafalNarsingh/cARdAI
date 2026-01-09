using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Vuforia;

public class ScannerUIHandler : MonoBehaviour
{
    // --- STATIC MANAGER (Prevents Conflict) ---
    private static ScannerUIHandler currentActiveHandler = null;

    [Header("UI References (Buttons stay, Text changes)")]
    public Button mainNameButton;
    public Button phonemeButton1;
    public Button syllableButton2;
    public Button soundButton;
    public Button closeButton;

    [Header("Model Reference")]
    public GameObject modelObject;      // Model still hides when lost

    [Header("Audio")]
    public AudioSource audioSourceEn;
    public AudioSource audioSourceNp;

    [Header("English Data")]
    public string nameEn;
    public string phonemeEn;
    public string syllableEn;

    [Header("Nepali Data")]
    public string nameNp;
    public string phonemeNp;
    public string syllableNp;

    private AudioSource currentAudioSource;
    private ObserverBehaviour observerBehaviour;
    private bool manuallyClosed = false;

    // Internal references to the Text components (so we can hide just the text)
    private TextMeshProUGUI mainTextComp;
    private TextMeshProUGUI phoneTextComp;
    private TextMeshProUGUI syllTextComp;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        // Cache the text components so we don't look for them every frame
        if (mainNameButton) mainTextComp = mainNameButton.GetComponentInChildren<TextMeshProUGUI>();
        if (phonemeButton1) phoneTextComp = phonemeButton1.GetComponentInChildren<TextMeshProUGUI>();
        if (syllableButton2) syllTextComp = syllableButton2.GetComponentInChildren<TextMeshProUGUI>();

        // Ensure Model starts hidden
        if (modelObject) modelObject.SetActive(false);

        // Ensure UI Text starts empty/hidden
        ToggleTextVisibility(false);
    }

    private void OnDestroy()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        bool isTracked =
            targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED;

        if (isTracked)
        {
            if (!manuallyClosed) OnTargetFound();
        }
        else
        {
            OnTargetLost();
            manuallyClosed = false;
        }
    }

    private void OnTargetFound()
    {
        // 1. Take Ownership
        currentActiveHandler = this;

        // 2. Show Model
        if (modelObject) modelObject.SetActive(true);

        // 3. Determine Language
        string currentLocale = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
        bool isNepali = (currentLocale == "ne-NP");

        // 4. Update the TEXT content (and make it visible)
        if (mainTextComp)
        {
            mainTextComp.text = isNepali ? nameNp : nameEn;
            mainTextComp.enabled = true; // Show Text
        }
        if (phoneTextComp)
        {
            phoneTextComp.text = isNepali ? phonemeNp : phonemeEn;
            phoneTextComp.enabled = true; // Show Text
        }
        if (syllTextComp)
        {
            syllTextComp.text = isNepali ? syllableNp : syllableEn;
            syllTextComp.enabled = true; // Show Text
        }

        // 5. Ensure the Buttons themselves are active (Backgrounds visible)
        if (mainNameButton) mainNameButton.gameObject.SetActive(true);
        if (phonemeButton1) phonemeButton1.gameObject.SetActive(true);
        if (syllableButton2) syllableButton2.gameObject.SetActive(true);
        if (soundButton) soundButton.gameObject.SetActive(true);
        if (closeButton) closeButton.gameObject.SetActive(true);

        // 6. Audio Setup
        currentAudioSource = isNepali ? audioSourceNp : audioSourceEn;

        if (soundButton)
        {
            soundButton.onClick.RemoveAllListeners();
            soundButton.onClick.AddListener(() =>
            {
                // Plays the sound of the CURRENT active card
                if (currentAudioSource != null) currentAudioSource.Play();
            });
        }

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (currentActiveHandler == this)
                {
                    // If user manually closes, we hide EVERYTHING (buttons included)
                    // because "Close" implies "Clear Screen"
                    HideAllUI();
                    manuallyClosed = true;
                }
            });
        }
    }

    private void OnTargetLost()
    {
        // Always hide Model
        if (modelObject) modelObject.SetActive(false);

        // --- THE LOGIC: UNLOAD TEXT ONLY ---
        // Only modify UI if we are the owner
        if (currentActiveHandler == this)
        {
            // We DO NOT hide the buttons. We only hide the Text.
            ToggleTextVisibility(false);

            // We DO NOT stop the audio or hide the sound button. 
            // It stays on screen as "Last Detected" state.

            currentActiveHandler = null;
        }
    }

    // Helper to toggle ONLY the text (leaving background bubbles visible)
    private void ToggleTextVisibility(bool state)
    {
        // Option A: Disable the Text Component (Text vanishes, Bubble stays)
        if (mainTextComp) mainTextComp.enabled = state;
        if (phoneTextComp) phoneTextComp.enabled = state;
        if (syllTextComp) syllTextComp.enabled = state;

        // Option B: (Alternative) Set text to empty string
        // if (!state) { mainTextComp.text = ""; ... }
    }

    // Helper to hide EVERYTHING (used only for Manual Close)
    private void HideAllUI()
    {
        if (mainNameButton) mainNameButton.gameObject.SetActive(false);
        if (phonemeButton1) phonemeButton1.gameObject.SetActive(false);
        if (syllableButton2) syllableButton2.gameObject.SetActive(false);
        if (soundButton) soundButton.gameObject.SetActive(false);
        if (closeButton) closeButton.gameObject.SetActive(false);
    }
}