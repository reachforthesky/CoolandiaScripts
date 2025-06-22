using UnityEngine;

public enum UIType
{
    PlayerInventory,
    Toolbelt,
    Campfire,
    Chest,
    Smelter,
}

public abstract class UIWindow : MonoBehaviour
{
    public UIType uiType;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    public bool IsOpen => gameObject.activeSelf;
}
