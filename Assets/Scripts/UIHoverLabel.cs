using UnityEngine;
using TMPro;

public class UIHoverLabel : MonoBehaviour
{

    void Start()
    {
        GetComponent<TextMeshProUGUI>().enabled = false;
    }

    public void Show(string text, Vector2 screenPosition)
    {
        GetComponent<TextMeshProUGUI>().text = text;
        GetComponent<TextMeshProUGUI>().transform.position = screenPosition;
        GetComponent<TextMeshProUGUI>().enabled = true;
    }

    public void Hide()
    {
        GetComponent<TextMeshProUGUI>().enabled = false;
    }
}
