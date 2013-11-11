using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
	public enum Turn{ PLAYER1, PLAYER2};
	public Turn curTurn;
	public float playerHealth = 100;
	public GameObject player1;
	public GameObject player2;
	public GameObject turnDisplay;

	private dfProgressBar player1Health;
	private dfProgressBar player2Health;
	private dfLabel turnLabel;

	private string player1Turn = "first Players turn!";
	private string player2Turn = "second Players turn!";

	// Use this for initialization
	void Awake ()
	{
		player1Health = player1.GetComponent<dfProgressBar>();
		player2Health = player2.GetComponent<dfProgressBar>();
		turnLabel = turnDisplay.GetComponent<dfLabel>();

		player1Health.MaxValue = playerHealth;
		player2Health.MaxValue = playerHealth;

		player1Health.Value = playerHealth;
		player2Health.Value = playerHealth;

		curTurn = Turn.PLAYER2;
		SwitchTurn();
	}

	public void Attack(List<dfButton> attackPoints)
	{
		DoAttack(curTurn == Turn.PLAYER1 ? player2Health : player1Health, attackPoints.Count);
	}

	private void DoAttack(dfProgressBar victem, int count)
	{
		Debug.Log("Player " + (curTurn) + " dealt Damage with a Power of " + (count));
		victem.Value -= count*10;
		SwitchTurn();
	}

	public void SwitchTurn()
	{
		switch (curTurn)
		{
			case Turn.PLAYER1:
				{
					player2Health.Enable();
					player1Health.Disable();
					turnLabel.Text = player2Turn;
					curTurn = Turn.PLAYER2;
					break;
				}
			case Turn.PLAYER2:
				{
					player1Health.Enable();
					player2Health.Disable();
					turnLabel.Text = player1Turn;
					curTurn = Turn.PLAYER1;
					break;
				}
		}
	}

	public void OnClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		SwitchTurn();
	}

	// Update is called once per frame
	void Update()
	{

	}

}
