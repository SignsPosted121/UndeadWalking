using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Android;

public class GameManagerScript : MonoBehaviour
{

	public Transform Enemies;
	public Transform Player;
	public Transform Sounds;
	public Transform Loots;
	public Transform Inventory;
	public float TurnCount = 0;
	public float MaxActions = 1;
	public float Actions = 1;
	public float Waiting = 0;

	private bool CanSeeTarget(Transform targetA, Transform targetB, float Distance)
	{
		bool found = false;
		if ((targetA.position - targetB.position).magnitude > Distance)
		{
			return false;
		}
		for (float y = -0.4f; y < 1; y += 0.8f)
		{
			for (float x = -0.4f; x < 1; x += 0.8f)
			{
				Vector3 positioning = targetA.position + new Vector3(x, y, 0);
				RaycastHit2D hit2 = Physics2D.Raycast(positioning, (targetB.position - positioning).normalized, (targetB.position - positioning).magnitude, 9 << 10);
				if (!hit2 || hit2.transform.tag != "Obstacle")
				{
					found = true;
				}
				else
				{
					found = false;
				}
			}
		}
		return found;
	}

	Vector2[] nodePoints(Vector2 mapSize)
	{
		Vector2[] points = new Vector2[(int)(mapSize.x * mapSize.y)];
		int mapX = -Mathf.FloorToInt(mapSize.x / 2);
		int mapY = -Mathf.FloorToInt(mapSize.y / 2);
		for (int i = 0; i < points.Length; i++)
		{
			points[i] = new Vector2(mapX, mapY);
			mapX += 1;
			if (mapX > mapSize.x / 2)
			{
				mapY += 1;
				mapX = -Mathf.FloorToInt(mapSize.x / 2);
			}
		}
		return points;
	}

