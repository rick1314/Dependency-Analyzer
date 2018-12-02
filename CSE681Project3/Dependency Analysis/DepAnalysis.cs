///////////////////////////////////////////////////////////////////////
// DepeAnalysis.cs - Analyzes all the files and finds their relative //
//                   dependency and shows all the strong components  //
// ver 1.0                                                           //
// Language:    C#, 2017, .Net Framework 4.7.1                       //
// Platform:    Dell Precision T8900, Win10                          //
// Application: Demonstration for CSE681, Project #3, Fall 2018      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   DepAnalysis:
 *   - uses Parser, Graph, SemiExp, and Toker packages to perform Dependency Analysis
 *
 * - Required files:   
 * 
 * DepAnalysis.cs
 * Parser.cs
 * Graph.cs
 * IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 * FileMgr.cs, TypeTable.cs
 * ITokenCollection.cs, Semi.cs, Toker.cs
 * Display.cs
 *
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 31 Oct 2018
 * - first release
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CodeAnalysis;
using Lexer;

namespace Dependency_Analysis
{
    using AdjL = List<Tuple<string, string>>;
    public class DepAnalysis
    {
        //Seaches all the directories in the current path and looks for files that 
        //match the given pattern, if no pattern is given it uses the default "*.cs" 
        //pattern and finds all the .cs files
        static List<string> ProcessCommandline(string[] args, string pattern = "*.cs")
        {
            List<string> files = new List<string>();
            if (args.Length < 1)
            {
                Console.Write("\n  Please enter path to analyze\n\n");
                return files;
            }
            string path = args[0];
            if (!Directory.Exists(path))
            {
                Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
                return files;
            }
            path = Path.GetFullPath(path);
            //            Console.WriteLine("\nPath: {0}\n",path);
            string[] filearr = Directory.GetFileSystemEntries(path, pattern, SearchOption.AllDirectories);
            files = filearr.OfType<string>().ToList();
            //Console.WriteLine("Files found: {0}",files.Count);
            //foreach (var dir in dirs)
            //{
            // for (int i = 1; i < args.Length; ++i)
            // {
            //     string filename = Path.GetFileName(args[i]);
            //     files.AddRange(Directory.GetFiles(dir,pattern));
            //  }
            //}
            return files;
        }
        //Shows the commandline arguments passed to the function along with  
        //the current directory and all the files found by ProcessCommandLine function
        public static void ShowCommandLine(string[] args)
        {
            Console.WriteLine("\n  Commandline args are: \n");
            foreach (string arg in args)
            {
                Console.Write("  {0}", arg);
            }
            List<string> files = ProcessCommandline(args);
            string path = args[0];
            path = Path.GetFullPath(path);
            Console.WriteLine("\nPath: {0}\n", path);

            foreach (string file in files)   //loops through all the available files 
            {
                Console.WriteLine("\nFile Found: {0} ", file);
            }
            //Console.Write("\n  current directory: {0}", Directory.GetCurrentDirectory());
            Console.Write("\n");
        }

    /// <summary>
    /// From list of files
    /// </summary>

    static TypeTable makeTTF(List<string> files)
    {
      StringBuilder msg = new StringBuilder();
      TypeTable tt = new TypeTable();
      //Console.Write("\n  Demonstrating Parser");
      //Console.Write("\n ======================\n");

      //ShowCommandLine(args);

      //List<string> files = ProcessCommandline(args);
      foreach (string file in files)   //loops through all the available files 
      {
        //msg.Append(Environment.NewLine+"  Processing file "+ System.IO.Path.GetFileName(file));
        ITokenCollection semi = Factory.create();
        //semi.displayNewLines = false;
        if (!semi.open(file as string))
        {
          throw new Exception("\n  Can't open file!\n\n");
        }
        BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
        Parser parser = builder.build();

        try
        {
          while (semi.get().Count > 0) //This is where the semi expressions 
            parser.parse(semi);        //are getting parsed, above we had just defined the parser
                                       //rememeber to remove the comment from the foreach in side parse
        }
        catch (Exception ex)
        {
          Console.Write("\n\n  {0}\n", ex.Message);
        }
        Repository rep = Repository.getInstance();
        List<Elem> table = rep.locations;
        string[] fullname = file.Split('\\');
        string filename = fullname[fullname.Length - 1];
        foreach (var element in table)
        {
          //Console.WriteLine("Name: {0}  Type: {1}  Filename: {2}", element.name, element.type, filename);
          tt.add(element.type, filename, element.name);
        }
        //Display.showMetricsTable(table);
        semi.close();
      }
      //Console.WriteLine("\n\n     The TypeTable\n     ============\n");
      //tt.show();   //my TypeTable
      //Console.Write("\n\n");
      return tt;
      //Console.ReadKey();
    }

