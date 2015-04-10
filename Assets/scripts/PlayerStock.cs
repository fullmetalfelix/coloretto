using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerStock : MonoBehaviour {

	public SpriteAnimator animator;
	
	public int[] stock;
	public int points
	{
		get { return ComputePoints(cards); }
	}
	public GameObject texttemplate;

	protected GameLogic logic;
	protected GUIText namescore;

	public List<Card> cards;

	public static PlayerStock Fabricate(GameObject template, Vector3 position, Texture2D icon)
	{
		GameObject g = (GameObject)Instantiate(template);
		g.name = "stock";
		PlayerStock s = g.GetComponent<PlayerStock>();

		g.transform.position = position;
		g.transform.LookAt(Vector3.zero,-Vector3.forward);

		s.logic = GameObject.Find("master").GetComponent<GameLogic>();
		//s.animator.SetTexture(icon);
		s.stock = new int[s.logic.ncolors + 2];
		s.cards = new List<Card>();

		//make the guitext for playername-score
		GameObject gt = (GameObject)Instantiate(s.texttemplate);
		gt.transform.parent = g.transform;
		
		Vector3 txtpos = g.transform.position - g.transform.forward * s.logic.cardspacing * 2;
		gt.transform.position = Camera.main.WorldToViewportPoint(txtpos);
		
		s.namescore = gt.GetComponent<GUIText>();
		s.namescore.text = "ASD!";

		//make the player icon
		Sprite plicon = Sprite.Fabricate();
		plicon.transform.parent = s.transform;
		plicon.SetTexture(icon);

		plicon.scalefactor = 10;
		Vector3 iconpos = s.transform.position + 2*s.transform.right * s.logic.cardspacing - 1.5f*s.transform.forward * s.logic.cardspacing;
		plicon.SendTo(iconpos, 0);

		s.animator = plicon.animator;

		return s;
	}

	public static int StockValueAfterPileasd(PlayerStock stock, Pile pile)
	{
		int points = stock.points;

		List<Card> nst = new List<Card>();
		nst.AddRange(stock.cards);
		nst.AddRange(pile.cards);
		int newpoints =stock.ComputePoints(nst);

		return newpoints - points;
	}

	public int DeltaStockValueAfterPilePlusCard(Pile pile, Card pickedcard)
	{
		
		//if the pile is full... dont care!!!
		
		List<Card> nst = new List<Card>();
		nst.AddRange(cards);
		nst.AddRange(pile.cards);
		nst.Add(pickedcard);
		int newpoints = ComputePoints(nst);

		return ComputePoints(nst) - points;
	}
	public int StockValueAfterPilePlusCard(Pile pile, Card pickedcard)
	{

		//if the pile is full... dont care!!!

		List<Card> nst = new List<Card>();
		nst.AddRange(cards);
		nst.AddRange(pile.cards);
		nst.Add(pickedcard);
		int newpoints = ComputePoints(nst);

		return ComputePoints(nst);
	}
	public int EntropyAfterPilePlusCard(Pile pile, Card pickedcard)
	{

		//if the pile is full... dont care!!!

		int[] newstock = new int[stock.Length];
		for (int i = 0; i < newstock.Length; i++)
			newstock[i] = stock[i];
		foreach (Card c in pile.cards)
			newstock[c.cardindex]++;
		newstock[pickedcard.cardindex]++;

		int entropy = 0;
		for (int i = 0; i < stock.Length - 2; i++)
			if (newstock[i] > 0)
				entropy++;

		return entropy;
	}


	public Vector4 GetNewCardPosition(Card card)
	{
		Vector4 result = Vector4.zero;
		Vector3 pos = transform.position;

		result.w = Vector3.Angle(Vector3.up, transform.forward);
		if (result.w >= 180.0f)
			result.w -= 180.0f;
		
		if (card.cardindex < logic.ncolors)
		{
			pos += logic.cardspacing * transform.right * (card.cardindex - 0.5f * (stock.Length - 3));
			pos += logic.cardvspacing * transform.forward * stock[card.cardindex] * 1;

		} else {
			pos -= transform.forward * logic.cardspacing * 1.6f;
			pos += transform.right * logic.cardvspacing * stock[card.cardindex] * 1.5f;
			pos += transform.right * 2 * ((card.cardindex == logic.ncolors) ? -logic.cardspacing : logic.cardspacing);
			result.w += 90;

		}
		pos += 0.01f * transform.up * stock[card.cardindex];

		stock[card.cardindex]++; //add the card to the stock

		//card.transform.parent = transform;
		cards.Add(card);

		result.x = pos.x;
		result.y = pos.y;
		result.z = pos.z;

		return result;
	}


	protected int ComputePoints(List<Card> cardlist)
	{
		int pts = 0;
		int jokers = cardlist.FindAll(o => o.cardindex == logic.ncolors + 1).Count;
		int plus2 = cardlist.FindAll(o => o.cardindex == logic.ncolors).Count;

		List<int> colorcards = new List<int>(logic.ncolors);

		for (int i = 0; i < logic.ncolors; i++)
		{
			colorcards.Add(Mathf.Min(
				cardlist.FindAll(o => o.cardindex == i).Count, 6));
		}

		colorcards.Sort();
		colorcards.Reverse();

		for (int i = 0; i < 3; i++)
		{
			if (colorcards[i] < 6 && jokers > 0)
			{
				int used = Mathf.Min(jokers, 6 - colorcards[i]);
				colorcards[i] += used;
				jokers -= used;
				if (jokers == 0) break;
				if (jokers < 0) Debug.LogError("ERROR! when computing scores!");
				i = 0;
			}
		}

		for (int i = 0; i < logic.ncolors; i++)
		{

			pts += (i < 3) ? Mathf.FloorToInt((float)colorcards[i] * (colorcards[i] + 1.0f) * 0.5f) :
				-Mathf.FloorToInt((float)colorcards[i] * (colorcards[i] + 1.0f) * 0.5f);

		}


		pts += plus2 * 2;
		return pts;
	}

	public int DeltaPointsAfterPile(Pile pile)
	{

		int val = points;

		List<Card> nst = new List<Card>();
		nst.AddRange(cards);
		nst.AddRange(pile.cards);
		int newpoints = ComputePoints(nst);

		return newpoints - val;

	}
	public int PointsAfterPile(Pile pile)
	{

		List<Card> nst = new List<Card>();
		nst.AddRange(cards);
		nst.AddRange(pile.cards);
		int newpoints = ComputePoints(nst);

		return newpoints;

	}
	public int EntropyAfterPile(Pile pile)
	{

		int[] newstock = new int[stock.Length];
		for (int i = 0; i < newstock.Length; i++)
			newstock[i] = stock[i];
		foreach (Card c in pile.cards)
			newstock[c.cardindex]++;

		int entropy = 0;
		for (int i = 0; i < stock.Length-2; i++)
			if (newstock[i] > 0)
				entropy++;

		return entropy;
	}

	public void UpdateScore()
	{
		namescore.text = ComputePoints(cards).ToString();
		Debug.Log("player points " + points.ToString());
	}

	public void Clear()
	{

		foreach (Card c in cards)
		{
			c.animator.FadeOut(0, 1);
			Destroy(c.gameObject, 2.0f);
		}
		cards.Clear();
		for (int i = 0; i < stock.Length; i++)
			stock[i] = 0;

		UpdateScore();

	}


}
