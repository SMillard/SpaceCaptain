using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour
{
	public enum OrderType { None, FireLasers, FireTorpedoes, Repair, PowerManagement, ShieldsUP, JamEnemy, EvasiveManeuvers, FlankingAttack }
	
	public Order[] Orders;
	
	private bool _interacting;
	
	private GameController _gameController;
	private Interaction[] _interactions;
	private EnemyShip _enemyShip;
	
	private bool _onCooldown;
	
	private float _timer = 0f;
	private float _effectTimer = 0f;
	private float _maxEffectTimer = 0f;
	private float _powerRegen = 0f;
	
	private Vector3 _myPos;
	
	private Order _currentOrder;
	
	void Start ()
	{
		_gameController = FindObjectOfType(typeof(GameController)) as GameController;
		_interactions = FindObjectsOfType(typeof(Interaction)) as Interaction[];
		_enemyShip = FindObjectOfType(typeof(EnemyShip)) as EnemyShip;
		
		_myPos = Camera.mainCamera.WorldToScreenPoint(transform.position);
	}
	
	void Update ()
	{
		if (_onCooldown)
		{
			_timer -= Time.deltaTime;
			if (_timer <= 0)
			{
				_timer = 0;
				_onCooldown = false;
			}
			else if (_currentOrder != null && _currentOrder.Type == OrderType.FireLasers)
			{
				foreach (LineRenderer laser in _gameController.GetLasers())
				{
					laser.enabled = ((int)(_timer * 5)) % 2 == 0;
				}
			}
		}
		
		if (_currentOrder != null)
		{
			_effectTimer -= Time.deltaTime;
			ActivateOrder();
			
			if (_effectTimer <= 0)
			{
				_effectTimer = 0;
				_currentOrder = null;
			}
		}
	}
	
	void Tap()
	{
		_interacting = !_interacting;
		
		foreach (Interaction i in _interactions)
		{
			if (i != this)
				i.DisableInteraction();
		}
	}
	
	void OnGUI()
	{
		if (_interacting)
		{
			for (int i = 0; i < Orders.Length; i++)
			{
				GUI.enabled = !_onCooldown && Orders[i].PowerCost <= _gameController.PowerLevel;
				Rect orderRect = new Rect(_myPos.x  + 25 - 75 * Orders.Length + i * 150, Screen.height - _myPos.y - 100, 100, 100);
				Rect orderRectName = new Rect(_myPos.x  + 30 - 75 * Orders.Length + i * 150, Screen.height - _myPos.y - 80, 150, 100);
				Rect orderRectPower = new Rect(_myPos.x  + 30 - 75 * Orders.Length + i * 150, Screen.height - _myPos.y - 60, 150, 100);
				Rect orderRectCooldown = new Rect(_myPos.x  + 30 - 75 * Orders.Length + i * 150, Screen.height - _myPos.y - 40, 150, 100);
				GUI.Box(orderRect, "");
				GUI.Label(orderRectName, Orders[i].Name);
				GUI.Label(orderRectPower, "Power: " + Orders[i].PowerCost);
				GUI.Label(orderRectCooldown, "Cooldown: " + Orders[i].Cooldown);
				if (GUI.enabled && Input.GetMouseButtonUp(0) && new Rect(_myPos.x  + 25 - 75 * Orders.Length + i * 150, _myPos.y, 100, 100).Contains(Input.mousePosition))
				{
					_gameController.PowerLevel -= Orders[i].PowerCost;
					_onCooldown = true;
					_timer = Orders[i].Cooldown;
					_interacting = false;
					_effectTimer = Orders[i].EffectTime;
					_maxEffectTimer = Orders[i].EffectTime;
					_currentOrder = Orders[i];
					_powerRegen = 100 - _gameController.PowerLevel;
					
					try {
						audio.PlayOneShot(_currentOrder.SelectAudio[Random.Range(0, _currentOrder.SelectAudio.Length + 1)]);
					} catch {}
				}
			}
			
			GUI.enabled = true;
		}
			
		if (_onCooldown && _currentOrder != null)
		{
			GUIContent text = new GUIContent(_currentOrder.Name + ": " + string.Format("{0:0.0}", _effectTimer));
			Vector2 textSize = GUI.skin.box.CalcSize(text);
			GUI.Box(new Rect(_myPos.x - 13 - textSize.x / 2, Screen.height - _myPos.y - 160, 26 + textSize.x, 50), "");
			GUI.Box(new Rect(_myPos.x - 13 - textSize.x / 2, Screen.height - _myPos.y - 160, Mathf.Max((26 + textSize.x) * (_effectTimer / _currentOrder.EffectTime), 5), 50), "");
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.Label(new Rect(_myPos.x - 13 - textSize.x / 2, Screen.height - _myPos.y - 160, 26 + textSize.x, 50), text);
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
		}
	}
	
	public void DisableInteraction()
	{
		_interacting = false;
	}
	
	public Order GetCurrentOrder()
	{
		return _currentOrder;
	}
	
	private void ActivateOrder()
	{
		try {
			audio.PlayOneShot(_currentOrder.ActivateAudio[Random.Range(0, _currentOrder.ActivateAudio.Length + 1)]);
		} catch {}
		
		switch (_currentOrder.Type)
		{
		case OrderType.FireLasers:
			if (_effectTimer <= 0)
			{
				float accuracyBonus = 0;
				float damageBonus = 0;
				
				foreach (Interaction i in _interactions)
				{
					if (i.GetCurrentOrder() != null && i.GetCurrentOrder().Type == Interaction.OrderType.FlankingAttack)
					{
						damageBonus = 30;
						accuracyBonus = 20;
					}
				}
				
				if (Random.value * 100 < (_gameController.Accuracy - 10 + accuracyBonus))
				{
					_enemyShip.HullLevel -= _currentOrder.Damage * (1f + (damageBonus / 100f));
				}
				
				foreach (LineRenderer laser in _gameController.GetLasers())
				{
					laser.enabled = false;
				}
			}
			break;
		case OrderType.FireTorpedoes:
			if (_effectTimer <= 0)
			{
				float accuracyBonus = 0;
				float damageBonus = 0;
				
				foreach (Interaction i in _interactions)
				{
					if (i.GetCurrentOrder() != null && i.GetCurrentOrder().Type == Interaction.OrderType.FlankingAttack)
					{
						damageBonus = 30;
						accuracyBonus = 20;
					}
				}
				
				if (Random.value * 100 < (_gameController.Accuracy + 20 + accuracyBonus))
				{
					_enemyShip.HullLevel -= _currentOrder.Damage * (1f + (damageBonus / 100f));
				}
			}
			break;
		case OrderType.PowerManagement:
			_gameController.PowerLevel = Mathf.Min(_gameController.PowerLevel + _powerRegen * Time.deltaTime / _maxEffectTimer, 100);
			break;
		case OrderType.Repair:
			// repair
			break;
		}
	}
	
	[System.Serializable]
	public class Order
	{
	    public string Name;
	    public OrderType Type;
		public float Cooldown;
		public float EffectTime;
		public int PowerCost;
		public float Damage;
		public AudioClip[] SelectAudio;
		public AudioClip[] ActivateAudio;
	}
}
