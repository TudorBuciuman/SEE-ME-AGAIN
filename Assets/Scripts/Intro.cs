using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SeeMeAgainIntro : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private RectTransform redSky;
    [SerializeField] private CanvasGroup studioLogo;
    [SerializeField] private CanvasGroup grass;
    [SerializeField] private CanvasGroup campfire;

    [Header("Particles")]
    [SerializeField] private ParticleSystem campfireSmoke;
    [SerializeField] private ParticleSystem embers;

    [Header("Sky Animation")]
    [SerializeField] private float skySlideDistance = 900f;
    [SerializeField] private float skySlideDuration = 2.5f;

    [Header("Logo")]
    [SerializeField] private float logoFadeIn = 0.8f;
    [SerializeField] private float logoHoldTime = 1.5f;

    [Header("Campfire")]
    [SerializeField] private float campfireFadeIn = 1.0f;
    [SerializeField] private float smokeDelay = 0.4f;
    [SerializeField] private float smokeDuration = 2.5f;

    [Header("Ember")]
    [SerializeField] private float emberDelay = 0.5f;

    private Vector2 originalSkyPosition;

    private void Awake()
    {
        originalSkyPosition = redSky.anchoredPosition;

        studioLogo.alpha = 0f;
        grass.alpha = 0f;
        campfire.alpha = 0f;

        campfireSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        /*
         * ---------------------------------------------------------
         * 1. RED SKY
         * ---------------------------------------------------------
         */

        // Grass is already underneath the sky.
        grass.alpha = 1f;

        yield return new WaitForSeconds(0.5f);

        /*
         * ---------------------------------------------------------
         * 2. SKY SLIDES UP
         * ---------------------------------------------------------
         */

        yield return SlideSky();

        /*
         * ---------------------------------------------------------
         * 3. STUDIO LOGO APPEARS
         * ---------------------------------------------------------
         */

        yield return FadeCanvasGroup(
            studioLogo,
            0f,
            1f,
            logoFadeIn
        );

        yield return new WaitForSeconds(logoHoldTime);

        /*
         * ---------------------------------------------------------
         * 4. CAMPFIRE APPEARS
         * ---------------------------------------------------------
         */

        yield return FadeCanvasGroup(
            campfire,
            0f,
            1f,
            campfireFadeIn
        );

        yield return new WaitForSeconds(smokeDelay);

        /*
         * ---------------------------------------------------------
         * 5. SMOKE
         * ---------------------------------------------------------
         */

        campfireSmoke.Play();

        yield return new WaitForSeconds(smokeDuration);

        /*
         * ---------------------------------------------------------
         * 6. LOGO DISAPPEARS INTO SMOKE
         * ---------------------------------------------------------
         */

        yield return FadeCanvasGroup(
            studioLogo,
            1f,
            0f,
            1.2f
        );

        /*
         * ---------------------------------------------------------
         * 7. ONE EMBER
         * ---------------------------------------------------------
         */

        yield return new WaitForSeconds(emberDelay);

        embers.Play();

        /*
         * ---------------------------------------------------------
         * 8. HAND CONTROL TO ANNOUNCEMENT
         * ---------------------------------------------------------
         */

        yield return new WaitForSeconds(1.5f);

        Announcement();
    }

    private IEnumerator SlideSky()
    {
        Vector2 start = originalSkyPosition;
        Vector2 end = start + Vector2.up * skySlideDistance;

        float timer = 0f;

        while (timer < skySlideDuration)
        {
            timer += Time.deltaTime;

            float t = timer / skySlideDuration;

            // Smooth cinematic easing.
            t = EaseInOutCubic(t);

            redSky.anchoredPosition =
                Vector2.Lerp(start, end, t);

            yield return null;
        }

        redSky.anchoredPosition = end;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        float from,
        float to,
        float duration)
    {
        float timer = 0f;

        group.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = Mathf.Clamp01(t);

            // Smoothstep.
            t = t * t * (3f - 2f * t);

            group.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        group.alpha = to;
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private void Announcement()
    {
        /*
         * Leave this part to yourself.
         *
         * Example:
         *
         * announcement.SetActive(true);
         *
         * Or trigger your own cinematic / UI sequence.
         */

        Debug.Log("SEE ME AGAIN — Announcement begins.");
    }
}