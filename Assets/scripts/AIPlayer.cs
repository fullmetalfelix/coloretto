using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;


public class AIPlayer {

	protected NeuralNetwork decider, cardput;

	public AIPlayer(string ai)
	{

		XmlDocument xml = new XmlDocument();

		TextAsset txt = Resources.Load(ai) as TextAsset;  //load the xml from resources
		xml.Load(new MemoryStream(txt.bytes));  //load the xml data

		XmlNode main = xml.SelectSingleNode("networks");
		XmlNodeList nets = main.SelectNodes("network");
		
		decider = new NeuralNetwork(nets[0]);
		cardput = new NeuralNetwork(nets[1]);

	}


	public void ClearInputs()
	{

		for (int i = 0; i < decider.inputs.Count; i++)
			decider.inputs[i] = 0;
		for (int i = 0; i < cardput.inputs.Count; i++)
			cardput.inputs[i] = 0;

	}

	public void SetInputs(Dictionary<string, double> gamevals)
	{

		foreach (string key in gamevals.Keys)
		{

			if (decider.inputs.Contains(key))
				decider.inputs[key] = gamevals[key];
			if (cardput.inputs.Contains(key))
				cardput.inputs[key] = gamevals[key];
		}

	}

	public OrderedDictionary PileOrDeck()
	{

		decider.Update();

		return decider.outputs;
	}

	public OrderedDictionary CardToPile()
	{

		cardput.Update();

		return cardput.outputs;
	}
		


}
