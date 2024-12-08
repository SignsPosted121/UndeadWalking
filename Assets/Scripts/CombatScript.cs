using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatScript : MonoBehaviour
{
	public float Damage = 3;
	[Range(0, 100)] public float Accuracy = 100;
	[Range(0, 100)] public float CriticalChance = 10;
	public float Range = 1;
	public bool Bullets = false;

	public Transform Weapon;
	public Transform Inventory;

	private Transform GameManager;
	private Transform Enemies;

	private Transform CanSeeTarget(Transform targetA, Transform targetB, float Distance)
	{
		Transform found = null;
		if ((targetA.position - targetB.position).magnitude > Distance)
		{
			return null;
		}
		Transform[] obstaclesHit = new Transform[20];
		int obCount = 0;
		for (float y = -0.4f; y < 1; y += 0.8f)
		{
			for (float x = -0.4f; x < 1; x += 0.8f)
			{
				Vector3 positioning = targetA.position + new Vector3(x, y, 0);
				int layerMask = LayerMask.GetMask("ObstacleSight");
				RaycastHit2D hit = Physics2D.Raycast(positioning, (targetB.position - positioning).normalized, (targetB.position - positioning).magnitude, layerMask);
				if (hit && hit.transform.tag == "ObstacleSight")
				{
					bool superFound = false;
					for (int i = 0; i < 20; i++)
					{
						if (obstaclesHit[i] == hit.transform)
						{
							superFound = true;
						}
					}
					if (!superFound)
					{
						DamageTarget(hit.transform, false, 0);
						obstaclesHit[obCount] = hit.transform;
						obCount += 1;
					}
				}

				layerMask = LayerMask.GetMask("Entity", "Obstacle");
				hit = Physics2D.Raycast(positioning, (targetB.position - positioning).normalized, (targetB.position - positioning).magnitude, layerMask);
				if (!hit || hit == targetB)
				{
					found = targetB;
				}
				else if (!found && hit.transform.tag == "Entity")
				{
					found = hit.transform;
				}
			}
		}
		return found;
	}

	private void DamageTarget(Transform target, bool isEntity, float turnTake)
	{
		GameManager.GetComponent<GameManagerScript>().Actions -= turnTake;
		if (Random.Range(0, 99) <= Accuracy)
		{
			if (isEntity)
			{
				target.GetComponent<EntityScript>().Damage(Mathf.Floor(Random.Range(Damage * 0.75f, Damage * 1.25f) + 0.5f));
				if (Random.Range(0, 99) <= CriticalChance)
				{
					target.GetComponent<EntityScript>().Damage(Mathf.Floor(Random.Range(Damage * 0.75f, Damage * 1.25f) + 0.5f));
				}
			} else
			{
				target.GetComponent<DestructableObject>().Damage(Mathf.Floor(Random.Range(Damage * 0.75f, Damage * 1.25f) + 0.5f));
				if (Random.Range(0, 99) <= CriticalChance)
				{
					target.GetComponent<DestructableObject>().Damage(Mathf.Floor(Random.Range(Damage * 0.75f, Damage * 1.25f) + 0.5f));
				}
			}
		}
	}

	private bool IsMouseOverUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}

	private bool ConsumeBullet(bool consume)
	{
		bool hasBullets = false;
		for (int i = 0; i < Inventory.childCount; i++)
		{
			ItemScript itemStats = Inventory.GetChild(i).GetComponent<ItemScript>();
			if (itemStats.ItemName == "Bullet")
			{
				if (consume)
				{
					itemStats.Amount -= 1;
					GameManager.Find("Sounds").Find("Attacks").Find("Gunshot").GetComponent<AudioSource>().Play();
				}
				hasBullets = true;
				if (itemStats.Amount <= 0)
				{
					Destroy(Inventory.GetChild(i).gameObject);
				}
			}
		}
		if (hasBullets)
		{
			return true;
		} else
		{
			return false;
		}
	}

	void Awake()
	{
		GameManager = GameObject.FindGameObjectWithTag("GameController").transform;
		Enemies = GameManager.Find("Enemies");
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !IsMouseOverUI() && (!Bullets || (Bullets && ConsumeBullet(false))))
		{
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			float distance = (mousePosition - transform.position).magnitude;
			Vector3 mouseDirection = (new Vector3(mousePosition.x, mousePosition.y, 0) - transform.position).normalized;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, mouseDirection, distance, LayerMask.GetMask("Entity"));
			RaycastHit2D hitFloor = Physics2D.Raycast(mousePosition, mouseDirection, 0.001f);
			float Eyesight = transform.GetComponent<CombatScript>().Range + 0.5f;
			bool didDamage = false;
			if (hitFloor && hitFloor.transform.name == "Floor")
			{
				return;
			}
			if (hit && CanSeeTarget(transform, hit.transform, Eyesight))
			{
				if (CanSeeTarget(transform, hit.transform, Eyesight).IsChildOf(Enemies) && GameManager.GetComponent<GameManagerScript>().Actions >= 1)
				{
					didDamage = true;
					DamageTarget(hit.transform, true, 1);
					if (Bullets)
					{
						ConsumeBullet(true);
					}
					else if (Weapon != null)
					{
						GameManager.Find("Sounds").Find("Attacks").Find(Weapon.GetComponent<ItemScript>().ItemName).GetComponent<AudioSource>().Play();
					}
				}
			}
			if (!didDamage)
			{
				hit = Physics2D.Raycast(transform.position, mouseDirection, distance, LayerMask.GetMask("ObstacleSight", "BigObject"));
				if (hit)
				{
					if (CanSeeTarget(transform, hit.transform, Eyesight) && GameManager.GetComponent<GameManagerScript>().Actions >= 1)
					{
						DamageTarget(hit.transform, false, 1);
						if (Bullets)
						{
							ConsumeBullet(true);
						}
						else if (Weapon != null)
						{
							GameManager.Find("Sounds").Find("Attacks").Find(Weapon.GetComponent<ItemScript>().ItemName).GetComponent<AudioSource>().Play();
						}
					}
				}
			}
		}
	}
}
