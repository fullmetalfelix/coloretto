using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;



public enum GamePhases
{
	SettingUp = 0,
	DeckOrPile = 10,
	CardToPile = 20,
	Dummy

}
public enum SeatType
{
	Human = 0,
	AI1 = 10,
	Empty

}

public class GameLogic : MonoBehaviour {

	public GamePhases phase = GamePhases.SettingUp;

	public float tableaspect = 1;
	public float tablesize = 100;
	public float cardsize, cardspacing, cardvspacing;
	public float cardaspect = 0.636f;


	public Deck deck;
	public GameObject decktemplate;
	//public int[] availablecards;
	//public double[] pickchance;
	public Color[] cardcolors;
	public Texture2D[] cardtex;
	public GameObject cardtemplate;




	public bool pilesallfull { get { return (PilesAllFull() == 0) ? true : false; } }
	//public List<List<int>> piles;
	public List<Pile> pilelist;
	//public int[,] pilevalues, hypopilevalues;
	public GameObject piletemplate;
	public Texture2D[] piletex;
	

	public int ncolors = 7;
	public int nplayers = 4;

	public bool lastround = false;


	protected Dictionary<string, double> gameparams;
	protected void InitParams()
	{

		gameparams = new Dictionary<string, double>();

		for (int i = 0; i < nplayers; i++)
		{

			gameparams.Add("p" + (i + 1).ToString() + "_score", 0);
			gameparams.Add("p" + (i + 1).ToString() + "_pass", 0);

			for (int j = 0; j < pilelist.Count; j++)
			{

				gameparams.Add("p" + (i + 1).ToString() + "_pile" + (j + 1).ToString() + "value", 0);
				gameparams.Add("p" + (i + 1).ToString() + "_pile" + (j + 1).ToString() + "Hvalue", 0);
			}
		}


		for (int j = 0; j < pilelist.Count; j++)
		{
			gameparams.Add("pile" + (j + 1).ToString() + "_pickable", 0);
			for (int i = 0; i < 3; i++)
			{
				gameparams.Add("pile" + (j + 1).ToString() + "_card" + (i + 1).ToString(), 0);
			}
		}
		gameparams.Add("deck_pickable", 0);
		gameparams.Add("freeslots", 0);
		gameparams.Add("pickedcard", 0);

		for (int i = 0; i < ncolors + 2; i++)
		{
			gameparams.Add("card" + (i + 1).ToString() + "_prob", 0);
		}



	}



	#region "player Setup"

	public int currentplayer;
	protected int[] playerscores; //scores for the single round
	protected int[] playerscorestot; // total score for game
	public SeatType[] playerAI;
	public AIPlayer[] players;
	public bool[] playerout;
	public Vector3[] playerstockpoints;
	public PlayerStock[] playerstocks;
	public GameObject stocktemplate;
	public Texture2D[] playericons;

	/// <summary>
	/// Setup the player icons, stocks, ...
	/// </summary>
	/// <param name="pltypes"></param>
	protected void PlayerSetup(int[] pltypes)
	{

		nplayers = pltypes.Length;

		
		playerscorestot = new int[nplayers];

		playerAI = new SeatType[nplayers];
		players = new AIPlayer[nplayers];
		playerstockpoints = new Vector3[nplayers];
		playerstocks = new PlayerStock[nplayers];

		for (int i = 0; i < nplayers; i++)
		{
			playerscorestot[i] = 0;

			if (pltypes[i] == 0) playerAI[i] = SeatType.Human;
			else if (pltypes[i] == 1) playerAI[i] = SeatType.AI1;

			//load an AI depending on the difficulty
			players[i] = new AIPlayer("ai_0");
			

			//determine where the playerstock goes
			Vector3 pos = Vector3.up * tablesize;
			Quaternion q = Quaternion.AngleAxis(-i * 360.0f / nplayers, Vector3.forward);
			pos = q * pos; pos.y /= tableaspect;

			playerstocks[i] = PlayerStock.Fabricate(stocktemplate,  pos, playericons[pltypes[i]]);

		}

		ScoreBoard scores = ScoreBoard.Fabricate(pltypes);

	}


