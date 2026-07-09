using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI symbolText;

    public void Setup(string playerName, string symbol)
    {
        if (nameText != null)
        {
            nameText.text = playerName;
        }

        if (symbolText != null)
        {
            symbolText.text = symbol;
        }
    }
}
