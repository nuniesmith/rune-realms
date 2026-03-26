using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RuneRealms.Data;
using RuneRealms.Core;

namespace RuneRealms.Inventory
{
    /// <summary>
    /// Manages the player inventory display and interactions.
    /// Attach to the Inventory tab panel.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform slotContainer; // Parent for slot prefabs
        [SerializeField] private GameObject slotPrefab;    // Prefab for each inventory slot
        [SerializeField] private TextMeshProUGUI slotCountText;

        [Header("Item Detail Popup")]
        [SerializeField] private GameObject detailPopup;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailCategory;
        [SerializeField] private TextMeshProUGUI detailQuantity;
        [SerializeField] private Image detailRarityBorder;
        [SerializeField] private Button useButton;
        [SerializeField] private Button closePopupButton;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color rareColor = new Color(0.2f, 0.4f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.6f, 0f);

        private PlayerInventory inventory;
        private List<GameObject> slotObjects = new List<GameObject>();
        private InventoryItem selectedItem;

        public void Initialize(PlayerInventory inv)
        {
            inventory = inv;
            RefreshGrid();

            if (closePopupButton != null)
            {
                closePopupButton.onClick.RemoveAllListeners();
                closePopupButton.onClick.AddListener(CloseDetailPopup);
            }

            if (useButton != null)
            {
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(OnUseItem);
            }

            if (detailPopup != null)
                detailPopup.SetActive(false);
        }

        public void AddItem(InventoryItem item)
        {
            if (inventory == null) return;

            // Try to stack
            var existing = inventory.items.Find(i => i.itemId == item.itemId);
            if (existing != null)
            {
                existing.quantity += item.quantity;
            }
            else if (inventory.items.Count < inventory.maxSlots)
            {
                inventory.items.Add(item);
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] Inventory full, can't add {item.name}");
                return;
            }

            RefreshGrid();
        }

        public void RefreshGrid()
        {
            // Clear existing slots
            foreach (var obj in slotObjects)
            {
                if (obj != null) Destroy(obj);
            }
            slotObjects.Clear();

            if (inventory == null || slotPrefab == null || slotContainer == null) return;

            // Create item slots
            for (int i = 0; i < inventory.maxSlots; i++)
            {
                var slotObj = Instantiate(slotPrefab, slotContainer);
                slotObjects.Add(slotObj);

                var nameText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
                var bgImage = slotObj.GetComponent<Image>();
                var button = slotObj.GetComponent<Button>();
                var quantityText = slotObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

                if (i < inventory.items.Count)
                {
                    var item = inventory.items[i];

                    if (nameText != null)
                        nameText.text = item.name;

                    if (quantityText != null)
                        quantityText.text = item.quantity > 1 ? $"x{item.quantity}" : "";

                    if (bgImage != null)
                        bgImage.color = GetRarityColor(item.rarity);

                    if (button != null)
                    {
                        int index = i;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => ShowItemDetail(inventory.items[index]));
                    }
                }
                else
                {
                    // Empty slot
                    if (nameText != null)
                        nameText.text = "";

                    if (quantityText != null)
                        quantityText.text = "";

                    if (bgImage != null)
                        bgImage.color = new Color(0.15f, 0.12f, 0.08f, 0.5f); // Dark empty slot

                    if (button != null)
                        button.interactable = false;
                }
            }

            // Update slot count
            if (slotCountText != null)
                slotCountText.text = $"{inventory.items.Count}/{inventory.maxSlots} slots";
        }

        private void ShowItemDetail(InventoryItem item)
        {
            if (detailPopup == null) return;

            selectedItem = item;
            detailPopup.SetActive(true);

            if (detailName != null) detailName.text = item.name;
            if (detailCategory != null) detailCategory.text = FormatCategory(item.category);
            if (detailQuantity != null) detailQuantity.text = $"Quantity: {item.quantity}";
            if (detailRarityBorder != null) detailRarityBorder.color = GetRarityColor(item.rarity);

            // Only consumables can be used
            if (useButton != null)
                useButton.interactable = item.category == "consumable";
        }

        private void CloseDetailPopup()
        {
            if (detailPopup != null)
                detailPopup.SetActive(false);
            selectedItem = null;
        }

        private void OnUseItem()
        {
            if (selectedItem == null) return;
            if (selectedItem.category != "consumable") return;

            DevvitBridge.Instance?.UseItem(selectedItem.itemId, response =>
            {
                if (response != null && response.success)
                {
                    inventory = response.inventory;
                    if (GameManager.Instance != null)
                        GameManager.Instance.CurrentInventory = inventory;

                    RefreshGrid();
                    CloseDetailPopup();
                }
            });
        }

        private Color GetRarityColor(string rarity)
        {
            return rarity switch
            {
                "uncommon" => uncommonColor,
                "rare" => rareColor,
                "legendary" => legendaryColor,
                _ => commonColor,
            };
        }

        private string FormatCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return "";
            return char.ToUpper(category[0]) + category.Substring(1);
        }
    }
}