    //Finds all the different types and names used in different files



    static TypeTable makettF(List<string> files)
    {
      TypeTable tt = new TypeTable();
      //Console.WriteLine("\nFine till now");
      //List<string> files = ProcessCommandline(args);

      foreach (string file in files)   //loops through all the available files 
      {
        //Console.WriteLine("\n  Processing file {0}", file);

        ITokenCollection semi = Factory.create();
        //semi.displayNewLines = false;
        if (!semi.open(file as string))
        {
          throw new Exception("\n  Can't open file!\n\n");
        }
        NameCodeAnalyzer builder = new NameCodeAnalyzer(semi);
        Parser parser = builder.build();
        try
        {
          while (semi.get().Count > 0) //This is where the semi expressions 
            parser.parse(semi);        //are getting parsed, above we had just defined the parser
                                       //rememeber to remove the comment from the foreach in side parse
        }
        catch (Exception ex)
        {
          Console.Write("\n\n  {0}\n", ex.Message);
        }
        Repository rep = Repository.getInstance();
        List<Elem> table = rep.locations;

        string[] fullname = file.Split('\\');
        string filename = fullname[fullname.Length - 1];
        //Console.WriteLine("\n\n              Alias Table\n           =============\n");
        //Console.WriteLine("        Name                inFile");
        //Console.WriteLine("       ======               =======\n");
        foreach (var element in table)
        {

          //Console.WriteLine("Name: {0}  Type: {1}  Filename: {2}", element.name, element.type, filename);
          //Console.Write("   |  {0}  ", element.name.PadRight(15));
          //Console.Write("   |  {0}  ", filename.PadRight(10));
          //Console.Write(" | \n");

          //if (tt.constains(element.name))

          //TAB table contains all the different types and all names along with file names

          tt.add(element.type, filename, element.name);
        }
        //Display.showMetricsTable(table);

        //Console.Write("\n");
        //tt.show();
        semi.close();
      }
      //Console.Write("\n");
      //tt.show();
      return tt;
    }


    //Generates the Type Table for all the different types that are used
    //in the given files
    static TypeTable makeTT(string[] args)
        {
      StringBuilder msg = new StringBuilder();
            TypeTable tt = new TypeTable();
            //Console.Write("\n  Demonstrating Parser");
            //Console.Write("\n ======================\n");

            //ShowCommandLine(args);

            List<string> files = ProcessCommandline(args);
            foreach (string file in files)   //loops through all the available files 
            {
                //msg.Append(Environment.NewLine+"  Processing file "+ System.IO.Path.GetFileName(file));
                ITokenCollection semi = Factory.create();
                //semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    throw new Exception("\n  Can't open file!\n\n");
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.get().Count > 0) //This is where the semi expressions 
                        parser.parse(semi);        //are getting parsed, above we had just defined the parser
                                                   //rememeber to remove the comment from the foreach in side parse
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                string[] fullname = file.Split('\\');
                string filename = fullname[fullname.Length - 1];
                foreach (var element in table)
                {
                    //Console.WriteLine("Name: {0}  Type: {1}  Filename: {2}", element.name, element.type, filename);
                    tt.add(element.type, filename, element.name);
                }
                //Display.showMetricsTable(table);
                semi.close();
            }
            //Console.WriteLine("\n\n     The TypeTable\n     ============\n");
            //tt.show();   //my TypeTable
            //Console.Write("\n\n");
            return tt;
            //Console.ReadKey();
        }

        //Finds all the different types and names used in different files



        static TypeTable makett(string[] args)
        {
            TypeTable tt = new TypeTable();
            //Console.WriteLine("\nFine till now");
            List<string> files = ProcessCommandline(args);
            
            foreach (string file in files)   //loops through all the available files 
            {
                //Console.WriteLine("\n  Processing file {0}", file);

                ITokenCollection semi = Factory.create();
                //semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    throw new Exception("\n  Can't open file!\n\n");
                }
                NameCodeAnalyzer builder = new NameCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.get().Count > 0) //This is where the semi expressions 
                        parser.parse(semi);        //are getting parsed, above we had just defined the parser
                                                   //rememeber to remove the comment from the foreach in side parse
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;

