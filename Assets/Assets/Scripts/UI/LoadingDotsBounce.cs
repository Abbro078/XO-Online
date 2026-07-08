using UnityEngine;

public class LoadingDotsBounce : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your 3 dot RectTransforms here in order (Left to Right)")]
    public RectTransform[] dots;

    [Header("Bounce Settings")]
    public float bounceHeight = 15f; 
    public float speed = 5f;
    [Tooltip("How far behind the previous dot the next one should be")]
    public float staggerDelay = 0.5f; 

    private float[] startY;

    void Start()
    {
        startY = new float[dots.Length];
        
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
            {
                startY[i] = dots[i].anchoredPosition.y;
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;
            
            float wave = Mathf.Abs(Mathf.Sin(Time.time * speed - (i * staggerDelay)));
            
            float newY = startY[i] + (wave * bounceHeight);

            dots[i].anchoredPosition = new Vector2(dots[i].anchoredPosition.x, newY);
        }
    }
}