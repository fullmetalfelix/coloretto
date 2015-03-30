using UnityEngine;
using System.Collections;

public class Deck : Card {

	public int[] gamedeck;
	public int currentcard = 0;
	public bool lastround = false;
	public GameObject lastcardtemplate;
	public int lastpickedcardID;
	public Card lastpickedcard;
	protected Card lastcardobj;
	public double[] pickchance;
	public int[] availablecards;

	public new static Deck Fabricate(GameObject template)
	{
		GameObject c = (GameObject)Instantiate(template);
		Deck s = c.GetComponent<Deck>();
		s.meshobject = s.transform.FindChild("mesh").gameObject;
		s.SetColor(Color.gray);
		c.name = "deck";


		s.logic = GameObject.Find("master").GetComponent<GameLogic>();
		s.scalefactor = s.logic.cardsize;
		s.aspectratio = s.logic.cardaspect;
		//s.GetComponent<MeshRenderer>().material.mainTexture = s.logic.decktex;

		s.currentcard = 0;

		return s;
	}


	public void MakeDeck(int[] cardspercolor)
	{
		int ncards = logic.ncolors * 9 + 10 + 3 + 1;
		ncards = ncards - logic.nplayers;

		int[] deck = new int[ncards - 1];
		int[] sdeck = new int[ncards];

		int cnt = 0;
		for (int i = 0; i < logic.ncolors; i++)
		{
			for (int j = 0; j < cardspercolor[i]; j++)
			{
				deck[cnt] = i;
				cnt++;
			}
		}
		for (int i = 0; i < 10; i++)
		{
			deck[cnt] = logic.ncolors + 0; cnt++;
		}
		for (int i = 0; i < 3; i++)
		{
			deck[cnt] = logic.ncolors + 1; cnt++;
		}

		deck = GameLogic.Shuffle<int>(deck);
		cnt = 0;

		//copy the last 15 cards to the new deck
		for (int i = 0; i < 15; i++)
		{
			sdeck[ncards - 1 - i] = deck[ncards - 1 - i - 1];
		}
		sdeck[ncards - 1 - 15] = 99;
		for (int i = ncards - 1 - 15 - 1; i >= 0; i--)
			sdeck[i] = deck[i];
		//sdeck[4] = 99;
		gamedeck = sdeck;

		availablecards = new int[logic.ncolors + 2];
		for (int i = 0; i < logic.ncolors; i++)
			availablecards[i] = cardspercolor[i];
		availablecards[logic.ncolors] = 10;
		availablecards[logic.ncolors+1] = 3;

	}


	public int PickCardFromDeck()
	{
		//pick a card
		currentcard++;

		lastpickedcardID = gamedeck[currentcard];
		Debug.Log("pickedcard was " + lastpickedcardID.ToString() + " -- " + currentcard.ToString());

		#region "if lastround card"
		if (lastpickedcardID == 99)
		{
			lastround = true;
			//TODO: show the last turn card
			lastcardobj = Card.Fabricate(lastcardtemplate, Color.gray, 9);
			lastcardobj.transform.parent = logic.transform;

			StartCoroutine(CardPop(lastcardobj, -Vector3.right * logic.cardspacing * 0.5f,
				Vector3.up * logic.cardspacing * 1.7f, 90));

			currentcard++;
			lastpickedcardID = gamedeck[currentcard];
			Debug.Log("pickedcard was " + lastpickedcardID.ToString() + " -- " + currentcard.ToString());
		}
		#endregion

		lastpickedcard = Card.Fabricate(logic.cardtemplate, logic.cardcolors[lastpickedcardID], lastpickedcardID);
		lastpickedcard.transform.parent = logic.transform;
		StartCoroutine(CardPop(lastpickedcard, -Vector3.right * logic.cardspacing * 0.5f,
			-Vector3.right * logic.cardspacing*2, 90));
		//phase = GamePhases.CardToPile;
		availablecards[lastpickedcardID]--;

		return lastpickedcardID;
	}

	public IEnumerator CardPop(Card card, Vector3 startpos, Vector3 finalpos, float finalangle)
	{
		logic.phase = GamePhases.Dummy;

		card.SendTo(-Vector3.forward+startpos, 90);
		yield return new WaitForSeconds(0.2f);
		card.SendTo(finalpos, finalangle);

		logic.phase = GamePhases.CardToPile;
	}



	void OnMouseUpAsButton()
	{

		Debug.Log("deck clicked " + logic.pilesallfull.ToString());

		if (logic.phase == GamePhases.DeckOrPile && !logic.pilesallfull &&
			logic.playerAI[logic.currentplayer] == SeatType.Human)
		{
			Debug.Log("deck clicked!");
			animator.SizeOscillateStop(0.5f);
			logic.PlayerChosesDeck();

			PickCardFromDeck();

			return;
		}


	}

	public double CardTypeProbability(int typeID)
	{
		double result = 0;

		result = (double)availablecards[typeID] / (gamedeck.Length - 1 - currentcard);

		return result;
	}


	public void Reset()
	{
		lastcardobj.animator.FadeOut(0, 1);
		Destroy(lastcardobj.gameObject, 2);
		lastcardobj = null;

	}


}
