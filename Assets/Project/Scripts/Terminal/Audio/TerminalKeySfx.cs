using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

/// <summary>
/// Plays key SFX while a terminal input field is focused.
/// Uses the new Input System (Keyboard.current).
/// </summary>
public class TerminalKeySfx : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip keyPress;
    [SerializeField] private AudioClip keySpace;
    [SerializeField] private AudioClip keyBackspace;
    [SerializeField] private AudioClip keyEnter;

    [Header("Tuning")]
    [Tooltip("Volume for space, backspace, and enter (matches shell “Key sounds → Volume”).")]
    [SerializeField, Range(0f, 5f)]
    [FormerlySerializedAs("volume")]
    private float keySoundsVolume = 1f;

    [Tooltip("Volume for regular key presses — letters, numbers, symbols, arrows, Tab (matches shell “Keypress volume”).")]
    [SerializeField, Range(0f, 5f)]
    private float keyPressVolume = 1f;

    private void Reset()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponentInChildren<TMP_InputField>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        if (inputField == null)
            inputField = GetComponentInChildren<TMP_InputField>();

        if (inputField != null)
            inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnDisable()
    {
        if (inputField != null)
            inputField.onSubmit.RemoveListener(OnSubmit);
    }

    private void Update()
    {
        if (inputField == null || audioSource == null) return;
        if (!inputField.isFocused) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Enter is handled via TMP_InputField.onSubmit to avoid focus-timing issues.
        if (kb.backspaceKey.wasPressedThisFrame || kb.deleteKey.wasPressedThisFrame)
        {
            Play(keyBackspace, keySoundsVolume);
            return;
        }

        if (kb.spaceKey.wasPressedThisFrame)
        {
            Play(keySpace, keySoundsVolume);
            return;
        }

        // Any other "printing" key: letters/numbers/punctuation, etc.
        // Filter out modifier/navigation keys so they don't spam.
        foreach (KeyControl key in kb.allKeys)
        {
            if (!key.wasPressedThisFrame) continue;
            if (IsIgnoredKey(key)) continue;

            Play(keyPress, keyPressVolume);
            return;
        }
    }

    private void OnSubmit(string _)
    {
        Play(keyEnter, keySoundsVolume);
    }

    private void Play(AudioClip clip, float volumeScale)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, volumeScale);
    }

    private static bool IsIgnoredKey(KeyControl key)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return true;

        // modifiers
        if (key == kb.leftShiftKey || key == kb.rightShiftKey) return true;
        if (key == kb.leftCtrlKey || key == kb.rightCtrlKey) return true;
        if (key == kb.leftAltKey || key == kb.rightAltKey) return true;
        if (key == kb.leftMetaKey || key == kb.rightMetaKey) return true;

        // navigation / editing keys we don't want as "press"
        if (key == kb.upArrowKey || key == kb.downArrowKey) return true;
        if (key == kb.leftArrowKey || key == kb.rightArrowKey) return true;
        if (key == kb.tabKey || key == kb.escapeKey) return true;
        if (key == kb.homeKey || key == kb.endKey) return true;
        if (key == kb.pageDownKey || key == kb.pageUpKey) return true;
        if (key == kb.capsLockKey) return true;

        // handled explicitly
        if (key == kb.enterKey || key == kb.numpadEnterKey) return true;
        if (key == kb.backspaceKey || key == kb.deleteKey) return true;
        if (key == kb.spaceKey) return true;

        return false;
    }
}

