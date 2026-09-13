using System.Collections;
using UnityEngine;

/// <summary>
/// Animates named float properties on a material over time using
/// AnimationCurves — this is how the intro touches your logo shader and your
/// CRT_Full / CRT_AnalogNoise shaders without this project ever editing
/// those shader files. Point it at the Renderer (3D) or Graphic (UI) using
/// your material, list the exact property names you want driven, and call
/// Play() from a phase's onPhaseEnter.
/// </summary>
public class ShaderPropertyDriver : MonoBehaviour
{
    [System.Serializable]
    public class DrivenProperty
    {
        [Tooltip("Exact shader property name as it appears in the shader source, e.g. \"_TwitchAmount\" or \"_NoiseIntensity\".")]
        public string propertyName;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    [Tooltip("Use for a 3D object's shader (logo mesh, etc.). Leave null if driving a UI Graphic instead.")]
    public Renderer targetRenderer;

    [Tooltip("Use for a UI Image/RawImage's shader. Leave null if driving a Renderer instead.")]
    public UnityEngine.UI.Graphic targetGraphic;

    public DrivenProperty[] properties;
    public float duration = 2f;
    public bool loop = false;
    public bool useUnscaledTime = false;

    MaterialPropertyBlock _block;
    Material _runtimeMaterial;

    void Awake()
    {
        _block = new MaterialPropertyBlock();
        if (targetGraphic != null)
        {
            // UI materials don't take MaterialPropertyBlocks the way Renderers do,
            // so instance the material directly — the shared asset stays untouched.
            _runtimeMaterial = targetGraphic.material = Instantiate(targetGraphic.material);
        }
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(Drive());
    }

    public void Stop() => StopAllCoroutines();

    IEnumerator Drive()
    {
        do
        {
            float t = 0f;
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                Apply(Mathf.Clamp01(t / duration));
                yield return null;
            }
            Apply(1f);
        } while (loop);
    }

    void Apply(float normalized)
    {
        foreach (var p in properties)
        {
            float value = p.curve.Evaluate(normalized);

            if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(_block);
                _block.SetFloat(p.propertyName, value);
                targetRenderer.SetPropertyBlock(_block);
            }
            else if (_runtimeMaterial != null)
            {
                _runtimeMaterial.SetFloat(p.propertyName, value);
            }
        }
    }
}
