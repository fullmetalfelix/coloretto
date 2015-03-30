using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System;


public class NeuralNetwork
{

	public OrderedDictionary inputs, outputs;
	protected string topofile;

	public double fitness = 0;
	public int roundswon = 0, roundsplayed = 0;

	protected List<double[]> neurons;
	protected List<double[]> bias;
	protected List<double[,]> links;

	protected System.Random rnd;

	protected double maxlink = 1.0;
	protected double maxbias = 10.0;


	
	public NeuralNetwork(XmlNode main)
	{
		rnd = new System.Random();

		#region "inputs"
		XmlNode mainins = main.SelectSingleNode("inputs");
		XmlNodeList ins = mainins.SelectNodes("input");
		inputs = new OrderedDictionary();
		foreach (XmlNode node in ins)
		{
			inputs.Add(node.Attributes["name"].Value, 0.0);
		}

		#endregion
		#region "outputs"
		XmlNode mainouts = main.SelectSingleNode("outputs");
		XmlNodeList outs = mainouts.SelectNodes("output");
		outputs = new OrderedDictionary();
		foreach (XmlNode node in outs)
		{
			outputs.Add(node.Attributes["name"].Value, 0.0);
		}

		#endregion
		#region "make layers"
		XmlNode mainlayers = main.SelectSingleNode("layers");
		XmlNodeList layers = mainlayers.SelectNodes("layer");

		bias = new List<double[]>(layers.Count); //bias for layers + outputs
		neurons = new List<double[]>(layers.Count + 1); //one for each hidden + in + out
		neurons.Add(new double[inputs.Count]); //add the input layer

		foreach (XmlNode node in layers)
		{ //add the hidden layers and output
			int size = int.Parse(node.Attributes["size"].Value);
			neurons.Add(new double[size]);

			string[] words = node.InnerText.Split(new char[] {'\t', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			double[] b = new double[size];

			for (int i = 0; i < size; i++)
				b[i] = double.Parse(words[i]);
			bias.Add(b);

		}

		//neurons.Add(new double[outputs.Count]); //add the output layer
		//bias.Add(new double[outputs.Count]); //add biases for output layer

		#endregion

		#region "make links"

		XmlNode mainlinks = main.SelectSingleNode("links");
		XmlNodeList lnks = mainlinks.SelectNodes("link");

		links = new List<double[,]>(layers.Count);
		//links.Add(new double[inputs.Count, bias[0].Length]); //connection input->hidden0

		for (int k = 1; k <= layers.Count; k++)
		{
	
			int x = neurons[k - 1].Length;
			int y = neurons[k].Length;
			
			double[,] linkmatrix = new double[x, y];

			//Debug.Log(lnks[k - 1].InnerText);
			string[] words = lnks[k-1].InnerText.Split(new char[] { '\t', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < x; i++)
			{
				for (int j = 0; j < y; j++)
				{
					linkmatrix[i, j] = double.Parse(words[i * y + j]);
				}
			}

			links.Add(linkmatrix); //connection hiddeni->hiddeni-1
		}

		#endregion

	}

	public void RandomizeNeurons()
	{

		foreach (double[] d in bias)
			for (int i = 0; i < d.Length; i++)
			{
				d[i] = 2 * maxbias * (rnd.NextDouble() - 0.5);
			}

	}
	public void RandomizeLinks()
	{

		foreach (double[,] matrix in links)
		{
			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					matrix[i, j] = 2 * maxlink * (rnd.NextDouble() - 0.5);
				}
			}

		}

	}

	public void ClearInputs()
	{

		for (int i = 0; i < inputs.Count; i++)
			inputs[i] = 0;

	}

	public void Update()
	{
		#region "copy the input from dictionary"
		double[] vals = new double[inputs.Count];
		inputs.Values.CopyTo(vals, 0);

		for (int i = 0; i < inputs.Count; i++)
		{
			neurons[0][i] = vals[i];
		}
		#endregion

		for (int layer = 1; layer < neurons.Count; layer++)
		{ //loop over the layers+out

			//loop over the neurons to be updated
			for (int output = 0; output < neurons[layer].Length; output++)
			{
				neurons[layer][output] = bias[layer - 1][output];

				//loop over the neurons in the previous layer
				for (int input = 0; input < neurons[layer - 1].Length; input++)
				{
					neurons[layer][output] += neurons[layer - 1][input] * links[layer - 1][input, output];
				}

				neurons[layer][output] = Math.Tanh(neurons[layer][output]);

			}
		}

		#region "copy the output to dictionary"
		for (int i = 0; i < outputs.Count; i++)
		{
			outputs[i] = neurons[neurons.Count - 1][i];
		}
		#endregion


	}


}



