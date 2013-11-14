using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
	public enum Turn{ PLAYER1, PLAYER2};
	public Turn curTurn;
	public GameObject turnDisplay;

	private Combat combat;
	private dfLabel turnLabel;

	private string player1Turn = "first Players turn!";
	private string player2Turn = "second Players turn!";

	// Use this for initialization
	void Awake ()
	{
		turnLabel = turnDisplay.GetComponent<dfLabel>();

		combat = gameObject.GetComponent<Combat>();

		curTurn = Turn.PLAYER2;
	}

	public void SwitchTurn()
	{
		combat.DisabelPlayerBars(curTurn);
		switch (curTurn)
		{
			case Turn.PLAYER1:
				{
					turnLabel.Text = player2Turn;
					curTurn = Turn.PLAYER2;
					break;
				}
			case Turn.PLAYER2:
				{
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
