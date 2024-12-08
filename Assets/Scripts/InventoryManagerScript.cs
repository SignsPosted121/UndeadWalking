using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManagerScript : MonoBehaviour
{

	public EntityScript PlayerStats;
	public Transform Inspector;
	public Transform Slots;
	public Transform Items;
	public Transform Loot;

	private Transform currentLoot;
	private Transform gameManager;

	public void DropItem(ItemScript stats)
	{
		if (currentLoot)
		{
			bool foundItem = false;
			for (int i = 0; i < currentLoot.childCount; i++)
			{
				if (currentLoot.GetChild(i).GetComponent<ItemScript>().ItemID == stats.ItemID)
				{
					if (PlayerStats.GetComponent<CombatScript>().Weapon == stats)
					{
						UseWeapon(stats, false);
					}
					currentLoot.GetChild(i).GetComponent<ItemScript>().Amount += stats.Amount;
					ClearInspectionWindow();
					stats.Amount -= stats.Amount;
					foundItem = true;
				}
			}
			if (!foundItem)
			{
				Transform newItem = Instantiate((GameObject)Resources.Load("Items/" + stats.ItemID)).transform;
				newItem.SetParent(currentLoot);
				newItem.GetComponent<ItemScript>().Amount = stats.Amount;
				stats.Amount -= stats.Amount;
				StartCoroutine(ReorganizeLoot(currentLoot));
			}
		} else
		{
			Transform newLoot = Instantiate((GameObject)Resources.Load("Lootbag")).transform;
			newLoot.SetParent(gameManager.GetComponent<GameManagerScript>().Loots);
			newLoot.position = PlayerStats.transform.position;
			newLoot.name = "Lootbag";
			currentLoot = newLoot;
			Transform newItem = Instantiate((GameObject)Resources.Load("Items/" + stats.ItemID)).transform;
			newItem.SetParent(currentLoot);
			newItem.GetComponent<ItemScript>().Amount = stats.Amount;
			stats.Amount -= stats.Amount;
			StartCoroutine(ReorganizeLoot(currentLoot));
		}
		if (stats.Amount <= 0)
		{
			Destroy(stats.gameObject);
		}
		StartCoroutine(ReorganizeItems());
		ClearInspectionWindow();
	}

	public void Consume(ItemScript stats)
	{
		PlayerStats.Water += stats.WaterValue;
		PlayerStats.Food += stats.FoodValue;
		PlayerStats.Health += stats.HealthValue;
		if (PlayerStats.Health > PlayerStats.MaxHealth)
		{
			PlayerStats.Health = PlayerStats.MaxHealth;
		}
		if (stats.Conversion != "")
		{
			AddItem(stats.Conversion, 1, null);
		}
		stats.Amount -= 1;
		if (stats.Amount <= 0)
		{
			Destroy(stats.gameObject);
			ClearInspectionWindow();
			StartCoroutine(ReorganizeItems());
		}
		else
		{
			Inspect(stats.transform);
		}
	}

	public void UseWeapon(ItemScript stats, bool equipping)
	{
		CombatScript cStats = PlayerStats.gameObject.GetComponent<CombatScript>();
		if (equipping)
		{
			cStats.Damage = stats.Damage;
			cStats.Accuracy = stats.Accuracy;
			cStats.CriticalChance = stats.CriticalChance;
			cStats.Range = stats.Range;
			cStats.Bullets = stats.Bullets;
			cStats.Weapon = stats.transform;
		}
		else
		{
			cStats.Damage = 1;
			cStats.Accuracy = 100;
			cStats.CriticalChance = 1;
			cStats.Range = 1;
			cStats.Bullets = false;
			cStats.Weapon = null;
		}
		Inspect(stats.transform);
	}

	public void AddItem(string itemID, float amount, Transform original)
	{
		Transform foundItem = null;
		if (original)
		{
			Destroy(currentLoot.GetChild(original.GetSiblingIndex()).gameObject);
			StartCoroutine(ReorganizeLoot(currentLoot));
			ClearInspectionWindow();
		}
		for (int i = 0; i < Items.childCount; i++)
		{
			if (Items.GetChild(i).GetComponent<ItemScript>().ItemID == itemID)
			{
				foundItem = Items.GetChild(i);
				if (Items.GetChild(i).GetComponent<ItemScript>().ItemType == "Firearm" || Items.GetChild(i).GetComponent<ItemScript>().ItemType == "Melee")
				{
					foundItem = null;
				}
			}
		}
		if (foundItem)
		{
			foundItem.GetComponent<ItemScript>().Amount += amount;
		}
		else
		{
			Transform newItem = Instantiate((GameObject)Resources.Load("Items/" + itemID)).transform;
			newItem.SetParent(Items);
			newItem.GetComponent<ItemScript>().Amount = amount;
		}
		StartCoroutine(ReorganizeItems());
	}

	// Inspection and cleanup

	private void ClearInspectionWindow()
	{
		for (int i = 0; i < Inspector.Find("Type").childCount; i++)
		{
			Inspector.Find("Type").GetChild(i).GetComponent<Image>().enabled = false;
		}
		for (int i = 0; i < Inspector.childCount; i++)
		{
			if (Inspector.GetChild(i).GetComponent<Button>())
			{
				Inspector.GetChild(i).localScale = new Vector3(0, 1, 1);
				Inspector.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
			}
			if (Inspector.GetChild(i).GetComponent<Text>())
			{
				Inspector.GetChild(i).localScale = new Vector3(0, 1, 1);
			}
		}
	}

	private IEnumerator ReorganizeItems()
	{
		yield return new WaitForEndOfFrame();
		float x = 0.05f;
		float y = 0.725f;
		for (int i = 0; i < Items.childCount; i++)
		{
			if (Items.GetChild(i).GetComponent<ItemScript>().Amount > 0)
			{
				Transform currentItem = Items.GetChild(i);
				currentItem.Find("Text").GetComponent<Text>().text = currentItem.GetComponent<ItemScript>().ItemName;
				currentItem.name = "Item" + i;
				currentItem.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
				currentItem.GetComponent<RectTransform>().anchorMax = new Vector2(x + 0.3f, y + 0.1f);
				currentItem.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
				currentItem.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
				currentItem.GetComponent<Button>().onClick.RemoveAllListeners();
				currentItem.GetComponent<Button>().onClick.AddListener(() => Inspect(currentItem));
				x += 0.3f;
				if (x >= 0.9f)
				{
					x = 0.05f;
					y -= 0.1f;
				}
			}
		}
		x = 0.05f;
		y = 0.725f;
		float slotCount = 6;
		for (int i = 1; i < 25; i++)
		{
			if (Slots.childCount - 1 < i)
			{
				Transform newSlot = Instantiate(Slots.GetChild(0)).transform;
				newSlot.SetParent(Slots);
				newSlot.name = "Slot" + i;
			}
			Slots.GetChild(i).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
			if (i - 1 < slotCount)
			{
				Slots.GetChild(i).GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1);
			}
			Slots.GetChild(i).GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
			Slots.GetChild(i).GetComponent<RectTransform>().anchorMax = new Vector2(x + 0.3f, y + 0.1f);
			Slots.GetChild(i).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			Slots.GetChild(i).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			x += 0.3f;
			if (x >= 0.9f)
			{
				x = 0.05f;
				y -= 0.1f;
			}
		}
	}
	// Also destroys lootbags open full looting \\//
	private IEnumerator ReorganizeLoot(Transform lootTable)
	{
		yield return new WaitForEndOfFrame();
		transform.Find("Lootscreen_Text").GetComponent<Text>().text = lootTable.name;
		for (int i = 0; i < Loot.childCount; i++)
		{
			Destroy(Loot.GetChild(i).gameObject);
		}
		if (lootTable.childCount <= 0 && lootTable.name == "Lootbag")
		{
			Destroy(lootTable.gameObject);
			currentLoot = null;
		}
		float x = 0.05f;
		float y = 0.725f;
		for (int i = 0; i < lootTable.childCount; i++)
		{
			ItemScript stats = lootTable.GetChild(i).GetComponent<ItemScript>();
			Transform newItem = Instantiate((GameObject)Resources.Load("Items/" + stats.ItemID)).transform;
			ItemScript itemStats = newItem.GetComponent<ItemScript>();
			newItem.SetParent(Loot);
			newItem.Find("Text").GetComponent<Text>().text = newItem.GetComponent<ItemScript>().ItemName;
			newItem.name = "Item" + i;
			newItem.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
			newItem.GetComponent<RectTransform>().anchorMax = new Vector2(x + 0.3f, y + 0.1f);
			newItem.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			newItem.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			newItem.GetComponent<Button>().onClick.AddListener(() => Inspect(newItem));
			itemStats.Amount = stats.Amount;
			x += 0.3f;
			if (x >= 0.9f)
			{
				x = 0.05f;
				y -= 0.1f;
			}
		}
	}

	// Biggest function, inspects an item

	public void Inspect(Transform item)
	{
		ItemScript stats = item.GetComponent<ItemScript>();
		ClearInspectionWindow();
		Inspector.Find("ItemName").localScale = new Vector3(1, 1, 1);
		Inspector.Find("ItemName").GetComponent<Text>().text = stats.ItemName;
		Inspector.Find("Amount").localScale = new Vector3(1, 1, 1);
		Inspector.Find("Amount").GetComponent<Text>().text = "Amount: " + stats.Amount;
		switch (stats.ItemType)
		{
			case "Component":
				Inspector.Find("Type").Find("Component").GetComponent<Image>().enabled = true;
				break;
			case "Consumable":
				Inspector.Find("Type").Find("Consumable").GetComponent<Image>().enabled = true;
				break;
			case "Health":
				Inspector.Find("Type").Find("Health").GetComponent<Image>().enabled = true;
				break;
			case "Firearm":
				Inspector.Find("Type").Find("Firearm").GetComponent<Image>().enabled = true;
				break;
			case "Melee":
				Inspector.Find("Type").Find("Melee").GetComponent<Image>().enabled = true;
				break;
		}
		if (item.IsChildOf(transform.Find("Items")))
		{
			Inspector.Find("Action2").localScale = new Vector3(1, 1, 1);
			Inspector.Find("Action2").Find("Text").GetComponent<Text>().text = "Drop";
			Inspector.Find("Action2").GetComponent<Button>().onClick.AddListener(() => DropItem(stats));
			switch (stats.ItemType)
			{
				case "Component":
					Inspector.Find("Action1").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Craft";
					break;
				case "Consumable":
					Inspector.Find("Action1").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Consume";
					Inspector.Find("Action1").GetComponent<Button>().onClick.AddListener(() => Consume(stats));
					break;
				case "Health":
					Inspector.Find("Action1").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Use";
					break;
				case "Firearm":
					Inspector.Find("Action1").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Equip";
					Inspector.Find("Action1").GetComponent<Button>().onClick.AddListener(() => UseWeapon(stats, true));
					if (PlayerStats.transform.GetComponent<CombatScript>().Weapon == item)
					{
						Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Unequip";
						Inspector.Find("Action1").GetComponent<Button>().onClick.AddListener(() => UseWeapon(stats, false));
					}
					Inspector.Find("Action3").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action3").Find("Text").GetComponent<Text>().text = "Craft";
					break;
				case "Melee":
					Inspector.Find("Action1").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Equip";
					Inspector.Find("Action1").GetComponent<Button>().onClick.AddListener(() => UseWeapon(stats, true));
					if (PlayerStats.transform.GetComponent<CombatScript>().Weapon == item)
					{
						Inspector.Find("Action1").Find("Text").GetComponent<Text>().text = "Unequip";
						Inspector.Find("Action1").GetComponent<Button>().onClick.AddListener(() => UseWeapon(stats, false));
					}
					Inspector.Find("Action3").localScale = new Vector3(1, 1, 1);
					Inspector.Find("Action3").Find("Text").GetComponent<Text>().text = "Craft";
					break;
			}
		}
		else
		{
			Inspector.Find("Action3").localScale = new Vector3(1, 1, 1);
			Inspector.Find("Action3").Find("Text").GetComponent<Text>().text = "Pick Up";
			Inspector.Find("Action3").GetComponent<Button>().onClick.AddListener(() => AddItem(stats.ItemID, stats.Amount, item));
		}
		int CurrentValue = 1;
		if (stats.FoodValue > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Calories: " + (int)stats.FoodValue * 500;
			CurrentValue += 1;
		}
		if (stats.WaterValue > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Liters: " + (int)stats.WaterValue;
			CurrentValue += 1;
		}
		if (stats.HealthValue > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Heals: " + (int)stats.HealthValue + "HP";
		}
		if (stats.Damage > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Damage: " + (int)stats.Damage + "HP";
			CurrentValue += 1;
		}
		if (stats.Durability > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Durability: " + (int)stats.Durability + "/" + (int)stats.MaxDurability;
			CurrentValue += 1;
		}
		if (stats.Accuracy > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Accuracy: " + (int)stats.Accuracy + "%";
			CurrentValue += 1;
		}
		if (stats.CriticalChance > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Critical Chance: " + (int)stats.CriticalChance + "%";
			CurrentValue += 1;
		}
		if (stats.Range > 0)
		{
			Inspector.Find("Value" + CurrentValue).localScale = new Vector3(1, 1, 1);
			Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Range: " + (int)stats.Range + " Squares";
			if (stats.Range == 1)
			{
				Inspector.Find("Value" + CurrentValue).GetComponent<Text>().text = "Range: 1 Square";
			}
		}
	}

	private Transform lastLoot;

	private void FixedUpdate()
	{
		StartCoroutine(ReorganizeItems());
		currentLoot = null;
		Transform lootPlaces = gameManager.Find("Loot");
		for (int i = 0; i < lootPlaces.childCount; i++)
		{
			if ((lootPlaces.GetChild(i).position - PlayerStats.transform.position).magnitude <= 0.5f)
			{
				currentLoot = lootPlaces.GetChild(i);
			}
		}
		if (currentLoot && lastLoot != currentLoot)
		{
			Loot.gameObject.SetActive(true);
			transform.Find("Lootscreen_Text").gameObject.SetActive(true);
			StartCoroutine(ReorganizeLoot(currentLoot));
			lastLoot = currentLoot;
		}
		if (!currentLoot)
		{
			Loot.gameObject.SetActive(false);
			transform.Find("Lootscreen_Text").gameObject.SetActive(false);
			lastLoot = null;
		}
	}

	private void Awake()
	{
		gameManager = GameObject.FindGameObjectWithTag("GameController").transform;
		PlayerStats = transform.parent.parent.GetComponent<MenuManagementScript>().Player.GetComponent<EntityScript>();
		for (int i = 0; i < Items.childCount; i++)
		{
			Items.GetChild(i).Find("Text").GetComponent<Text>().text = Items.GetChild(i).GetComponent<ItemScript>().ItemName;
		}
		StartCoroutine(ReorganizeItems());
	}
}