	Vector2 FindBestPath(Vector2 start, Vector2 target, Vector2 mapSize)
	{
		Vector2[] localGrid = nodePoints(mapSize);
		float[] nodeFCost = new float[localGrid.Length];
		float[] nodeGCost = new float[localGrid.Length];
		float[] nodeHCost = new float[localGrid.Length];
		int startGridLocation = 0;
		int endGridLocation = 0;
		// Parses through obstacles and deletes their grid coordinates
		for (int i = 0; i < localGrid.Length; i++)
		{
			bool plotted = false;
			if (localGrid[i] == target)
			{
				endGridLocation = i;
			}
			if (localGrid[i] == start)
			{
				startGridLocation = i;
			}
			else
			{
				for (int x = 0; x < Enemies.childCount; x++)
				{
					if (localGrid[i] == new Vector2(Enemies.GetChild(x).position.x, Enemies.GetChild(x).position.y))
					{
						Vector2[] newGrid = new Vector2[localGrid.Length - 1];
						for (int y = 0; y < localGrid.Length - 1; y++)
						{
							if (y < i)
							{
								newGrid[y] = localGrid[y];
							}
							if (y >= i)
							{
								newGrid[y] = localGrid[y + 1];
							}
						}
						localGrid = newGrid;
						i -= 1;
						plotted = true;
					}
				}
			}
			Transform map = transform.Find("Map");
			for (int x = 0; x < map.childCount; x++)
			{
				if (!plotted)
				{
					SpriteRenderer sprite = map.GetChild(x).GetComponent<SpriteRenderer>();
					if (localGrid[i].x > map.GetChild(x).position.x - sprite.size.x / 2 && localGrid[i].x < map.GetChild(x).position.x + sprite.size.x / 2)
					{
						if (localGrid[i].y > map.GetChild(x).position.y - sprite.size.y / 2 && localGrid[i].y < map.GetChild(x).position.y + sprite.size.y / 2)
						{
							Vector2[] newGrid = new Vector2[localGrid.Length - 1];
							for (int y = 0; y < localGrid.Length - 1; y++)
							{
								if (y < i)
								{
									newGrid[y] = localGrid[y];
								}
								if (y >= i)
								{
									newGrid[y] = localGrid[y + 1];
								}
							}
							localGrid = newGrid;
							i -= 1;
							plotted = true;
						}
					}
				}
			}
		}
		// Find the best path
		bool found = false;
		for (int i = 0; i < localGrid.Length; i++)
		{
			nodeFCost[i] = Mathf.Infinity;
			nodeGCost[i] = Mathf.Infinity;
			nodeHCost[i] = Mathf.Infinity;
		}
		nodeFCost[startGridLocation] = 0;
		nodeGCost[startGridLocation] = Mathf.Floor((start - target).magnitude * 10);
		nodeHCost[startGridLocation] = Mathf.Floor((start - target).magnitude * 10);
		bool[] parsed = new bool[localGrid.Length];
		for (int i = 0; i < localGrid.Length; i++)
		{
			parsed[i] = false;
		}
		float BreakCount = 0;
		int cheapest = startGridLocation;
		while (!found)
		{
			if (BreakCount > 0)
			{
				cheapest = -1;
			}
			for (int i = 0; i < localGrid.Length; i++)
			{
				if (!parsed[i])
				{
					if (cheapest == -1 || nodeHCost[i] < nodeHCost[cheapest])
					{
						cheapest = i;
					}
					if (nodeHCost[i] == nodeHCost[cheapest] && nodeGCost[i] < nodeGCost[cheapest])
					{
						cheapest = i;
					}
				}
			}
			if (cheapest != -1)
			{
				parsed[cheapest] = true;
				for (int i = 0; i < localGrid.Length; i++)
				{
					BreakCount += 1;
					if (i != cheapest && (localGrid[i] - localGrid[cheapest]).magnitude <= 1.5f)
					{
						float potentialCost = 14f;
						if ((localGrid[i] - localGrid[cheapest]).magnitude < 1.2f)
						{
							potentialCost = 10f;
						}
						if (nodeFCost[i] > potentialCost + nodeFCost[cheapest])
						{
							nodeFCost[i] = potentialCost + nodeFCost[cheapest];
						}
						nodeGCost[i] = Mathf.Floor((localGrid[i] - target).magnitude * 10);
						nodeHCost[i] = nodeFCost[i] + nodeGCost[i];
						if (nodeGCost[i] <= 0)
						{
							found = true;
							endGridLocation = i;
						}
					}
				}
			} else
			{
				found = true;
				endGridLocation = startGridLocation;
			}
			if (BreakCount > 10000)
			{
				found = true;
			}
		}
		// Finally get the closest point along the path
		int currentChoice = endGridLocation;
		found = false;
		while (!found)
		{
			int bestChoice = currentChoice;
			for (int i = 0; i < localGrid.Length; i++)
			{
				BreakCount += 1;
				if (!found && (localGrid[bestChoice] - localGrid[i]).magnitude < 2)
				{
					if (nodeHCost[i] <= nodeHCost[bestChoice])
					{
						if (nodeFCost[i] < nodeFCost[bestChoice])
						{
							bestChoice = i;
						}
					}
					if ((start - localGrid[bestChoice]).magnitude <= 1.5f)
					{
						found = true;
					}
				}
				if (BreakCount > 10000)
				{
					found = true;
					bestChoice = startGridLocation;
				}
			}
			currentChoice = bestChoice;
		}
		return localGrid[currentChoice];
	}

