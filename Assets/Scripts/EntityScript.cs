using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
	public float Health = 20;
	public float MaxHealth = 20;
	public float BaseDamage = 2;
	public float Movement = 1;
	public float MaxMovement = 1;
	public float Eyesight = 3;
	public float Hearing = 5;
	public bool Tracking = false;

	[Range(0, 12)] public float Water = 8;
	[Range(0, 12)] public float Food = 6;

	public void Damage(float damage)
	{
		Health -= damage;
		if (Health > 0)
		{
			Notification("Damage");
		}
		if (Health <= 0)
		{
			transform.GetComponent<Animator>().SetBool("Dead", true);
			if (tag == "Zombie")
			{
				transform.SetParent(transform.parent.parent.Find("Floor"));
				StartCoroutine(PushNotification("Death"));
				gameObject.GetComponent<SpriteRenderer>().enabled = false;
				transform.Find("Shadow").GetComponent<SpriteRenderer>().enabled = false;
				gameObject.GetComponent<BoxCollider2D>().enabled = false;
			}
			if (tag == "Player")
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene("_Menu");
			}
		}
	}

	IEnumerator PushNotification(string noteType)
	{
		Transform note = Instantiate((GameObject)Resources.Load("Notifications/" + noteType)).transform;
		note.SetParent(transform);
		float timer = 0;
		note.position = transform.position + new Vector3(Random.Range(-5, 5) / 10, 1, 0);
		switch(noteType)
		{
			case "Damage":
				note.position = transform.position + new Vector3(0, 0.5f, 0);
				timer = 1.5f;
				break;
			case "Death":
				note.SetParent(transform.parent);
				note.position = transform.position + new Vector3(0, 0.5f, 0);
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
		if (noteType == "Death")
		{
			Destroy(gameObject);
		}
	}

	public void Notification(string noteType)
	{
		StartCoroutine(PushNotification(noteType));
	}
}
