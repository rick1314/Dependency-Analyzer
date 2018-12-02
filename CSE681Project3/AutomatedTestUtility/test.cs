////////////////////////////////////////////////////////////////////////////////////
//  AutomatedTestUtility.cs - Demonstrates Requirements for Project #3 of CSE681  //
//  ver 1.0                                                                       //
//  Language:     C#, VS 2017                                                     //
//  Platform:     Asus GL552VW, Windows 2010 Home                                 //
//  Application:  Demonstrates Requirements for Project #3 of CSE681              //
//  Author:       Debopriyo Bhattacharya, Syracuse University                     //
//                (315) 278 7480, debhatta@syr.edu                                //
////////////////////////////////////////////////////////////////////////////////////

/*
 *   Automated Testing Utility
 *   -----------------
 *   This package demonstrates all the necessary requirements for Project #3 
 *   of CSE 681. It works by using Dependency Analysis, Display, Element, FileMgr, Parser, SemiExp and Toker packages.
 *  
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   
 *   test.cs
 *   DepAnalysis.cs
 *   Parser.cs
 *   Graph.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   FileMgr.cs, TypeTable.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs
 *                          
 * Compiler Command:
 *   devenv Project3 /rebuild debug
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 31 October 2018
 *     - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dependency_Analysis;

namespace AutomatedTestUtility
{
    public class AutoTestUtil
    {

        public void req1to3()
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("\n1.Shall use Visual Studio 2017 and its C# Windows Console Projects, as provided in the ECS computer labs.");
            msg.Append("\n2.Shall use the .Net System.IO and System.Text for all I/O.");
            msg.Append("\n3.Shall provide C# packages as described in the Purpose section.");
            msg.Append("\n  Have fulfilled Requirements 1 & 2. \n  Requirement 3 is fulfilled by Packages- \n   Toker\n   SemiExp\n   Parser\n   FileMgr\n   Element\n   Display\n   TypeTable\n   Dependency Analysis\n   AutomatedTestUtility");
            Console.WriteLine(msg);
        }
        public void req4(string[] args)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("\n4. These packages shall evaluate all the dependencies between files in a specified file set. Please support specifying the collection as all C# files in a sub-directory tree, rooted at a specified path. You may elect to also provide an alternate means to specify the collection as a list of filenames, but you are not required to do so.");
            msg.Append("\n  Have fulfilled Requirements 4 as the following:");
            Console.WriteLine(msg);
            DepAnalysis.ShowCommandLine(args);
        }

        public StringBuilder req5(string[] args)
        {
          StringBuilder msg = new StringBuilder();
            //msg.Append(Environment.NewLine + "5. Your dependency analysis shall be based on identification of all the user-defined types in the specified set of files. That means you will need to identify all of the Types defined within that code, e.g., interfaces, classes, structs, enums, and delegates. You will also need to consider aliases, since an alias may refer to a type defined in another file. One last obligation - you need to account for namespaces.");
            //msg.Append(Environment.NewLine + "  Have fulfilled Requirements 5 as the following:");
            msg.Append(DepAnalysis.demoTypeTable(args));
          return msg;
        }

        public StringBuilder req6(string[] args)
        {
      
          //Console.WriteLine("\n6. Shall find all strong components, if any, in the file collection, based on the dependency analysis, cited above.");
           return DepAnalysis.demoStrongComp(args);
        }

    public StringBuilder req5F(List<string> files)
    {
      StringBuilder msg = new StringBuilder();
      //msg.Append(Environment.NewLine + "5. Your dependency analysis shall be based on identification of all the user-defined types in the specified set of files. That means you will need to identify all of the Types defined within that code, e.g., interfaces, classes, structs, enums, and delegates. You will also need to consider aliases, since an alias may refer to a type defined in another file. One last obligation - you need to account for namespaces.");
      //msg.Append(Environment.NewLine + "  Have fulfilled Requirements 5 as the following:");
      msg.Append(DepAnalysis.demoTypeTableF(files));
      return msg;
    }

    public StringBuilder req6F(List<string> files)
    {

      //Console.WriteLine("\n6. Shall find all strong components, if any, in the file collection, based on the dependency analysis, cited above.");
      return DepAnalysis.demoStrongCompF(files);
    }

    public void req7()
        {

            Console.WriteLine("\n7. Shall display the results in a well formated area of the output.");
            Console.WriteLine("Requirement fulfilled by the output so far.");
        }
        public void req8()
        {

            Console.WriteLine("\n8. Shall include an automated unit test suite that demonstrates the requirements you've implemented and exercises all of the special cases that seem appropriate for these two packages.");
            Console.WriteLine("Requirement fulfilled. Thank you for using the Automated Test Suit.");
        }


    }
    class Test
    {
        static void Main(string[] args)
        {

            StringBuilder msg = new StringBuilder();
            msg.Append("\n/////////Solution for CSE681 Project #3 as per Requirements/////////");
            msg.Append("\n");
            Console.Write(msg);
            AutoTestUtil a = new AutoTestUtil();

      //a.req1to3();
      //a.req4(args);
      //a.req5(args);
      //a.req6(args);
      //a.req7();
      //a.req8();

      /*
       * Declare folder and write to file
       */
      string an = "result.txt";
      string sc = "strongCom.txt";
      string path = "../../../result/";
      path = System.IO.Path.GetFullPath(path);
      System.IO.Directory.CreateDirectory(path);

      StringBuilder result = new StringBuilder();
      result.Append(Environment.NewLine+ a.req5(args));

      StringBuilder strongcom = new StringBuilder();
      strongcom.Append(Environment.NewLine + a.req6(args));

      System.IO.File.WriteAllText(path + an, result.ToString());
      System.IO.File.WriteAllText(path + sc, strongcom.ToString());
      Console.WriteLine(path + an);
      Console.WriteLine(path + sc);

      Console.Write("\n\n");
            Console.ReadKey();
        }
    }
}
