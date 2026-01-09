using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    [Header("UI References")]
    public Button switchButton;          // The button you click
    public TextMeshProUGUI buttonLabel;  // The text inside the button (e.g. "NEP")

    private bool isChanging = false;     // Prevents spam clicking

    void Start()
    {
        // 1. Attach the click event
        if (switchButton != null)
        {
            switchButton.onClick.RemoveAllListeners();
            switchButton.onClick.AddListener(ToggleLanguage);
        }

        // 2. Set the correct button text immediately on start
        StartCoroutine(UpdateLabelRoutine());
    }

    void ToggleLanguage()
    {
        if (isChanging) return;
        StartCoroutine(ChangeLanguageRoutine());
    }

    IEnumerator ChangeLanguageRoutine()
    {
        isChanging = true;

        // 1. Check which language we are currently using
        // (Assuming "en" is English and "ne-NP" is Nepali)
        string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        // 2. Determine the NEXT language
        // If current is Nepali, switch to English. Otherwise, switch to Nepali.
        string targetCode = (currentCode == "ne-NP") ? "en" : "ne-NP";

        // 3. Find the locale in the project settings
        var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(targetCode);

        // 4. Wait for Unity to actually perform the switch
        yield return LocalizationSettings.SelectedLocale = targetLocale;

        // 5. Update the button text to match the new state
        UpdateButtonText(targetCode);

        isChanging = false;
    }

    // Helper to verify initialization before setting text
    IEnumerator UpdateLabelRoutine()
    {
        // Wait until the Localization system is ready
        yield return LocalizationSettings.InitializationOperation;

        string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        UpdateButtonText(currentCode);
    }

    void UpdateButtonText(string currentCode)
    {
        if (buttonLabel == null) return;

        // LOGIC: 
        // If current is English ("en"), show "NEP" (so user knows clicking will switch to Nepali).
        // If current is Nepali ("ne-NP"), show "EN".
        if (currentCode == "ne-NP")
        {
            buttonLabel.text = "EN";
        }
        else
        {
            buttonLabel.text = "NEP";
        }
    }
}