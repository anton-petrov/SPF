/*
 
 Dijkstra's algorithm, conceived by computer scientist Edsger Dijkstra in 1956 and published in 1959,[1][2] 
 is a graph search algorithm that solves the single-source shortest path problem for a graph with non-negative
 edge path costs, producing a shortest path tree. This algorithm is often used in routing and as a subroutine 
 in other graph algorithms. 
 
 Author: Anton Petrov, 2005-2014.
 */

using System;
using System.Collections;
using System.Globalization;

namespace SPF
{
	//  Shrotest Path First

	abstract class MathConsts
	{
		public const int INFINITE = (Int32.MaxValue / 2) - 1; 
	}

	class Metric
	{
		public int metric;			
		public string StringPath;	
		public VertexSet intPath;
		public Metric()
		{
			intPath = new VertexSet();
		}
	}

	class VertexSet : ArrayList
	{
		public VertexSet()
		{
		}
		public VertexSet(int ñapacity)
		{
			this.Capacity = ñapacity;
		}
		public void AddVertex(int vertex)
		{
			this.Add(vertex);
		}
		public bool HasVertex(int vertex)
		{
			foreach(int e in this)
			{
				if((int)(e) == vertex)
					return true;
			}
			return false;
		}
		public bool HasVertexes(int[] vertexes)
		{
			bool result = true;
			for(int i = 0; i < this.Count; i++)
			{
				for(int j = 0; j < vertexes.Length; j++)
				{
					if(vertexes[j] != Convert.ToInt32(this[i]))
						result = false;
				}
			}
			return result;
		}
		public int GetVertex(int i)
		{
			return Convert.ToInt32(this[i]);
		}
		public void PrintVertexes()
		{
			foreach(int e in this)
				Console.Write(e.ToString(CultureInfo.InvariantCulture) + " ");
			Console.WriteLine();
		}
	}

	class DijkstraAlgorithm
	{
		private VertexSet vertexSet;	
		public/*private*/ int[,] matrix;
		private int[] weight;			

		private int width;
		private int height;
		const int INF = MathConsts.INFINITE;

		private VertexSet[] v_path;		
		private int startVertex = 0;	

		public DijkstraAlgorithm()
		{
		}

		public DijkstraAlgorithm(int[,] adjacencyMatrix)
		{
			SetMatrix(adjacencyMatrix);			
		}

		public void SetMatrix(int[,] adjacencyMatrix)
		{
			int size = Convert.ToInt32(Math.Sqrt(System.Convert.ToDouble(adjacencyMatrix.Length)));
			width = size;
			height = size;
			matrix = new int[size, size];
			weight = new int[size];
			vertexSet = new VertexSet(size);
			v_path = new VertexSet[size];
			for(int i = 0; i < v_path.Length; i++)
				v_path[i] = new VertexSet(size);
			matrix = (int[,])adjacencyMatrix.Clone();
		
		}
		public void PrintWeights()
		{
			int i = 0;
			foreach(int e in weight)
				Console.Write("[" + (i++).ToString() + "] = " + e.ToString() + " ");
		}
		public void PrintVertexes()
		{
			vertexSet.PrintVertexes();
		}

		public Metric GetMetric(int a, int b)
		{
			int b1 = a, a1 = b;
			if(a > b)
			{
				a = a1;
				b = b1;
			}
			ShortestPath(a);			
			return GetMetric(b);		
		}

		private Metric GetMetric(int a) 
		{
			Metric result = new Metric();
			result.metric = weight[a];
			int i = v_path[a].Count-1;

			bool isolated = true;		
			foreach(int j in weight)
			{
				if(j < MathConsts.INFINITE && j > 0)
				{
					isolated = false;
					break;
				}
			}
			if(isolated)
				return result;

			if(startVertex != a) 
				while(true)
				{
					result.intPath.AddVertex(v_path[a].GetVertex(i));
					i--;
					if(i <= 0)
						break;
					a = v_path[a].GetVertex(i);
					i = v_path[a].Count-1;
				}

			result.intPath.AddVertex(startVertex);
			result.intPath.Reverse();
			foreach(int e in result.intPath)
				result.StringPath += (e+1).ToString() + " ";

			return result;
		}

		public void ShortestPath(int _startVertex = 0)
		{
			startVertex = _startVertex;
			int lastVertex = 0;
			int begin = (width > 2 ? 2 : 1);
			int v = startVertex;

			for(int i = 0; i < width; i++)
				weight[i] = matrix[startVertex, i];
			vertexSet.Add(v);
			for(int i = 0; i < v_path.Length; i++)
				v_path[i].AddVertex(v);
			for(int step = begin; step < width; step++)
			{
				v = FindMinWeight();
				vertexSet.AddVertex(v);
				v_path[v].AddVertex(v);
				for(int u = 0; u < width; u++)
				{
					if(!vertexSet.HasVertex(u))
					{
						if(weight[u] > weight[v] + matrix[v, u])
						{
							weight[u] = weight[v] + matrix[v, u];
							v_path[u].AddVertex(v);
						}
						lastVertex = u;
					}
				
				}
			}
			vertexSet.AddVertex(lastVertex);
			v_path[lastVertex].AddVertex(lastVertex);
		}

		private int FindMinWeight()
		{
			int min = INF;
			int min_index = 0;
			for(int i = 0; i < weight.Length; i++)
			{
				if (!vertexSet.HasVertex(i) && weight[i] < min)
				{
					min_index = i;
					min = weight[i];
				}
			}
			return min_index;
		}

	}
}