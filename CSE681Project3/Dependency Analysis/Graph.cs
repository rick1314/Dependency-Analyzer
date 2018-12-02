///////////////////////////////////////////////////////////////////////////
// Graph.cs  -  Graph class with Strongly Connected Components Function  //
// ver 1.1                                                               //
//  Language:     C#, VS 2017                                            //
//  Platform:     Asus GL552VW, Windows 2010 Home                        //
//  Application:  Generates Strongly Connected Components                //
//  Author:       Debopriyo Bhattacharya, Syracuse University            //
//                (315) 278 7480, debhatta@syr.edu                       //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * Takes input of node values and dependencies then adds it to a graph. 
 * Then implements Tarjan's Algorithm on the graph to find strong components.
 *  
 * Build Process
 * =============
 * Required Files:
 *   Graph.cs
 *   
 * Compiler Command:
 *   csc /define:Test_Graph Graph.cs
 *   
 * Maintenance History
 * ===================
 * ver 1.1 : 31 Oct 2018
 * - made required modifications to solve Project #3 of CSE 681
 * ver 1.0 : 8 April 2016
 * - first release by Roger Hill on Stackoverflow https://stackoverflow.com/a/36489758
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependency_Analysis
{
    public class Vertex
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public int Lowlink { get; set; }

        public HashSet<Vertex> Dependencies { get; set; }

        public Vertex()
        {
            Id = -1;
            Name = "";
            Index = -1;
            Lowlink = -1;
            Dependencies = new HashSet<Vertex>();
        }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Vertex other = obj as Vertex;

            if (other == null)
                return false;

            return Id == other.Id;
        }
    }

    public class TarjanCycleDetectStack
    {
        protected List<List<Vertex>> _StronglyConnectedComponents;
        protected Stack<Vertex> _Stack;
        protected int _Index;

        public List<List<Vertex>> DetectCycle(List<Vertex> graph_nodes)
        {
            _StronglyConnectedComponents = new List<List<Vertex>>();

            _Index = 0;
            _Stack = new Stack<Vertex>();

            foreach (Vertex v in graph_nodes)
            {
                if (v.Index < 0)
                {
                    StronglyConnect(v);
                }
            }

            return _StronglyConnectedComponents;
        }

        private void StronglyConnect(Vertex v)
        {
            v.Index = _Index;
            v.Lowlink = _Index;

            _Index++;
            _Stack.Push(v);

            foreach (Vertex w in v.Dependencies)
            {
                if (w.Index < 0)
                {
                    StronglyConnect(w);
                    v.Lowlink = Math.Min(v.Lowlink, w.Lowlink);
                }
                else if (_Stack.Contains(w))
                {
                    v.Lowlink = Math.Min(v.Lowlink, w.Index);
                }
            }

            if (v.Lowlink == v.Index)
            {
                List<Vertex> cycle = new List<Vertex>();
                Vertex w;

                do
                {
                    w = _Stack.Pop();
                    cycle.Add(w);
                } while (v != w);

                _StronglyConnectedComponents.Add(cycle);
            }
        }

#if(Test_Graph)
        public static void Main()
        {
            // tests simple model presented on https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
            var graph_nodes = new List<Vertex>();

            var v1 = new Vertex() { Id = 1, Name = "One" };
            var v2 = new Vertex() { Id = 2, Name = "Two" };
            var v3 = new Vertex() { Id = 3, Name = "Three" };
           /*
            var v4 = new Vertex() { Id = 4, Name = "Four" };
            var v5 = new Vertex() { Id = 5, Name = "Five" };
            var v6 = new Vertex() { Id = 6, Name = "Six" };
            var v7 = new Vertex() { Id = 7, Name = "Seven" };
            var v8 = new Vertex() { Id = 8, Name = "Eight" };
        */
            v1.Dependencies.Add(v2);
            v2.Dependencies.Add(v1);
         /*   v3.Dependencies.Add(v1);
            v4.Dependencies.Add(v3);
            v4.Dependencies.Add(v5);
            v5.Dependencies.Add(v4);
            v5.Dependencies.Add(v6);
            v6.Dependencies.Add(v3);
            v6.Dependencies.Add(v7);
            v7.Dependencies.Add(v6);
            v8.Dependencies.Add(v7);
            v8.Dependencies.Add(v5);
            v8.Dependencies.Add(v8);
        */
            graph_nodes.Add(v1);
            graph_nodes.Add(v2);
            graph_nodes.Add(v3);
          /*  graph_nodes.Add(v4);
            graph_nodes.Add(v5);
            graph_nodes.Add(v6);
            graph_nodes.Add(v7);
            graph_nodes.Add(v8);
        */
            var tcd = new TarjanCycleDetectStack();
            var cycle_list = tcd.DetectCycle(graph_nodes);

            Console.WriteLine("Total number of cycles: {0}", cycle_list.Count);


            foreach (var vertices in cycle_list)
            {
                //Console.WriteLine(v);

                foreach (var v in vertices)
                {
                    Console.Write(v.ToString());
                    // Console.WriteLine(string.Join(" ", v.Dependencies));
                }
                Console.Write("\n");

            }
            Console.ReadKey();
            //Assert.IsTrue(cycle_list.Count == 4);
        }
#endif
    }

}