	IEnumerator PassTurn()
	{
		for (int i = 0; i < Enemies.childCount; i++)
		{
			Transform zombie = Enemies.GetChild(i);
			EntityScript zStats = zombie.GetComponent<EntityScript>();
			float damage = Mathf.Floor(Random.Range(zStats.BaseDamage * 0.5f, zStats.BaseDamage * 1.5f) + 0.5f);
			if (CanSeeTarget(zombie, Player, zombie.GetComponent<EntityScript>().Eyesight + 0.5f))
			{
				if (zStats.Tracking)
				{
					bool attacked = false;
					for (int x = 0; x < zStats.Movement; x++)
					{
						Vector3 movingPosition = FindBestPath(Enemies.GetChild(i).position, Player.position, new Vector2(17, 12));
						float dis = (movingPosition - zombie.position).magnitude;
						int layerMask = LayerMask.GetMask("BigObject");
						RaycastHit2D hit = Physics2D.Raycast(zombie.position, (movingPosition - zombie.position).normalized, dis + 0.1f, layerMask);
						if (hit && (new Vector2(zombie.position.x, zombie.position.y) - hit.point).magnitude <= 1.5f)
						{
							if (hit.transform.GetComponent<DestructableObject>() && hit.transform.GetComponent<DestructableObject>().Health > 0)
							{
								hit.transform.GetComponent<DestructableObject>().Damage(damage);
								attacked = true;
							}
						}
						if (!attacked && (zombie.position - Player.position).magnitude <= 1.5f)
						{
							Player.GetComponent<EntityScript>().Damage(damage);
							attacked = true;
						}
						if ((zombie.position - Player.position).magnitude > 1.5f && !attacked)
						{
							Enemies.GetChild(i).position = movingPosition;
						}
					}
				}
				if (!zStats.Tracking)
				{
					zStats.Notification("Spot");
					Player.GetComponent<EntityScript>().Notification("Spotted");
					Sounds.Find("Spotted").GetComponent<AudioSource>().Play();
					zStats.Tracking = true;
				}
			}
			if (!CanSeeTarget(zombie, Player, zombie.GetComponent<EntityScript>().Eyesight + 0.5f) && Random.Range(0, 1) == 0)
			{
				zStats.Tracking = false;
				Vector3 newPosition = zombie.position + new Vector3(Mathf.Sin(Random.Range(0, 2 * Mathf.PI)), Mathf.Cos(Random.Range(0, 2 * Mathf.PI)), 0);
				RaycastHit2D hit = Physics2D.Raycast(zombie.position, (newPosition - zombie.position).normalized, (newPosition - zombie.position).magnitude, 1 << 9);
				if (!hit || (hit.transform.tag != "Obstacle" && hit.transform.tag != "ObstacleSight"))
				{
					if ((zombie.position - newPosition).normalized.x > 0)
					{
						zombie.localScale = new Vector3(-1, 1, 1);
					}
					if ((zombie.position - newPosition).normalized.x < 0)
					{
						zombie.localScale = new Vector3(1, 1, 1);
					}
					zombie.position = new Vector3(Mathf.Floor(newPosition.x + 0.5f), Mathf.Floor(newPosition.y + 0.5f), 0);
				}
				if (CanSeeTarget(zombie, Player, zombie.GetComponent<EntityScript>().Eyesight + 0.5f))
				{
					zStats.Notification("Spot");
					Player.GetComponent<EntityScript>().Notification("Spotted");
					Sounds.Find("Spotted").GetComponent<AudioSource>().Play();
					zStats.Tracking = true;
				}
			}
		}
		yield return new WaitForSeconds(0);
		Waiting += 1;
		if (Waiting >= 5 || (Waiting >= 4 && TurnCount > 5) || (Waiting >= 3 && TurnCount > 12))
		{
			Waiting = 0;
			Transform Enemy = Instantiate((GameObject)Resources.Load("ZombieNormal0")).transform;
			Enemy.SetParent(Enemies);
			Enemy.position = new Vector2(Random.Range(-1, 6), 4);
		}
		TurnCount += 1;
		EntityScript masterStats = Player.GetComponent<EntityScript>();
		Actions = MaxActions;
		masterStats.Movement = masterStats.MaxMovement;
	}

