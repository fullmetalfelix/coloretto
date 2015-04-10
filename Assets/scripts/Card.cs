using UnityEngine;
using System.Collections;



public class Card : Sprite {

	//public Texture2D texture;
	//public Color basecolor;
	public int cardindex;

	//public Texture2D icondown;

	//public Unit unit;
	//public float scalefactor = 10;
	//public float aspectratio = 1;
	//protected GameObject meshobject;


	//protected Vector3 screenpos;
	//protected Vector3 screenupright;
	//protected Vector3 yp, zparent;



	protected GameLogic logic;

	public static Card Fabricate(GameObject template, Color color, int index)
	{
		GameObject c = (GameObject)Instantiate(template);
		Card s = c.GetComponent<Card>();
		s.meshobject = s.transform.FindChild("mesh").gameObject;
		s.SetColor(color);
		s.cardindex = index;
		s.animator.FadeIn(0, 1, 1);
		s.animator.SizePulse(1, 1.5f, 0.8f);
		s.animator.Flip(180, 0.8f);
		//s.animator.Roll(30, 0.8f);

		s.logic = GameObject.Find("master").GetComponent<GameLogic>();
		s.texture = s.logic.cardtex[index];
		s.scalefactor = s.logic.cardsize;
		s.aspectratio = s.logic.cardaspect;

		return s;
	}
	public static Card Fabricate(GameObject template)
	{
		GameObject c = (GameObject)Instantiate(template);
		Card s = c.GetComponent<Card>();

		s.logic = GameObject.Find("master").GetComponent<GameLogic>();
		s.scalefactor = s.logic.cardsize;
		s.aspectratio = s.logic.cardaspect;
		
		return s;
	}


	public void SendToStock(PlayerStock stock)
	{
		//ask the stock where this goes
		Vector4 p = stock.GetNewCardPosition(this);
		targetpos = p;
		tangle = p.w;
		//tangle = Vector3.Angle(stock.transform.right, transform.up);
		animator.Roll(tangle, 0.8f);
		ismoving = true;

	}

}
