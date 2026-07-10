using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinLineAnimator : MonoBehaviour
{
    [Tooltip("Order: Row0, Row1, Row2, Col0, Col1, Col2, Diag1(\\), Diag2(/)")]
    [SerializeField] private Image[] winningLines;

    private float[] originalRotationsZ;

    private void Awake()
    {
        if (winningLines == null) return;
        
        originalRotationsZ = new float[winningLines.Length];
        for (int i = 0; i < winningLines.Length; i++)
        {
            if (winningLines[i] != null)
            {
                originalRotationsZ[i] = winningLines[i].transform.localEulerAngles.z;
                winningLines[i].gameObject.SetActive(false);
                winningLines[i].fillAmount = 0;
            }
        }
    }

    public void ResetLines()
    {
        if (winningLines == null) return;
        
        foreach (var line in winningLines)
        {
            if (line != null)
            {
                line.gameObject.SetActive(false);
                line.fillAmount = 0;
            }
        }
    }

    public IEnumerator AnimateWinLine(int lineIndex, int winX, int winY)
    {
        if (winningLines == null || lineIndex < 0 || lineIndex >= winningLines.Length) yield break;
        
        Image line = winningLines[lineIndex];
        if (line == null) yield break;

        line.gameObject.SetActive(true);
        line.fillAmount = 0;

        float randZ = Random.Range(-5f, 5f);
        Vector3 rot = line.transform.localEulerAngles;
        line.transform.localEulerAngles = new Vector3(rot.x, rot.y, originalRotationsZ[lineIndex] + randZ);
        
        if (lineIndex >= 0 && lineIndex <= 2) 
        {
            line.fillOrigin = (winY <= 1) ? 0 : 1; 
        }
        else if (lineIndex >= 3 && lineIndex <= 5) 
        {
            line.fillOrigin = (winX <= 1) ? 1 : 0; 
        }
        else if (lineIndex == 6) 
        {
            line.fillOrigin = (winX <= 1) ? 1 : 0;
        }
        else if (lineIndex == 7) 
        {
            line.fillOrigin = (winX <= 1) ? 0 : 1;
        }

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            line.fillAmount = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        line.fillAmount = 1f;
    }
}
