using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManagerScript : MonoBehaviour
{

	public Transform Player;
	public Transform GameManager;

	void Awake()
	{
		GameManagerScript gStats = GameManager.GetComponent<GameManagerScript>();
		transform.Find("EndTurn").GetComponent<Button>().onClick.AddListener(() => gStats.PerformAction("EndTurn", null, new Vector3()));
	}

	void Update()
    {
		EntityScript pStats = Player.GetComponent<EntityScript>();
		CombatScript cStats = Player.GetComponent<CombatScript>();
		GameManagerScript gStats = GameManager.GetComponent<GameManagerScript>();
		transform.Find("EndTurn").Find("TurnPool").GetComponent<Text>().text = "Actions : " + gStats.Actions + "/" + gStats.MaxActions;
		transform.Find("EndTurn").Find("Bar").GetComponent<Image>().fillAmount = gStats.Actions / gStats.MaxActions;
		if (gStats.Actions <= 0)
		{
			transform.Find("EndTurn").Find("TurnPool").GetComponent<Text>().color = new Color(1, 0, 0, 1);
		}
		else { transform.Find("EndTurn").Find("TurnPool").GetComponent<Text>().color = new Color(1, 1, 1, 1); }
		transform.Find("Health").Find("Text").GetComponent<Text>().text = "Health : " + pStats.Health + "/" + pStats.MaxHealth;
		transform.Find("Health").Find("Bar").GetComponent<Image>().fillAmount = pStats.Health / pStats.MaxHealth;
		transform.Find("Movement").Find("Text").GetComponent<Text>().text = "Movement : " + pStats.Movement + "/" + pStats.MaxMovement;
		transform.Find("Movement").Find("Bar").GetComponent<Image>().fillAmount = pStats.Movement / pStats.MaxMovement;
		transform.Find("Weapon").Find("Ammo").GetComponent<Text>().text = "";
		transform.Find("Weapon").Find("Ammo").GetComponent<Text>().color = new Color(1, 1, 1, 1);
		if (cStats.Weapon)
		{
			transform.Find("Weapon").Find("Text").GetComponent<Text>().text = Player.GetComponent<CombatScript>().Weapon.GetComponent<ItemScript>().ItemName;
			if (cStats.Bullets)
			{
				Transform lootTable = cStats.Inventory;
				float bulletCount = 0;
				for (int i = 0; i < lootTable.childCount; i++)
				{
					if (lootTable.GetChild(i).GetComponent<ItemScript>().ItemID == "Bullet")
					{
						bulletCount += lootTable.GetChild(i).GetComponent<ItemScript>().Amount;
					}
				}
				if (bulletCount > 0)
				{
					transform.Find("Weapon").Find("Ammo").GetComponent<Text>().text = "Bullets : " + bulletCount;
				} else
				{
					transform.Find("Weapon").Find("Ammo").GetComponent<Text>().text = "No Ammo!";
					transform.Find("Weapon").Find("Ammo").GetComponent<Text>().color = new Color(1, 0, 0, 1);
				}
			}
		} else
		{
			transform.Find("Weapon").Find("Text").GetComponent<Text>().text = "Fists";
		}
	}
}
