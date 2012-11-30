using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
	public float HullLevel;
	public float PowerLevel;
	
	public float Accuracy;
	
	public GameObject LaserObject;
	
	private LineRenderer[] _lasers;
	
	void Start ()
	{
		_lasers = LaserObject.GetComponentsInChildren<LineRenderer>();
	}
	
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100))
			{
				Interaction interaction = hit.collider.GetComponent<Interaction>();
				if (interaction != null)
				{
					interaction.SendMessage("Tap");
				}
			}
		}
	}
	
	void OnGUI()
	{
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		
		GUI.Box(new Rect(Screen.width / 8, 5, Screen.width / 3, 30), "Hull Integrity");
		if (HullLevel > 0)
			GUI.Box(new Rect(Screen.width / 8, 5, (Screen.width / 3) * HullLevel / 100, 30), "");
		
		GUI.Box(new Rect(Screen.width / 2, 5, Screen.width / 3, 30), "Main Power");
		if (PowerLevel > 0)
			GUI.Box(new Rect(Screen.width / 2, 5, (Screen.width / 3) * PowerLevel / 100, 30), "");
	}
	
	public LineRenderer[] GetLasers()
	{
		return _lasers;
	}
}
