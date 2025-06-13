using UnityEngine;
using UnityEngine.UI;

public class ToolbeltSlotUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject selectionHighlight;

    private void Start()
    {
    }

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    public void SetActive(bool active)
    {
        selectionHighlight.SetActive(active);
    }
}