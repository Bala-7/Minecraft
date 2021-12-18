using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s;

    private Image accessibleObjectSelector;
    private Image inventoryShortBackground;
    private Image[] shortInventoryIcons;
    private Image[] fullInventoryIcons;
    private Image[] shortIconsOnFullInventory;
    Transform shortIconsParent;
    private Image fullInventoryImage;
    public List<Sprite> cellIcons;

    private Image loadingScreen;

    private void Awake()
    {
        if (s)
            Destroy(this.gameObject);
        else s = this;

        InitializeInventoryIcons();
        loadingScreen = transform.Find("LoadingScreen").GetComponent<Image>();
    }

    private void Update()
    {
        
    }

    private void InitializeInventoryIcons()
    {
        inventoryShortBackground = transform.Find("InventoryShort").GetComponent<Image>();
        accessibleObjectSelector = inventoryShortBackground.transform.Find("CurrentObjectFrameIcon").GetComponent<Image>();
        shortInventoryIcons = new Image[9];
        Transform inventoryShort = inventoryShortBackground.transform.Find("Icons");
        for (int i = 0; i < inventoryShort.childCount; ++i)
        {
            Transform child = inventoryShort.GetChild(i);
            if (child.name.Contains("InventoryIcon")) 
            {
                shortInventoryIcons[i] = child.GetComponent<Image>();
                shortInventoryIcons[i].enabled = false;
                shortInventoryIcons[i].GetComponentInChildren<Text>().enabled = false;
            }
        }

        fullInventoryImage = transform.Find("InventoryFull").GetComponent<Image>();
        fullInventoryImage.enabled = false;
        shortIconsOnFullInventory = new Image[9];
        shortIconsParent = fullInventoryImage.transform.Find("ShortInventoryIcons");
        for (int i = 0; i < shortIconsParent.childCount; ++i)
        {
            Transform child = shortIconsParent.GetChild(i);
            if (child.name.Contains("InventoryIcon"))
            {
                shortIconsOnFullInventory[i] = child.GetComponent<Image>();
                shortIconsOnFullInventory[i].enabled = false;
                shortIconsOnFullInventory[i].GetComponentInChildren<Text>().enabled = false;
            }
        }

    }

    public void BlockAddedToInventory(InventoryObjectBlock block, int position, int currentStack) 
    {
        shortInventoryIcons[position].sprite = cellIcons[(int)block.CellType];
        Text t = shortInventoryIcons[position].GetComponentInChildren<Text>();
        t.text = currentStack.ToString();

        t.enabled = true;
        shortInventoryIcons[position].enabled = true;
    }

    public void ObjectAddedToInventory(InventoryObject.OBJECT_TYPE type, int position, int currentStack) 
    {
        if (type == InventoryObject.OBJECT_TYPE.BLOCK) 
        {
            //InventoryObjectBlock block = 
            //shortInventoryIcons[position].sprite = cellIcons[];
            Text t = shortInventoryIcons[position].GetComponentInChildren<Text>();
            t.text = currentStack.ToString();
            
            t.enabled = true;
            shortInventoryIcons[position].enabled = true;
        }
    }

    public void CurrentObjectChanged() 
    {
        accessibleObjectSelector.GetComponent<RectTransform>().localPosition = new Vector3(-320 + 80 * Inventory.s.CurrentObjectIndex, -496, 0);
    }

    public void CurrentObjectPlaced()
    {
        shortInventoryIcons[Inventory.s.CurrentObjectIndex].GetComponentInChildren<Text>().text = Inventory.s.CurrentObjectStack.ToString();

    }

    public void UpdateAccessibleObjectsUI()
    {
        for (int i = 0; i < Inventory.s.AccessibleObjects.Length; ++i)
        {
            InventoryObjectBlock block = Inventory.s.AccessibleObjects[i] as InventoryObjectBlock;
            if (!ReferenceEquals(block, null) && block.currentStack > 0)
            {
                shortInventoryIcons[i].sprite = cellIcons[(int)block.CellType];
                Text t = shortInventoryIcons[i].GetComponentInChildren<Text>();
                t.text = block.currentStack.ToString();

                t.enabled = true;
                shortInventoryIcons[i].enabled = true;
            }
            else
            {
                Text t = shortInventoryIcons[i].GetComponentInChildren<Text>();
                t.enabled = false;
                shortInventoryIcons[i].enabled = false;
            }
                
        }
    }

    public void FadeOut()
    {
        loadingScreen.CrossFadeAlpha(0, 0, true);
        loadingScreen.canvasRenderer.SetAlpha(1);
    }

    public void FadeIn()
    {
        //loadingScreen.CrossFadeAlpha(1, 3f, true);
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        while (loadingScreen.canvasRenderer.GetAlpha() > 0) 
        {
            float currentAlpha = loadingScreen.canvasRenderer.GetAlpha();
            loadingScreen.canvasRenderer.SetAlpha(currentAlpha - 0.3f * Time.deltaTime);
            yield return null;
        }
        loadingScreen.canvasRenderer.SetAlpha(0);
    }

    public void InventoryKeyPressed() 
    {
        bool isShowingFullInventory = fullInventoryImage.enabled;
        if (!isShowingFullInventory)
        {
            fullInventoryImage.enabled = true;
            for (int i = 0; i < shortIconsOnFullInventory.Length; ++i)
            {
                shortIconsOnFullInventory[i].enabled = shortInventoryIcons[i].enabled;
                shortIconsOnFullInventory[i].sprite = shortInventoryIcons[i].sprite;

                Text shortIconOnFullInventoryText = shortIconsOnFullInventory[i].GetComponentInChildren<Text>();
                Text shortIconText = shortInventoryIcons[i].GetComponentInChildren<Text>();
                shortIconOnFullInventoryText.enabled = shortIconText.enabled;
                shortIconOnFullInventoryText.text = shortIconText.text;
            }
            SetShortInventoryVisibility(false);
        }
        else 
        {
            fullInventoryImage.enabled = false;
            for (int i = 0; i < shortIconsOnFullInventory.Length; ++i)
            {
                shortIconsOnFullInventory[i].enabled = false;
                Text shortIconOnFullInventoryText = shortIconsOnFullInventory[i].GetComponentInChildren<Text>();
                shortIconOnFullInventoryText.enabled = false;
            }
            SetShortInventoryVisibility(true);
        }
    }

    private void SetShortInventoryVisibility(bool visible)
    {
        inventoryShortBackground.enabled = visible;
        accessibleObjectSelector.enabled = visible;

        if (!visible)
        {
            for(int i = 0; i < shortInventoryIcons.Length; ++i)
            {
                Image s = shortInventoryIcons[i];
                s.enabled = false;
                s.GetComponentInChildren<Text>().enabled = false;
            }
        }
        else 
        {
            UpdateAccessibleObjectsUI();
        }
    }
}
