using UnityEngine;

// Attach to any GameObject. Assign CloudNoiseCompute and targetMaterial, and it will
// bake the 3D noise volume and assign it to the material automatically on Start
// (and any time you call Bake() manually, e.g. from the context menu).
//
// Note: we deliberately do NOT read the baked RenderTexture back to the CPU and save
// it as a Texture3D asset. AsyncGPUReadback on 3D RenderTextures is unreliable across
// GPU/driver combinations (mismatched buffer sizes either direction). Baking straight
// into a RenderTexture and feeding it to the material directly avoids that entirely,
// and the bake itself is cheap (a few ms for 128^3), so doing it once per play session
// costs nothing noticeable.
[ExecuteAlways]
public class CloudNoiseBaker : MonoBehaviour
{
    public ComputeShader noiseCompute;
    [Range(32, 256)] public int resolution = 128;
    public float seed = 0f;

    [Tooltip("The VolumetricClouds material — its 'Cloud Noise 3D' slot gets assigned automatically after baking.")]
    public Material targetMaterial;

    [HideInInspector] public RenderTexture bakedTexture;

    private void Start()
    {
        Bake();
    }

    [ContextMenu("Bake Noise Texture")]
    public void Bake()
    {
        if (noiseCompute == null)
        {
            Debug.LogError("CloudNoiseBaker: assign CloudNoiseGen.compute first.");
            return;
        }

        if (bakedTexture != null)
        {
            bakedTexture.Release();
        }

        var rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32)
        {
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            volumeDepth = resolution,
            enableRandomWrite = true,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear
        };
        rt.Create();

        int kernel = noiseCompute.FindKernel("CSMain");
        noiseCompute.SetTexture(kernel, "Result", rt);
        noiseCompute.SetInt("Resolution", resolution);
        noiseCompute.SetFloat("Seed", seed);

        int groups = Mathf.CeilToInt(resolution / 4f);
        noiseCompute.Dispatch(kernel, groups, groups, groups);

        bakedTexture = rt;

        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_NoiseTex", bakedTexture);
        }

        Debug.Log($"CloudNoiseBaker: baked {resolution}^3 noise volume{(targetMaterial != null ? " and assigned to material" : "")}.");
    }
}

