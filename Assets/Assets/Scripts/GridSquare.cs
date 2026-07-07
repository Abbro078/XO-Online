using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    
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
        if (markText == null) return;

        switch (player)
        {
            case PlayerType.None:
                markText.text = "";
                break;
            case PlayerType.X:
                markText.text = "X";
                break;
            case PlayerType.O:
                markText.text = "O";
                break;
        }
    }
}
