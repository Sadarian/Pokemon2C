using UnityEngine;
using System.Collections.Generic;

public class TouchHandle : MonoBehaviour
{
	private const float DISTANCE = 45f;
	public List<dfButton> activeCombination = new List<dfButton>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnMouseMove(dfControl control, dfMouseEventArgs mouseEvent)
	{
		//Debug.Log("Mouse " + mouseEvent.Position);
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			GameObject curChild = gameObject.transform.GetChild(i).gameObject;
			dfButton curButton = curChild.GetComponent<dfButton>();
			Vector2 buttonCenter = new Vector2(curButton.Position.x + curButton.Size.x / 2, curButton.Position.y - curButton.Size.y / 2);
			
			float dist = (mouseEvent.Position - buttonCenter).magnitude;
			//Debug.Log("button " + i + buttonCenter + " distance " + dist);

			if (!activeCombination.Contains(curButton) && dist <= DISTANCE)
			{
				curButton.State = dfButton.ButtonState.Hover;
				activeCombination.Add(curButton);
				//Debug.Log("Active Combination " + activeCombination.Count);
			}
		}
	}
	
	public void OnMouseUp(dfControl control, dfMouseEventArgs mouseEvent)
	{
		//Debug.Log("Mouse Up!");
		if (activeCombination.Count == 4)
		{
			GameObject.FindGameObjectWithTag("GameController").GetComponent<Combat>().Attack(activeCombination);
			foreach (dfButton button in activeCombination)
			{
				button.State = dfButton.ButtonState.Default;
			}
			activeCombination.Clear();
		}
	}

}
