/////////////////////////////////////////////////////////////////////
// TypeTableDemo.cs - Project #3 Helper                            //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeTable
{
    using File = String;
    using Type = String;
    using Namespace = String;

    public struct TypeItem
    {
        public File file;
        public Namespace namesp;
    }

    public class TypeTable
    {
        public Dictionary<File, List<TypeItem>> table { get; set; } =
          new Dictionary<File, List<TypeItem>>();

        public void add(Type type, TypeItem ti)
        {
            if (table.ContainsKey(type))
                table[type].Add(ti);
            else
            {
                List<TypeItem> temp = new List<TypeItem>();
                temp.Add(ti);
                table.Add(type, temp);
            }
        }
        public void add(Type type, File file, Namespace ns)
        {
            TypeItem temp;
            temp.file = file;
            temp.namesp = ns;
            add(type, temp);
        }
        public void show()
        {
            foreach (var elem in table)
            {
                Console.Write("\n  {0}", elem.Key);
                foreach (var item in elem.Value)
                {
                    Console.Write("\n    [{0}, {1}]", item.file, item.namesp);
                }
            }
            Console.Write("\n");
        }
    }
    class TestTypeTableDemo
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrate how to build typetable");
            Console.Write("\n ====================================");
            // build demo table

            TypeTable tt = new TypeTable();
            tt.add("Type_X", "File_A", "Namespace_Test1");
            tt.add("Type_X", "File_B", "Namespace_Test2");
            tt.add("Type_Y", "File_A", "Namespace_Test1");
            tt.add("Type_Z", "File_C", "Namespace_Test3");

            tt.show();

            // access elements in table

            Console.Write("\n  TypeTable contains types: ");
            foreach (var elem in tt.table)
            {
                Console.Write("{0} ", elem.Key);
            }
            Console.Write("\n\n");
        }
    }
}
