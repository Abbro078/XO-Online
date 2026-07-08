using UnityEngine;
using UnityEngine.UI;

public class UIFlipAnimator : MonoBehaviour
{
    [Header("Face Objects")]
    public GameObject frontFace;
    public GameObject backFace;

    [Header("Text Fading")]
    [Tooltip("Drag your Text or TextMeshPro component here")]
    public Graphic fadingText; 
    [Tooltip("Maximum Alpha (1 = 255)")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;       
    [Tooltip("Minimum Alpha (0.784 = ~200/255)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.784f;   

    [Header("Animation Settings")]
    public float spinSpeed = 180f; 
    public float loopPauseDuration = 0.3f; 

    private float currentRotationY = 0f;
    private float pauseTimer = 0f;
    private bool isPaused = false;

    void Update()
    {
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                currentRotationY = 0f;
                transform.localRotation = Quaternion.identity; 
            }
            return; 
        }

        float step = spinSpeed * Time.deltaTime;
        currentRotationY += step;

        if (currentRotationY >= 360f)
        {
            float overshoot = currentRotationY - 360f;
            step -= overshoot;
            
            isPaused = true;
            pauseTimer = loopPauseDuration;
            
            currentRotationY = 360f; 
        }

        transform.Rotate(0f, step, 0f);

        float currentYAngle = transform.eulerAngles.y % 360f;
        bool isBackFacingUs = currentYAngle > 90f && currentYAngle < 270f;

        if (frontFace.activeSelf == isBackFacingUs)
        {
            frontFace.SetActive(!isBackFacingUs);
            backFace.SetActive(isBackFacingUs);
        }

        if (fadingText != null)
        {
            float wave = (1f - Mathf.Cos(currentRotationY * Mathf.Deg2Rad)) / 2f;
            
            float currentAlpha = Mathf.Lerp(maxAlpha, minAlpha, wave);
            
            Color c = fadingText.color;
            c.a = currentAlpha;
            fadingText.color = c;
        }
    }
}