using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemUseFlash : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private float flashDuration = 0.5f;

    private Coroutine flashRoutine;

    public void Flash(Sprite itemIcon)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DoFlash(itemIcon));
    }

    private IEnumerator DoFlash(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        iconImage.enabled = false;
    }
}