                string[] fullname = file.Split('\\');
                string filename = fullname[fullname.Length - 1];
                //Console.WriteLine("\n\n              Alias Table\n           =============\n");
                //Console.WriteLine("        Name                inFile");
                //Console.WriteLine("       ======               =======\n");
                foreach (var element in table)
                {

                    //Console.WriteLine("Name: {0}  Type: {1}  Filename: {2}", element.name, element.type, filename);
                    //Console.Write("   |  {0}  ", element.name.PadRight(15));
                    //Console.Write("   |  {0}  ", filename.PadRight(10));
                    //Console.Write(" | \n");

                    //if (tt.constains(element.name))

                    //TAB table contains all the different types and all names along with file names

                    tt.add(element.type, filename, element.name);
                }
                //Display.showMetricsTable(table);

                //Console.Write("\n");
                //tt.show();
                semi.close();
            }
            //Console.Write("\n");
            //tt.show();
            return tt;
        }

        //Finds the dependency by comparing the data from makeTT and makett
        static AdjL depAn(string[] args)
        {
            //ShowCommandLine(args);
            //stores types and files they were used in
            //Depend depend = new Depend();
            AdjL e = new AdjL(); // Storing edge list as list of tuples string,string

            //makes type table
            TypeTable tab = new TypeTable();
            tab = makeTT(args);

            //makes types used table
            TypeTable tt = new TypeTable();
            tt = makett(args);
            
            //We are gonna process and find namespaces we use in each file
            //Console.Write("\n  Demonstrating NameSpace Finder");
            //Console.Write("\n ===============================\n");

            string[] toCheck = { "namespace", "class", "interface" };
            foreach (var type in toCheck)
            {
                if (tab.table.ContainsKey(type))

                    foreach (TypeItem val in tab.table[type]) // will have to do same for class
                    {
                        //Console.Write("\n    [{0}, {1}]", val.namesp, val.file);
                        foreach (var elem in tt.table)
                            foreach (var item in elem.Value)
                            {
                                //Console.Write("\n      {0}    [{1}, {2}]", elem.Key, item.namesp, item.file);
                                if (val.namesp.Equals(item.namesp) && !val.file.Equals(item.file)) //checking if the current using is defined in some file
                                {
       //    important line=>                         //Console.Write("\n [{0} - USED IN - {1} - DEFINED IN - {2}]", item.namesp, item.file, val.file);
                                    e.Add(Tuple.Create(item.file, val.file));
                                }
                            }
                    }
            }
            Console.Write("\n");
            //Console.ReadKey();
            return e;
        }

    static AdjL depAnF(List<string> files)
    {
      //ShowCommandLine(args);
      //stores types and files they were used in
      //Depend depend = new Depend();
      AdjL e = new AdjL(); // Storing edge list as list of tuples string,string

      //makes type table
      TypeTable tab = new TypeTable();
      tab = makeTTF(files);

      //makes types used table
      TypeTable tt = new TypeTable();
      tt = makettF(files);

      //We are gonna process and find namespaces we use in each file
      //Console.Write("\n  Demonstrating NameSpace Finder");
      //Console.Write("\n ===============================\n");

      string[] toCheck = { "namespace", "class", "interface" };
      foreach (var type in toCheck)
      {
        if (tab.table.ContainsKey(type))

          foreach (TypeItem val in tab.table[type]) // will have to do same for class
          {
            //Console.Write("\n    [{0}, {1}]", val.namesp, val.file);
            foreach (var elem in tt.table)
              foreach (var item in elem.Value)
              {
                //Console.Write("\n      {0}    [{1}, {2}]", elem.Key, item.namesp, item.file);
                if (val.namesp.Equals(item.namesp) && !val.file.Equals(item.file)) //checking if the current using is defined in some file
                {
                  //    important line=>                         //Console.Write("\n [{0} - USED IN - {1} - DEFINED IN - {2}]", item.namesp, item.file, val.file);
                  e.Add(Tuple.Create(item.file, val.file));
                }
              }
          }
      }
      Console.Write("\n");
      //Console.ReadKey();
      return e;
    }

    //Helper function used for demonstrating the strong components found from the dependency analysis
    //by implmenting Tarjan's Algorithm for finding strongly connected components
    public static StringBuilder demoStrongComp(string[] args)
        {
            AdjL e = new AdjL();
            e = depAn(args);
            return genStrongComp(args, e);
        }

    public static StringBuilder demoStrongCompF(List<string> files)
    {
      AdjL e = new AdjL();
      e = depAnF(files);
      return genStrongCompF(files, e);
    }
    //Used for demonstrating Type Table
    public static StringBuilder demoTypeTable(string[] args)
        {
      StringBuilder msg = new StringBuilder();
      TypeTable tab = new TypeTable();
            tab = makeTT(args);
            msg.Append(Environment.NewLine+"     The TypeTable"+Environment.NewLine + "     ============");
            msg.Append(tab.shows());
      return msg;
        }

    public static StringBuilder demoTypeTableF(List<string> files)
    {
      StringBuilder msg = new StringBuilder();
      TypeTable tab = new TypeTable();
      tab = makeTTF(files);
      msg.Append(Environment.NewLine + "     The TypeTable" + Environment.NewLine + "     ============");
      msg.Append(tab.shows());
      return msg;
    }

    //Used for generating the strong components found from the dependency analysis
    //by implmenting Tarjan's Algorithm for finding strongly connected components
    static StringBuilder genStrongComp(string[] args, AdjL e)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append(Environment.NewLine+"     Generating all the Components");
            msg.Append(Environment.NewLine + "  ==============================");
            var graph_nodes = new List<Vertex>();
            var v = new List<Vertex>(); ;
            List<string> files = new List<string>();
            List<string> filepaths = ProcessCommandline(args);
            foreach (string file in filepaths)
            {
                string[] fullname = file.Split('\\');
                files.Add(fullname[fullname.Length - 1]);
            }
            for (int i = 0; i < files.Count; i++)
            {
                Vertex ver = new Vertex() { Id = i, Name = files[i] };
                v.Add(ver);
            }

            foreach (var node in v) // my nodes
            {
                foreach (var n in e)  // the file name in tuple list 
                {
                    if (node.Name == n.Item1)   //if node name == name in tuple
                        foreach (var targenode in v) // found the target node
                            if (targenode.Name == n.Item2)
                            {
                                if (!node.Dependencies.Any(child => child.Name == targenode.Name))
                                    node.Dependencies.Add(targenode);
                            }
                }
            }

            foreach (var node in v) // my nodes
            {
                graph_nodes.Add(node);
            }

            var tcd = new TarjanCycleDetectStack();
            var cycle_list = tcd.DetectCycle(graph_nodes);

            msg.Append(Environment.NewLine + "Total number of components: "+ cycle_list.Count + Environment.NewLine);


            foreach (var vertices in cycle_list)
            {
                //Console.WriteLine(v);

                foreach (var ver in vertices)
                {
                      msg.Append(ver.ToString());
                    // Console.WriteLine(string.Join(" ", v.Dependencies));
                }
            msg.Append(Environment.NewLine);

            }
      //Console.ReadKey();
      return msg;
        }

    static StringBuilder genStrongCompF(List<string> filepaths, AdjL e)
    {
      StringBuilder msg = new StringBuilder();
      msg.Append(Environment.NewLine + "     Generating all the Components");
      msg.Append(Environment.NewLine + "  ==============================");
      var graph_nodes = new List<Vertex>();
      var v = new List<Vertex>(); ;
      List<string> files = new List<string>();
      //List<string> filepaths = ProcessCommandline(args);
      foreach (string file in filepaths)
      {
        string[] fullname = file.Split('\\');
        files.Add(fullname[fullname.Length - 1]);
      }
      for (int i = 0; i < files.Count; i++)
      {
        Vertex ver = new Vertex() { Id = i, Name = files[i] };
        v.Add(ver);
      }

      foreach (var node in v) // my nodes
      {
        foreach (var n in e)  // the file name in tuple list 
        {
          if (node.Name == n.Item1)   //if node name == name in tuple
            foreach (var targenode in v) // found the target node
              if (targenode.Name == n.Item2)
              {
                if (!node.Dependencies.Any(child => child.Name == targenode.Name))
                  node.Dependencies.Add(targenode);
              }
        }
      }

      foreach (var node in v) // my nodes
      {
        graph_nodes.Add(node);
      }

      var tcd = new TarjanCycleDetectStack();
      var cycle_list = tcd.DetectCycle(graph_nodes);

      msg.Append(Environment.NewLine + "Total number of components: " + cycle_list.Count + Environment.NewLine);


      foreach (var vertices in cycle_list)
      {
        //Console.WriteLine(v);

        foreach (var ver in vertices)
        {
          msg.Append(ver.ToString());
          // Console.WriteLine(string.Join(" ", v.Dependencies));
        }
        msg.Append(Environment.NewLine);

      }
      //Console.ReadKey();
      return msg;
    }

#if (Test_Dep)
    static void Main(string[] args)
        {
            ShowCommandLine(args);
            
            //makes type table
            TypeTable tab = new TypeTable();
            tab = makeTT(args);
            tab.show();

            //makes types used table
            TypeTable tt = new TypeTable();
            tt = makett(args);
            tt.show();
            
            //Generating our adjacencry list

            AdjL e = new AdjL();
            e = depAn(args);
            genStrongComp(args, e);
            
            Console.ReadKey();
        }
#endif
    }
}