	#endregion


	/// <summary>
	/// Create the props for the game. This is executed after main menu.
	/// </summary>
	/// <param name="pltypes"></param>
	public void SetupTable(int[] pltypes)
	{

		PlayerSetup(pltypes);

		deck = Deck.Fabricate(decktemplate);

		InitParams(); //game parameters for AI

		StartCoroutine(MakePiles());

		SetupGame();

	}

	public void SetupGame()
	{

		currentplayer = Random.Range(0, nplayers - 1);
		playerstocks[currentplayer].animator.SizeOscillate(0.9f, 1.3f, 1.5f);

		playerscores = new int[nplayers];

		#region "decide starting colors"

		int[] startingcolors = new int[ncolors];
		for (int i = 0; i < ncolors; i++)
			startingcolors[i] = i;
		startingcolors = GameLogic.Shuffle<int>(startingcolors); //randomize

		//the first 4 are for the players - remove from deck
		int[] cardspercolor = new int[ncolors];
		for (int i = 0; i < ncolors; i++)
			cardspercolor[i] = 9;
		for (int i = 0; i < nplayers; i++)
		{
			cardspercolor[startingcolors[i]]--;
		}

		StartCoroutine(SetStartCards(startingcolors));

		#endregion

		deck.MakeDeck(cardspercolor);

		//DEBUG
		for (int k = 0; k < 10; k++)
		{
			for (int i = 0; i < nplayers; i++)
			{
				//int picked = deck.PickCardFromDeck();
				//Card crd = Card.Fabricate(cardtemplate, cardcolors[picked], picked);
				//crd.SendToStock(playerstocks[i]);
			}
		}


		StartCoroutine(WaitToStart(4.0f));

	}

	protected IEnumerator SetStartCards(int[] startingcolors)
	{
		for (int i = 0; i < nplayers; i++)
		{
			Card crd = Card.Fabricate(cardtemplate, cardcolors[startingcolors[i]],
				startingcolors[i]);
			//crd.SendTo(playerstockpoints[i], Mathf.Atan2(playerstockpoints[i].y,
			//playerstockpoints[i].x) * Mathf.Rad2Deg);
			crd.SendToStock(playerstocks[i]);
			yield return new WaitForSeconds(0.5f);
		}
	}

	protected IEnumerator MakePiles()
	{

		playerout = new bool[nplayers];
		pilelist = new List<Pile>(nplayers);
		//pilepicked = new bool[nplayers];
		//piles = new List<List<int>>(nplayers);
		//pilevalues = new int[nplayers, nplayers];
		//hypopilevalues = new int[nplayers, nplayers];

		for (int i = 0; i < nplayers; i++)
		{
			List<int> pile = new List<int>(nplayers);
			//pilepicked[i] = false;
			playerout[i] = false;
			//piles.Add(pile);

			Pile p = Pile.Fabricate(piletemplate, piletex[2]);
			
			p.pileID = i;
			Vector3 v = Vector3.zero;
			v.y = 1.6f * cardspacing * ((nplayers - 1) * 0.5f - i);
			v.x = 1.5f * cardspacing;

			pilelist.Add(p);
			p.SendTo(v, 90);
			yield return new WaitForSeconds(0.3f);

		}

	}


	public void GameCyclasde()
	{

		phase = GamePhases.DeckOrPile;

		#region "check pile status"
		bool pilesfull = true;
		bool pilesempty = true;
		for (int i = 0; i < pilelist.Count; i++)
		{
			pilesfull = pilesfull && (pilelist[i].cards.Count == pilelist[i].maxcards || pilelist[i].taken);
			pilesempty = pilesempty && (pilelist[i].cards.Count == 0);
		}

		#endregion

		if (playerAI[currentplayer] != SeatType.Human &&
			playerAI[currentplayer] != SeatType.Empty)
		{
			//ai player
		}
		else if (playerAI[currentplayer] == SeatType.Human)
		{
			//human player

			

		}


	}


