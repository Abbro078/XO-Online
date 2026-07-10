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
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
    }
}