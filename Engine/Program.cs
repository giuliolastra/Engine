using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].Equals("upload"))
                {
                    if (Program.uploadTree(args[1]))
                        Console.WriteLine("Your tree is on the Database");

                }
                else if (args[0].Equals("calculus"))
                {
                    Console.WriteLine(Program.performCalculus(args[1], args[2], args[3]));
                }
                else
                {
                    Console.WriteLine("Wrong Parameters");
                }
            } else
            {
                Console.WriteLine("Engine expects at leasts 2 parameters. To configurate Database please use PlannerPath.exe");
            }
        }

        public static bool uploadTree(string treeFile)
        {
            Uploader uploader = new Uploader();
            uploader.SaveToDB(treeFile);
            return true;
        }

        public static String performCalculus(string type, string vertexA, string vertexB)
        {
            Calculus calculus = new Calculus();
            return calculus.PathCalculus(type, vertexA, vertexB);
        }
    }
}