	protected void GameStatus()
	{

		int plid = currentplayer;

		#region "value of the piles for each player"
		plid = currentplayer;
		for (int pl = 0; pl < nplayers; pl++)
		{
			for (int i = 0; i < pilelist.Count; i++)
			{
				string iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "value";
				gameparams[iname] = playerstocks[plid].DeltaPointsAfterPile(pilelist[i]);

				iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "score";
				gameparams[iname] = playerstocks[plid].PointsAfterPile(pilelist[i]);

				iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "entropy";
				gameparams[iname] = playerstocks[plid].EntropyAfterPile(pilelist[i]);

			}
			plid++; if (plid == nplayers) plid = 0;
		}

		#endregion

		#region "status of the players - if they passed the turn"
		plid = currentplayer;
		for (int pl = 0; pl < nplayers; pl++)
		{
			string iname = "p" + (pl + 1).ToString() + "_pass";
			gameparams[iname] = (playerout[plid]) ? 0 : 1;

			iname = "p" + (pl + 1).ToString() + "_score";
			gameparams[iname] = playerstocks[plid].points;

			plid++; if (plid == nplayers) plid = 0;
		}
		#endregion

		#region "pickability of piles and deck & piles content & freeslots"
		int freepilespaces = 0;
		bool pilesfull = true;
		bool pilesempty = true;
		for (int i = 0; i < pilelist.Count; i++)
		{

			if (!pilelist[i].taken)
				freepilespaces = pilelist[i].maxcards - pilelist[i].cards.Count;

			pilesfull = pilesfull && (pilelist[i].cards.Count == pilelist[i].maxcards || pilelist[i].taken);
			pilesempty = pilesempty && (pilelist[i].cards.Count == 0 || pilelist[i].taken);

			string iname = "pile" + (i + 1).ToString() + "_pickable";

			gameparams[iname] = (pilelist[i].taken) ? -1 : pilelist[i].cards.Count;

			for (int j = 0; j < 3; j++)
			{
				iname = "pile" + (i + 1).ToString() + "_card" + (j + 1).ToString();
				gameparams[iname] = -1;
				if (!pilelist[i].taken)
				{ //if the pile is not picked
					if (j < pilelist[i].cards.Count)
						gameparams[iname] = pilelist[i].cards[j].cardindex;
				}
			}

		}
		gameparams["freeslots"] = freepilespaces;
		gameparams["deck_pickable"] = (pilesfull) ? 0 : 1;

		#endregion

		#region "pick chances"
		for (int i = 0; i < ncolors + 2; i++)
		{
			gameparams["card" + i.ToString() + "_prob"] = deck.CardTypeProbability(i);
		}
		#endregion

	}

