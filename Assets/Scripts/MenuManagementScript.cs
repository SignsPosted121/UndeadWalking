using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManagementScript : MonoBehaviour
{

	public Transform GameManager;
	public Transform Player;

	public Transform Missions;
	public Transform Inventory;

	public void ToggleMenu()
	{
		if (transform.Find("PersonalStats").gameObject.activeSelf)
		{
			transform.Find("PersonalStats").gameObject.SetActive(false);
			return;
		} else
		{
			transform.Find("PersonalStats").gameObject.SetActive(true);
			return;
		}
	}

}
