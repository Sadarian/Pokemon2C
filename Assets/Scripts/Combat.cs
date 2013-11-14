using UnityEngine;
using System.Collections.Generic;

public class Combat : MonoBehaviour
{
	public enum Elements{FIRE,AIR,EARTH,WATER};
	public Elements element;

	public float playerHealth = 1000;
	public float playerMana = 250;
	public GameObject player1;
	public GameObject player2;

	public dfProgressBar player1Health;
	public dfProgressBar player2Health;
	public dfProgressBar player1Mana;
	public dfProgressBar player2Mana;

	private GameController gameController;


	// Use this for initialization
	void Awake ()
	{
		gameController = gameObject.GetComponent<GameController>();

		player1Health = player1.transform.GetChild(0).gameObject.GetComponent<dfProgressBar>();
		player1Mana = player1.transform.GetChild(1).gameObject.GetComponent<dfProgressBar>();
		player2Health = player2.transform.GetChild(0).gameObject.GetComponent<dfProgressBar>();
		player2Mana = player2.transform.GetChild(1).gameObject.GetComponent<dfProgressBar>();

		player1Health.MaxValue = playerHealth;
		player2Health.MaxValue = playerHealth;
		player1Mana.MaxValue = playerMana;
		player2Mana.MaxValue = playerMana;

		player1Health.Value = playerHealth;
		player2Health.Value = playerHealth;
		player1Mana.Value = playerMana;
		player2Mana.Value = playerMana;

		gameController.SwitchTurn();
	}

	public void Attack(List<dfButton> buttons)
	{
		ButtonInfo firstButtonInfo = buttons[0].GetComponent<ButtonInfo>();

		switch (firstButtonInfo.element)
		{
			case Elements.FIRE:
				{
					DoAttack(gameController.curTurn == GameController.Turn.PLAYER1 ? player2Health : player1Health, firstButtonInfo);
					break;
				}
		}
		gameController.SwitchTurn();
	}

	public void DoAttack(dfProgressBar victem, ButtonInfo info)
	{
		Debug.Log("Player " + (gameController.curTurn) + " dealt Damage with a Power of " + (info.element));

		victem.Value -= info.power;
	}

	public void DisabelPlayerBars(GameController.Turn curTurn)
	{
		switch (curTurn)
		{
			case GameController.Turn.PLAYER1:
				{
					player2Health.Enable();
					player1Health.Disable();
					break;
				}
			case GameController.Turn.PLAYER2:
				{
					player1Health.Enable();
					player2Health.Disable();
					break;
				}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
