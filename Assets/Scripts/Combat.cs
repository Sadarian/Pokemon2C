using UnityEngine;
using System.Collections.Generic;

public class Combat : MonoBehaviour
{
	public enum Elements{FIRE,AIR,EARTH,WATER};
	public Elements element;

	public enum Attacks
	{
		Firestorm,
		Metore,
		Steam,
		Erthquacke,
		Sandstorm,
		Quicksand,
		Greatheal,
		IncreaseDef,
		IncreaseAtk,
		ManaReg
	}
	public Attacks attack;

	public float playerHealth = 1000;
	public float playerMana = 250;
	public GameObject barPlayer1;
	public GameObject barPlayer2;

	public Player player1;
	public Player player2;

	private GameController gameController;

	public class Player
	{
		public dfProgressBar health;
		public dfProgressBar mana;

		public Player(dfProgressBar health, dfProgressBar mana)
		{
			this.health = health;
			this.mana = mana;
		}

		public void Init(float maxHealth, float maxMana)
		{
			health.MaxValue = maxHealth;
			mana.MaxValue = maxMana;

			health.Value = maxHealth;
			mana.Value = maxMana;
		}

		public void Enable()
		{
			health.Enable();
			mana.Enable();
		}

		public void Disable()
		{
			health.Disable();
			mana.Disable();
		}
	}

	// Use this for initialization
	void Awake ()
	{
		gameController = gameObject.GetComponent<GameController>();

		player1 = new Player(barPlayer1.transform.GetChild(0).gameObject.GetComponent<dfProgressBar>(), barPlayer1.transform.GetChild(1).gameObject.GetComponent<dfProgressBar>());
		player2 = new Player(barPlayer2.transform.GetChild(0).gameObject.GetComponent<dfProgressBar>(), barPlayer2.transform.GetChild(1).gameObject.GetComponent<dfProgressBar>());
		
		player1.Init(playerHealth, playerMana);
		player2.Init(playerHealth, playerMana);

		gameController.SwitchTurn();
	}

	public void Attack(List<dfButton> buttons)
	{
		ButtonInfo firstButtonInfo = buttons[0].GetComponent<ButtonInfo>();
		ButtonInfo secButtonInfo = buttons[1].GetComponent<ButtonInfo>();

		switch (firstButtonInfo.element)
		{
			case Elements.FIRE:
				{
					switch (secButtonInfo.element)
					{
						case Elements.AIR:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Firestorm);
								break;
							}
						case Elements.EARTH:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Metore);
								break;
							}
						case Elements.WATER:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Steam);
								break;
							}
					}
					DoAttack(gameController.curTurn == GameController.Turn.PLAYER1 ? player2 : player1, 
							 gameController.curTurn == GameController.Turn.PLAYER1 ? player1 : player2, firstButtonInfo, secButtonInfo);
					break;
				}
			case Elements.EARTH:
				{
					switch (secButtonInfo.element)
					{
						case Elements.AIR:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Sandstorm);
								break;
							}
						case Elements.FIRE:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Erthquacke);
								break;
							}
						case Elements.WATER:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Quicksand);
								break;
							}
					}
					DoAttack(gameController.curTurn == GameController.Turn.PLAYER1 ? player2 : player1,
							 gameController.curTurn == GameController.Turn.PLAYER1 ? player1 : player2, firstButtonInfo, secButtonInfo);
					break;
				}
			case Elements.AIR:
				{
					switch (secButtonInfo.element)
					{
						case Elements.EARTH:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.IncreaseDef);
								Buff();
								break;
							}
						case Elements.FIRE:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.IncreaseAtk);
								Buff();
								break;
							}
						case Elements.WATER:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.Greatheal);
								Heal(gameController.curTurn == GameController.Turn.PLAYER1 ? player1 : player2, firstButtonInfo, secButtonInfo);
								break;
							}
					}
					break;
				}
			case Elements.WATER:
				{
					switch (secButtonInfo.element)
					{
						case Elements.EARTH:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.IncreaseDef);
								Buff();
								break;
							}
						case Elements.FIRE:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.IncreaseAtk);
								Buff();
								break;
							}
						case Elements.AIR:
							{
								Debug.Log(gameController.curTurn + " used " + Attacks.ManaReg);
								Mana(gameController.curTurn == GameController.Turn.PLAYER1 ? player1 : player2, firstButtonInfo, secButtonInfo);
								break;
							}
					}
					break;
				}
		}
		gameController.SwitchTurn();
	}

	private void Mana(Player offender, ButtonInfo firstButtonInfo, ButtonInfo secButtonInfo)
	{
		float manaAmount = firstButtonInfo.power + secButtonInfo.power;

		Debug.Log("It regenerates " + manaAmount + " HP");

		offender.mana.Value += manaAmount;
	}

	private void Heal(Player offender, ButtonInfo firstButtonInfo, ButtonInfo secButtonInfo)
	{
		float healAmount = firstButtonInfo.power + secButtonInfo.power;
		float manaCost = firstButtonInfo.manaUsage + secButtonInfo.manaUsage / 2;

		Debug.Log("It Heals " + healAmount + " HP");

		offender.health.Value += healAmount;
		offender.mana.Value -= manaCost;
	}

	public void DoAttack(Player victem, Player offender, ButtonInfo firstButtonInfo, ButtonInfo secButtonInfo)
	{
		float damage = firstButtonInfo.power + secButtonInfo.power;
		float manaCost = firstButtonInfo.manaUsage + secButtonInfo.manaUsage / 2;
		Debug.Log("It dealt " + damage + " " + firstButtonInfo.element + " " + secButtonInfo.element + " Damage");

		victem.health.Value -= damage;
		offender.mana.Value -= manaCost;
	}

	private void Buff()
	{
		
	}

	public void DisabelPlayerBars(GameController.Turn curTurn)
	{
		switch (curTurn)
		{
			case GameController.Turn.PLAYER1:
				{
					player2.Enable();
					player1.Disable();
					break;
				}
			case GameController.Turn.PLAYER2:
				{
					player1.Enable();
					player2.Disable();
					break;
				}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
