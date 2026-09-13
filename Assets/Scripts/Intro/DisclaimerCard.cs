using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Plays a pre-game disclaimer card in the same structural spirit as
/// Cyberpunk 2077's — clauses shown one at a time, joined visually with "//" —
/// but deliberately kept plain and serious rather than neon-styled. Feed it
/// your clauses (age rating, epilepsy warning, "views are the author's own",
/// whatever you need), it reveals them in order, then signals completion.
/// </summary>
public class DisclaimerCard : MonoBehaviour
{
    public TMP_Text disclaimerText;

    [TextArea(2, 4)]
    public string[] clauses;

    public float secondsPerClause = 4f;
    public bool allowAdvanceOnInput = true;

    int _index;
    public System.Action OnFinished;

    public void Play()
    {
        _index = 0;
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (_index < clauses.Length)
        {
            disclaimerText.text = $"// {clauses[_index]}";

            float t = 0f;
            bool advanced = false;
            while (t < secondsPerClause && !advanced)
            {
                t += Time.deltaTime;
                if (allowAdvanceOnInput && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
                    advanced = true;
                yield return null;
            }
            _index++;
        }

        OnFinished?.Invoke();
    }
}
