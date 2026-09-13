using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A plain character-by-character reveal — no scramble, no decode effect.
/// Used anywhere the digital-glitch treatment would feel wrong: the bilingual
/// apology, and the campfire-story ending where only sound and simple text
/// should carry the moment.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class SimpleTypewriter : MonoBehaviour
{
    public float charInterval = 0.05f;
    [Tooltip("Extra pause when a punctuation mark is revealed, for a more spoken cadence.")]
    public float punctuationPause = 0.35f;

    TMP_Text _text;
    Coroutine _running;

    void Awake() => _text = GetComponent<TMP_Text>();

    public void Play(string line, System.Action onComplete = null)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(Reveal(line, onComplete));
    }

    public void Clear()
    {
        if (_running != null) StopCoroutine(_running);
        _text.text = "";
    }

    IEnumerator Reveal(string line, System.Action onComplete)
    {
        _text.text = "";
        for (int i = 0; i < line.Length; i++)
        {
            _text.text += line[i];
            bool isPunctuation = ".,!?".IndexOf(line[i]) >= 0;
            yield return new WaitForSeconds(isPunctuation ? punctuationPause : charInterval);
        }
        _running = null;
        onComplete?.Invoke();
    }
}
