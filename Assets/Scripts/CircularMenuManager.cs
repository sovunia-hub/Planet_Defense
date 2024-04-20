using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularMenuManager : MonoBehaviour
{
    [Header("GENERAL")]
    public Camera UICamera;
    public GameObject BackgroundPanel;
    public GameObject CircleMenuElementPrefab;
    public bool UseGradient;

    [Header("BUTTONS")]
    public Color NormalButtonColor;
    public Color HighlightedButtonColor;
    public Gradient HighlightedButtonGradient = new Gradient();

    [Header("INFORMAL CENTER")]
    public Image InformalCenterBackground;
    public TMP_Text ItemName;
    public TMP_Text ItemDescription;
    public Image ItemIcon;

    private int currentMenuItemIndex;
    private int previousMenuItemIndex;
    private float calculatedMenuIndex;
    private float currentSelectionAngle;
    private Vector3 currentMousePosition;
    private List<CircularMenuElement> menuElements = new List<CircularMenuElement>();

    private static CircularMenuManager instance;
    public static CircularMenuManager Instance { get { return instance; } }
    public bool Active { get { return BackgroundPanel.activeSelf; } }
    public List<CircularMenuElement> MenuElements
    {
        get { return menuElements; }
        set { menuElements = value; }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {

        float rotationalIncrementalValue = 360f / menuElements.Count;
        float currentRotationValue = 0;
        float fillPercentageValue = 1f / menuElements.Count;

        for (int i = 0; i < menuElements.Count; i++)
        {
            GameObject menuElementGameObject = Instantiate(CircleMenuElementPrefab);
            menuElementGameObject.name = i + ": " + currentRotationValue;
            menuElementGameObject.transform.SetParent(BackgroundPanel.transform);
            MenuButton menuButton = menuElementGameObject.GetComponent<MenuButton>();

            menuButton.Recttransform.localScale = Vector3.one;
            menuButton.Recttransform.localPosition = Vector3.zero;
            menuButton.Recttransform.rotation = Quaternion.Euler(0f, 0f, currentRotationValue);
            currentRotationValue += rotationalIncrementalValue;

            menuButton.BackgroundImage.fillAmount = fillPercentageValue + 0.001f;
            menuElements[i].ButtonBackground = menuButton.BackgroundImage;

            menuButton.IconImage.sprite = menuElements[i].ButtonIcon;
            menuButton.IconRecttransform.rotation = Quaternion.identity;
        }

        BackgroundPanel.SetActive(false);
    }/*!< Create circular menu */

    private void Update()
    {
        if (!Active)
        {
            return;
        }

        GetCurrentMenuElement();
        if (Input.GetMouseButton(0))
        {
            Select();
        }
    }

    public void GetCurrentMenuElement()
    {
        float rotationalIncrementalValue = 360f / menuElements.Count;
        currentMousePosition = new Vector2(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2);

        currentSelectionAngle = 90 + rotationalIncrementalValue + Mathf.Atan2(currentMousePosition.y, currentMousePosition.x) * Mathf.Rad2Deg;
        currentSelectionAngle = (currentSelectionAngle + 360f) % 360f;

        currentMenuItemIndex = (int)(currentSelectionAngle / rotationalIncrementalValue);

        if (currentMenuItemIndex != previousMenuItemIndex)
        {
            menuElements[previousMenuItemIndex].ButtonBackground.color = NormalButtonColor;

            previousMenuItemIndex = currentMenuItemIndex;

            menuElements[currentMenuItemIndex].ButtonBackground.color = UseGradient ? HighlightedButtonGradient.Evaluate(1f / menuElements.Count * currentMenuItemIndex) : HighlightedButtonColor;
            InformalCenterBackground.color = UseGradient ? HighlightedButtonGradient.Evaluate(1f / menuElements.Count * currentMenuItemIndex) : HighlightedButtonColor;
            RefreshInformalCenter();
        }
    }/*!< Get menu element on cursor */

    public void RefreshInformalCenter()
    {
        ItemName.text = menuElements[currentMenuItemIndex].Name;
        ItemDescription.text = menuElements[currentMenuItemIndex].Description;
        ItemIcon.sprite = menuElements[currentMenuItemIndex].ButtonIcon;
    }/*!< Refresh menu center with name, description and icon of current element */

    public void Select()
    {
        BuildingSystem.Instance.SwitchToIndex(currentMenuItemIndex);
        Deactivate();
    }/*!< Instantiate selected building */

    public void Activate()
    {
        if (Active)
        {
            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        BackgroundPanel.SetActive(true);
        RefreshInformalCenter();
    }/*!< Show menu */

    public void Deactivate()
    {
        BackgroundPanel.SetActive(false);
    }/*!< Hide menu */
}
