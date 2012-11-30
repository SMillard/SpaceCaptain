using UnityEngine;
using System.Collections;

public class EnemyShip : MonoBehaviour
{
	public enum ActionType { None, FireLasers }
	
	public float HullLevel;
	
	public float Accuracy;
	
	public Action[] Actions;
	
	private float _startingHull;
	
	private float _timer = 5f;
	private float _effectTimer;
	
	private GameController _gameController;
	private Interaction[] _interactions;
	
	private Action _currentAction;
	
	void Start()
	{
		_gameController = FindObjectOfType(typeof(GameController)) as GameController;
		_interactions = FindObjectsOfType(typeof(Interaction)) as Interaction[];
		_startingHull = HullLevel;
	}
	
	void Update()
	{
		if (_currentAction != null && !_gameController.GetGameOverStatus())
		{
			if (_effectTimer <= 0)
			{
				if (_currentAction.Type == ActionType.FireLasers)
				{
					try {
						audio.PlayOneShot(_currentAction.ActivateAudio[Random.Range(0, _currentAction.ActivateAudio.Length + 1)]);
					} catch {}
					
					float evasionChance = 0f;
					float damageReduction = 0f;
					float accuracyReduction = 0f;
					
					foreach (Interaction i in _interactions)
					{
						if (i.GetCurrentOrder() != null && i.GetCurrentOrder().Type == Interaction.OrderType.EvasiveManeuvers)
							evasionChance = 70f;
						else if (i.GetCurrentOrder() != null && i.GetCurrentOrder().Type == Interaction.OrderType.ShieldsUP)
							damageReduction = _currentAction.Damage * 80 / 100;
						else if (i.GetCurrentOrder() != null && i.GetCurrentOrder().Type == Interaction.OrderType.JamEnemy)
							accuracyReduction = 30;
					}
					
					if (Random.Range(0, 100) >= evasionChance &&
						Random.Range(0, 100) < Accuracy - accuracyReduction)
					{
						_gameController.HullLevel = Mathf.Max(0, _gameController.HullLevel - (_currentAction.Damage - damageReduction));
						
						if (_gameController.HullLevel <= 0)
							_gameController.SetGameOverStatus(true, false);
					}
				}
				
				_effectTimer = 0;
				_currentAction = null;
			}
			
			_effectTimer -= Time.deltaTime;
		}
		
		if (_timer <= 0 && _currentAction == null && !_gameController.GetGameOverStatus())
		{
			try {
				audio.PlayOneShot(_currentAction.SelectAudio[Random.Range(0, _currentAction.SelectAudio.Length + 1)]);
			} catch {}
			
			_timer = 0;
			_currentAction = Actions[0];
			_effectTimer = _currentAction.EffectTime;
		}
		
		_timer -= Time.deltaTime;
	}
	
	void OnGUI()
	{
		if (!_gameController.GetGameOverStatus())
		{
			GUI.skin.box.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.normal.textColor = Color.red;
			
			Vector3 myPos = Camera.mainCamera.WorldToScreenPoint(transform.position);
			
			GUI.Box(new Rect(myPos.x - 75, Screen.height - myPos.y - 110, 150, 25), "Hull Integrity");
			if (HullLevel > 0)
				GUI.Box(new Rect(myPos.x - 75, Screen.height - myPos.y - 110, 150 * HullLevel / _startingHull, 25), "");
			
			if (_currentAction != null)
				GUI.Label(new Rect(myPos.x - 75, Screen.height - myPos.y + 35, 150, 25), _currentAction.Name);
			
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			GUI.skin.label.normal.textColor = Color.white;
		}
	}
	
	[System.Serializable]
	public class Action
	{
	    public string Name;
	    public ActionType Type;
		public float Cooldown;
		public float EffectTime;
		public float Damage;
		public AudioClip[] SelectAudio;
		public AudioClip[] ActivateAudio;
	}
}