	public void ClearArea()
	{
		for (int i = 0; i < transform.Find("Hidden").childCount; i++)
		{
			Transform fog = transform.Find("Hidden").GetChild(i);
			bool found = false;
			if ((fog.position - Player.position).magnitude <= Player.GetComponent<EntityScript>().Eyesight + 0.5f)
			{
				for (float y = -0.4f; y < 1; y += 0.8f)
				{
					for (float x = -0.4f; x < 1; x += 0.8f)
					{
						Vector3 positioning = Player.position + new Vector3(x, y, 0);
						Vector3 fogPosition = fog.position + new Vector3(x, y, 0);
						RaycastHit2D hit = Physics2D.Raycast(positioning, (fogPosition - positioning).normalized, (fogPosition - positioning).magnitude, 9 << 9);
						if (!hit || hit.transform.tag != "Obstacle")
						{
							found = true;
							fog.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
							fog.Find("Mask").GetComponent<SpriteMask>().enabled = false;
						}
						if (hit && hit.transform.tag == "Obstacle")
						{
							Transform obstacle = hit.transform;
							SpriteRenderer render = obstacle.GetComponent<SpriteRenderer>();
							if (fogPosition.x > obstacle.position.x - render.size.x / 2 && fogPosition.x < obstacle.position.x + render.size.x / 2)
							{
								if (fogPosition.y > obstacle.position.y - render.size.y / 2 && fogPosition.y < obstacle.position.y + render.size.y / 2)
								{
									found = true;
									fog.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
									fog.Find("Mask").GetComponent<SpriteMask>().enabled = false;
								}
							}
						}
					}
				}
			}
			if (fog.GetComponent<SpriteRenderer>().color.a == 0 && !found)
			{
				fog.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.5f);
				fog.Find("Mask").GetComponent<SpriteMask>().enabled = true;
			}
		}
	}

	public void PerformAction(string action, Transform target, Vector3 coordinates)
	{
		EntityScript stats = Player.GetComponent<EntityScript>();
		if (action == "Move")
		{
			if (stats.Movement > 0 && target.name == "Floor" && coordinates != Player.position)
			{
				if ((Player.position - coordinates).magnitude <= 1.5f)
				{
					if ((Player.position - coordinates).normalized.x > 0)
					{
						Player.localScale = new Vector3(-1, 1, 1);
					}
					if ((Player.position - coordinates).normalized.x < 0)
					{
						Player.localScale = new Vector3(1, 1, 1);
					}
					Player.position = coordinates;
					stats.Movement -= 1;
					ClearArea();
				}
			}
		}
		if (action == "EndTurn")
		{
			StartCoroutine(PassTurn());
		}
	}

	private bool IsMouseOverUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}

	void Awake()
	{
		Vector2 mapSize = transform.Find("Floor").GetComponent<SpriteRenderer>().size;
		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.x; x++)
			{
				Transform fog = Instantiate((GameObject) Resources.Load("Fog")).transform;
				fog.position = new Vector3(Mathf.Floor(x - mapSize.x / 2 + 0.5f), Mathf.Floor(y - mapSize.y / 2 + 0.5f), 0);
				fog.SetParent(transform.Find("Hidden"));
			}
		}
		ClearArea();
	}

	private float doubleTapTimer = 0;
	private float tapCount = 0;

	void Update()
	{
		// Update regular stuff
		Vector3 cameraDistance = Camera.main.transform.position - Player.position;
		Camera.main.transform.position -= new Vector3(cameraDistance.x, cameraDistance.y, 0) * Time.deltaTime * 5;
		if (tapCount > 0)
		{
			doubleTapTimer += Time.deltaTime;
		}
		if (doubleTapTimer > 0.5f)
		{
			doubleTapTimer = 0f;
			tapCount = 0;
		}
		Sounds.Find("Injured").GetComponent<AudioSource>().volume = 3 * (0.33333333333f - Player.GetComponent<EntityScript>().Health / Player.GetComponent<EntityScript>().MaxHealth);
		// Move the player
		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !IsMouseOverUI())
		{
			tapCount += 1;
			if (tapCount >= 2 || Input.GetMouseButtonDown(1))
			{
				doubleTapTimer = 0.0f;
				tapCount = 0;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				int layerMask = LayerMask.GetMask("Default", "Entity", "Obstacle", "ObstacleSight");
				RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, (Player.position - mousePosition).magnitude, layerMask);
				if (hit)
				{
					PerformAction("Move", hit.transform, new Vector3(Mathf.Floor(hit.point.x + 0.5f), Mathf.Floor(hit.point.y + 0.5f), 0));
				}
				int layerMask2 = LayerMask.GetMask("Loot");
				RaycastHit2D checkLoot = Physics2D.Raycast(ray.origin, ray.direction, (Player.position - mousePosition).magnitude, layerMask2);
				if (checkLoot && checkLoot.transform.parent == Loots && (Player.position - checkLoot.transform.position).magnitude <= 1.5f)
				{
					Inventory.parent.gameObject.SetActive(true);
				}
			}
		}
	}
}
