using UnityEngine;
using UnityEngine.UI;

// References point to ordinary editable UI objects in the Arena scene.
public sealed class ActiveSkillBarUI : MonoBehaviour
{
    [SerializeField] private Image qCooldownShade;
    [SerializeField] private Text qCooldownText;

    public void Configure(Image shade, Text label)
    {
        qCooldownShade = shade;
        qCooldownText = label;
    }

    public void SetQCooldown(float remaining, float duration)
    {
        if (qCooldownShade != null)
        {
            qCooldownShade.fillAmount = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
            qCooldownShade.enabled = remaining > 0f;
        }
        if (qCooldownText != null)
        {
            qCooldownText.text = remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : string.Empty;
            qCooldownText.enabled = remaining > 0f;
        }
    }
}
