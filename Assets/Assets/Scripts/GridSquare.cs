using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    
    [SerializeField] private GameObject markImageContainer;
    [SerializeField] private TextMeshProUGUI markText;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnSquareClicked);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSquare(x, y, this);
        }
        else
        {
            Debug.LogError("GameManager instance not found. Make sure there is a GameManager in the scene.");
        }
    }

    private void OnSquareClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryPlaceMark(x, y);
        }
    }

    public void UpdateVisual(PlayerType player)
    {
        if (player == PlayerType.None)
        {
            if (markImageContainer != null) markImageContainer.SetActive(false);
            if (markText != null) markText.text = "";
            return;
        }

        if (markImageContainer != null) markImageContainer.SetActive(true);

        if (markText != null)
        {
            markText.text = player == PlayerType.X ? "X" : "O";
        }

        if (markImageContainer != null)
        {
            float randomZ = Random.Range(-5f, 5f);
            markImageContainer.transform.localRotation = Quaternion.Euler(0, 0, randomZ);

            StopAllCoroutines();
            StartCoroutine(FallAnimationCoroutine());
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlaceMark();
        }
    }

    private System.Collections.IEnumerator FallAnimationCoroutine()
    {
        RectTransform rt = markImageContainer.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float duration = 0.35f;
        float elapsed = 0f;

        Vector2 endPos = new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
        Vector2 startPos = endPos + new Vector2(0, 150f);

        Vector3 startScale = new Vector3(1.3f, 1.3f, 1f);
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float s = 2.5f;
            float p = t - 1f;
            float easeT = (p * p * ((s + 1f) * p + s) + 1f);
            
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, easeT);
            rt.localScale = Vector3.LerpUnclamped(startScale, endScale, easeT);
            
            yield return null;
        }

        rt.anchoredPosition = endPos;
        rt.localScale = endScale;
    }
}
