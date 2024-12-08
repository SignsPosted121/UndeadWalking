using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{

	public float MaxHealth;
	public float Health;
	public bool DoDestroy = true;
	public string item1 = "";
	public int item1Max = 1;
	public float item1Chance = 0;
	public string item2 = "";
	public int item2Max = 1;
	public float item2Chance = 0;

	public void Damage(float damage)
	{
		Health -= damage;
		if (damage < 0)
		{
			if (Health > MaxHealth)
			{
				Health = MaxHealth;
			}
		}
		if (damage > 0)
		{
			if (Health > 0)
			{
				StartCoroutine(PushNotification("Damage"));
			} else
			{
				StartCoroutine(PushNotification("Death"));
				gameObject.GetComponent<SpriteRenderer>().enabled = false;
				gameObject.GetComponent<BoxCollider2D>().enabled = false;
				gameObject.GetComponent<AudioSource>().Play();
				// Spawns items to be dropped from destruction
				Transform currentLoot = null;
				if (item1 != "")
				{
					if (Random.Range(0, 99) <= item1Chance)
					{
						Transform lootTable = transform.parent.parent.Find("Loot");
						for (int i = 0; i < lootTable.childCount; i++)
						{
							if (lootTable.GetChild(i).position.x == transform.position.x && lootTable.GetChild(i).position.y == transform.position.y)
							{
								currentLoot = lootTable.GetChild(i);
							}
						}
						if (!currentLoot)
						{
							currentLoot = Instantiate((GameObject)Resources.Load("Lootbag")).transform;
							currentLoot.SetParent(lootTable);
							currentLoot.position = new Vector3(transform.position.x, transform.position.y, 0);
							currentLoot.name = "Lootbag";
						}
						Transform newLoot = Instantiate((GameObject)Resources.Load("Items/" + item1)).transform;
						newLoot.SetParent(currentLoot);
						newLoot.GetComponent<ItemScript>().Amount = Random.Range(1, item1Max);
					}
				}
				if (item2 != "")
				{
					if (Random.Range(0, 99) <= item2Chance)
					{
						Transform lootTable = transform.parent.parent.Find("Loot");
						for (int i = 0; i < lootTable.childCount; i++)
						{
							if (lootTable.GetChild(i).position.x == transform.position.x && lootTable.GetChild(i).position.y == transform.position.y)
							{
								currentLoot = lootTable.GetChild(i);
							}
						}
						if (!currentLoot)
						{
							currentLoot = Instantiate((GameObject)Resources.Load("Lootbag")).transform;
							currentLoot.SetParent(lootTable);
							currentLoot.position = new Vector3(transform.position.x, transform.position.y, 0);
							currentLoot.name = "Lootbag";
						}
						Transform newLoot = Instantiate((GameObject)Resources.Load("Items/" + item2)).transform;
						newLoot.SetParent(currentLoot);
						newLoot.GetComponent<ItemScript>().Amount = Random.Range(1, item2Max);
					}
				}
			}
		}
	}

	IEnumerator PushNotification(string noteType)
	{
		Transform note = Instantiate((GameObject)Resources.Load("Notifications/" + noteType)).transform;
		note.SetParent(transform);
		float timer = 0;
		note.position = transform.position + new Vector3(Random.Range(-5, 5) / 10, 1, 0);
		switch (noteType)
		{
			case "Damage":
				note.position = transform.position;
				timer = 1.5f;
				break;
			case "Death":
				note.SetParent(transform.parent);
				note.position = transform.position;
				timer = 1;
				break;
		}
		while (timer < 2)
		{
			note.position += new Vector3(0, Time.deltaTime / 5, 0);
			note.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, Time.deltaTime / 2);
			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		Destroy(note.gameObject);
		if (noteType == "Death" && DoDestroy)
		{
			Destroy(gameObject);
		}
	}

}