	public void DoDeckOrPile()
	{

		phase = GamePhases.DeckOrPile;
		Debug.Log("new Deck/Pile... player "+currentplayer.ToString());
		
		//update scores?
		foreach (PlayerStock pls in playerstocks)
		{
			pls.UpdateScore();
		}


		//void the pickedcard
		gameparams["pickedcard"] = -10;
		GameStatus();

		StopGraphicsEffects();

		//if the player is AI, connect the inputs and perform decision
		if (playerAI[currentplayer] != SeatType.Human)
		{
			//connect inputs
			Debug.Log("AI player " + currentplayer.ToString() + " is playing...");

			//clear all inputs
			players[currentplayer].ClearInputs();


			#region "make a decision to pick pile or card"
			
			string log = "player prefs: ";

			players[currentplayer].SetInputs(gameparams);
			OrderedDictionary outputs = players[currentplayer].PileOrDeck();

			#region "process output"
			//the first 4 out puts need to be post processed
			List<float> prefs = new List<float>(pilelist.Count + 1);
			float[] prefs2 = new float[pilelist.Count + 1];

			for (int i = 0; i < pilelist.Count + 1; i++)
			{

				string oname = "pref_" + i.ToString();

				double preforg = (double)outputs[oname];
				double pref = 0.5 * (preforg + 1);

				string iname = "pile" + (i + 1).ToString() + "_pickable";
				if (i == pilelist.Count)
					iname = "deck_pickable";

				//this sets the preference of picked and empty piles to 0 - multiply by pickability
				pref *= (gameparams[iname] <= 0) ? 0 : 1;
				prefs.Add((float)pref);
				prefs2[i] = (float)pref;

				log += pref.ToString() + " ";

				//outputs[oname] = pref; //correct the output... why?

			}
			int choice = prefs.IndexOf(Mathf.Max(prefs2));
			log += "chose " + choice.ToString();
			Debug.Log(log);
			#endregion 
			#endregion

			#region "chose to pick a pile"
			if (choice < pilelist.Count)
			{

				while (pilelist[choice].taken)
				{
					prefs2[choice] = float.NegativeInfinity;
					prefs[choice] = float.NegativeInfinity;
					choice = prefs.IndexOf(Mathf.Max(prefs2));
				}

				if (pilelist[choice].taken)
				{
					Debug.LogError("ERROR! AI is trying to pick a picked pile!");
				}

				Debug.Log("AI player " + currentplayer.ToString() + " picks pile " + choice.ToString());
				StartCoroutine(AIpicksPile(choice));

			}
			#endregion
			#region "pick a card"
			else
			{
				Debug.Log("AI player " + currentplayer.ToString() + " get card! ");

				int picked = deck.PickCardFromDeck();

				#region "set the rest of the inputs"
				gameparams["pickedcard"] = picked;

				//copy hypotetical values to the inputs
				int plid = currentplayer;
				for (int pl = 0; pl < nplayers; pl++)
				{
					for (int i = 0; i < pilelist.Count; i++)
					{
						string iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "Hvalue";
						gameparams[iname] = playerstocks[plid].DeltaStockValueAfterPilePlusCard(pilelist[i], deck.lastpickedcard);

						iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "Hscore";
						gameparams[iname] = playerstocks[plid].StockValueAfterPilePlusCard(pilelist[i], deck.lastpickedcard);

						iname = "p" + (pl + 1).ToString() + "_pile" + (i + 1).ToString() + "Hentropy";
						gameparams[iname] = playerstocks[plid].EntropyAfterPilePlusCard(pilelist[i], deck.lastpickedcard);
					}
					plid++; if (plid == nplayers) plid = 0;
				}
				#endregion

				//decide where to put the card
				players[currentplayer].SetInputs(gameparams);
				outputs = players[currentplayer].CardToPile();

				//process the last 3 outputs to see where to put the card
				prefs = new List<float>(pilelist.Count);
				prefs2 = new float[pilelist.Count];
				log = "player prefers pile: ";
				
				for (int i = 0; i < pilelist.Count; i++)
				{
					string oname = "cardpile_" + i.ToString();

					float pref = System.Convert.ToSingle(outputs[oname]);
					pref = 0.5f * (pref + 1);
					pref *= (pilelist[i].cards.Count < pilelist[i].maxcards && (!pilelist[i].taken)) ? 1 : 0;
					prefs.Add(pref);
					prefs2[i] = pref;

					log += pref.ToString() + " ";

					outputs[oname] = pref; 
				}
				choice = prefs.IndexOf(Mathf.Max(prefs2));

				while (pilelist[choice].taken || pilelist[choice].cards.Count == pilelist[choice].maxcards)
					choice = (choice + 1) % pilelist.Count;

				if (pilelist[choice].taken)
				{
					Debug.LogError("ERROR! AI is putting a card in a picked pile");
				}

				log += "chose pile " + choice.ToString();
				Debug.Log(log);
				StartCoroutine(AIchosesPileforCard(choice));
			}
			#endregion


		}
		else //just do graphics effects if non AI
		{
			//if human player mark the things that could be clicked
			if (!pilesallfull)
			{
				//blink the deck if the piles are not all full
				deck.animator.SizeOscillate(0.8f, 1.4f, 1.0f);

			}

			//piles can be taken only if not taken and not empty
			for (int i = 0; i < pilelist.Count; i++)
			{
				if (!pilelist[i].empty && !pilelist[i].taken)
				{
					pilelist[i].animator.SizeOscillate(0.8f, 1.4f, 1.0f);
				}
			}



		}

		

	}

