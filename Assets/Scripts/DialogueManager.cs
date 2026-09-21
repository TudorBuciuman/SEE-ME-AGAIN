using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoDialogueManager : MonoBehaviour
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)] public string text;
        public float charsPerSecond = 30f;  // typing speed for this line
        public float holdAfterTyped = 1.2f; // pause once fully typed before auto-advancing
        public AudioClip blipSound;         // optional per-line blip override
    }

    [Header("References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text speakerText;
    [SerializeField] private Text bodyText;
    [SerializeField] private AudioSource blipSource;
    [SerializeField] private AudioClip defaultBlip;
  
    [Header("Typing")]
    [SerializeField] private float defaultCharsPerSecond = 30f;
    [SerializeField] private int blipEveryNChars = 2;
    [SerializeField] private float holdAfterTyped = 1.2f; 

    [Header("Sequence")]
    [SerializeField] public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Optional")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float waitOnStart = 0f;

    public event Action<int, DialogueLine> OnLineStarted;
    public event Action<int, DialogueLine> OnLineFinishedTyping;
    public event Action OnDialogueComplete;

    private Coroutine _sequenceRoutine;
    private bool _skipRequested;
    private bool _isTyping;

    public void Start()
    {
        if (dialogueBox != null) 
            dialogueBox.SetActive(false);
        if (playOnStart)
        {
            if (waitOnStart > 0f)
                StartCoroutine(DelayedStart(waitOnStart));
            else
                Play(lines);
        }
    }
    public IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(lines);
    }
    public void Play(List<DialogueLine> sequence)
    {
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        lines = sequence;
        if (dialogueBox != null) dialogueBox.SetActive(true);
        _sequenceRoutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine line = lines[i];

            yield return StartCoroutine(TypeLine(line));

            OnLineFinishedTyping?.Invoke(i, line);

            // Hold on the fully-typed line before auto-advancing —
            // this is the "prophecy" pacing beat.
            yield return new WaitForSeconds(line.holdAfterTyped);
        }

        if (dialogueBox != null) dialogueBox.SetActive(false);
        OnDialogueComplete?.Invoke();
        _sequenceRoutine = null;
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        _isTyping = true;
        _skipRequested = false;
        bodyText.text = string.Empty;

        float cps = line.charsPerSecond > 0f ? line.charsPerSecond : defaultCharsPerSecond;
        float delay = 1f / cps;
        AudioClip blip = line.blipSound != null ? line.blipSound : defaultBlip;

        for (int c = 0; c < line.text.Length; c++)
        {
            if (_skipRequested)
            {
                bodyText.text = line.text;
                break;
            }

            bodyText.text += line.text[c];

            bool isVisibleChar = !char.IsWhiteSpace(line.text[c]);
            if (isVisibleChar && blip != null && c % Mathf.Max(1, blipEveryNChars) == 0)
            {
                blipSource?.PlayOneShot(blip);
            }

            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(holdAfterTyped); 
        bodyText.text = line.text;
        _isTyping = false;
    }
}