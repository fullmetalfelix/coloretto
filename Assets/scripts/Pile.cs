using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Pile : Card
{

	public int pileID;
	public List<Card> cards;
	public Deck gamedeck;
	public bool full { get { return cards.Count == maxcards; } }
	public bool empty { get { return (cards.Count == 0); } }
	public bool taken = false;
	public bool pickable { get { return taken && (cards.Count > 0); } }
	public int maxcards = 3;

	

	public static Pile Fabricate(GameObject template, Texture2D tex)
	{
		GameObject c = (GameObject)Instantiate(template);
		
		Pile s = c.GetComponent<Pile>();
		s.meshobject = s.transform.FindChild("mesh").gameObject;
		s.SetColor(Color.gray);
		s.SetTexture(tex);
		s.cards = new List<Card>(3);

		s.logic = GameObject.Find("master").GetComponent<GameLogic>();
		s.gamedeck = GameObject.Find("deck").GetComponent<Deck>();

		s.scalefactor = s.logic.cardsize;
		s.aspectratio = s.logic.cardaspect;

		return s;
	}



	void OnMouseUpAsButton()
	{

		if (logic.phase == GamePhases.CardToPile && !taken && !full &&
			logic.playerAI[logic.currentplayer] == SeatType.Human)
		{
			//the player chose this pile to put the card

			PutCardInPile();
			StartCoroutine(logic.SetNextPlayer());
			return;
		}

		if (logic.phase == GamePhases.DeckOrPile && !taken && cards.Count > 0 &&
			logic.playerAI[logic.currentplayer] == SeatType.Human)
		{
			//player wants to pick the pile
			PickPile();
			StartCoroutine(logic.SetNextPlayer());
		}

	}

	public void PickPile()
	{
		logic.phase = GamePhases.Dummy;
		foreach (Card c in cards)
		{
			c.SendToStock(logic.playerstocks[logic.currentplayer]);
		}
		cards.Clear();
		taken = true;

		//SendTo(transform.position, -180);
		animator.Roll(-90, 0.5f);

		//the player who picked this is out
		logic.playerout[logic.currentplayer] = true;

	}
	public void PutCardInPile()
	{
		logic.phase = GamePhases.Dummy;
		//play the animation
		Vector3 pos = transform.position + transform.right * logic.cardspacing * (cards.Count + 1);
		pos += transform.right * logic.cardspacing * 0.3f;

		gamedeck.lastpickedcard.SendTo(pos, 90);


		//add it to the pile
		cards.Add(gamedeck.lastpickedcard);

	}

	public void Reset()
	{
		taken = false;
		animator.Roll(0, 0.5f);

		//TODO: discard if 2pl game and pile has cards

		cards.Clear();
	}

}
