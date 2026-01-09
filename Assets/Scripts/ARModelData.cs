using UnityEngine;

public class ARModelData : MonoBehaviour
{
    [Header("English Data")]
    public string displayNameEn;    // e.g., "Dog"
    public string phonemeEn;        // e.g., "/d-og/"

    [Header("Nepali Data")]
    public string displayNameNp;    // e.g., "Kukur"
    public string phonemeNp;        // e.g., "/ku-kur/"

    [Header("Audio")]
    public AudioClip audioClipEn;
    public AudioClip audioClipNp;
}