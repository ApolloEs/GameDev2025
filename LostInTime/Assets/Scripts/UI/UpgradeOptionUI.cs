using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Button button;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Visual Styling")]
    [SerializeField] private Image backgroundPanel;

    [Header("Hover Effects")]
    [SerializeField] private float hoverScaleAmount = 1.1f;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.localScale = originalScale * hoverScaleAmount;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //transform.localScale = originalScale;
    }

    public void SetUpgradeInfo(Upgrade upgrade)
    {
        // Set basic info
        titleText.text = upgrade.upgradeName.ToUpper();
        descriptionText.text = upgrade.description;

        // Set icon
        if (upgrade.icon != null)
        {
            upgradeIcon.sprite = upgrade.icon;
        }

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