	private void StopGraphicsEffects()
	{
		StopGraphicsEffects(0.5f);
	}
	private void StopGraphicsEffects(float time)
	{

		deck.animator.SizeOscillateStop(time);

		for (int i = 0; i < pilelist.Count; i++)
		{
			pilelist[i].animator.SizeOscillateStop(time);
		}

	}
	public void PlayerChosesDeck()
	{
		StopGraphicsEffects(0.1f);

		//piles can be chosen for the picked card only if not taken and not full
		for (int i = 0; i < pilelist.Count; i++)
		{
			if (pilelist[i].full || pilelist[i].taken)
			{
				pilelist[i].animator.SizeOscillateStop(0.5f);
			}
			else
			{
				pilelist[i].animator.SizeOscillate(0.8f, 1.4f, 1.0f);
			}
		}

	}


	/// <summary>
	/// Returns 0 if all piles are full/taken, 1 if there are free spaces
	/// </summary>
	/// <returns></returns>
	public int PilesAllFull()
	{
		bool allfull = true;
		foreach (Pile p in pilelist)
		{
			if (p.taken) continue;
			allfull = allfull && p.full;
		}
		return (allfull) ? 0 : 1;
	}

	protected IEnumerator AIchosesPileforCard(int choice)
	{
		yield return new WaitForSeconds(1.0f);

		pilelist[choice].PutCardInPile();
		StartCoroutine(SetNextPlayer());

	}
	protected IEnumerator AIpicksPile(int choice)
	{
		yield return new WaitForSeconds(0.5f);
		pilelist[choice].PickPile();
		yield return new WaitForSeconds(2.0f);
		StartCoroutine(SetNextPlayer());
	}



	public IEnumerator SetNextPlayer()
	{

		playerstocks[currentplayer].animator.SizeOscillateStop(0.5f);
		Debug.Log("picking next player... ");
		yield return new WaitForSeconds(1.0f);

		//check if all players are out - end the round
		bool allplout = true;
		for (int i = 0; i < nplayers; i++)
			allplout = allplout && playerout[i];
		if (allplout)
		{
			if (deck.lastround) //end of game
			{
				Debug.Log("game ended!");

				//compute scores
				

				#region "reset table"
				foreach (PlayerStock stock in playerstocks)
					stock.Clear();
				foreach (Pile pile in pilelist)
					pile.Reset();

				deck.Reset();
				#endregion

			}
			else
			{
				StartCoroutine(NewRound());
			}


		}
		else
		{


			currentplayer = (currentplayer + 1) % nplayers;

			while (playerout[currentplayer] == true)
			{
				currentplayer = (currentplayer + 1) % nplayers;
			}

			//pilesallfull = true;
			//foreach (Pile p in pilelist)
			//    pilesallfull = pilesallfull && (p.full || p.taken);

			Debug.Log("next player is: " + currentplayer.ToString());

			//make the player icon of current player flash
			playerstocks[currentplayer].animator.SizeOscillate(0.9f, 1.3f, 1.5f);

			DoDeckOrPile();

		}
	}

	public IEnumerator WaitToStart(float time)
	{
		yield return new WaitForSeconds(time);
		DoDeckOrPile();
	}

	protected IEnumerator NewRound()
	{
		phase = GamePhases.Dummy;
		Debug.Log("all players picked... next round");

		yield return new WaitForSeconds(1.0f);
		
		//restore piles
		foreach (Pile p in pilelist)
			p.Reset();
		playerout = new bool[nplayers]; //reset the playerout
		//pilesallfull = false;

		yield return new WaitForSeconds(1.0f);

		DoDeckOrPile();

	}


	public static T[] Shuffle<T>(T[] array)
	{
		T[] shuffledArray = new T[array.Length];
		int rndNo;
		Random rnd = new Random();

		for (int i = array.Length; i >= 1; i--)
		{
			rndNo = Random.Range(1, i) - 1;
			shuffledArray[i - 1] = array[rndNo];
			array[rndNo] = array[i - 1];
		}
		return shuffledArray;
	}
	


}
