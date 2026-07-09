using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPulseAnimator : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("How fast the pulse loops")]
    public float pulseSpeed = 3f;
    [Tooltip("The lowest opacity during the pulse (0 to 1)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.4f;
    [Tooltip("The highest opacity during the pulse (0 to 1)")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Grab the CanvasGroup attached to this GameObject
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // 1. Generate a Sine wave that naturally goes up and down over time
        // Mathf.Sin naturally goes from -1 to 1. We +1 and /2 to make it go from 0 to 1.
        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // 2. Smoothly blend between your minimum and maximum alpha based on the wave
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
    }
}