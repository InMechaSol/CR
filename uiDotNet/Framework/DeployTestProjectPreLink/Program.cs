using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using ccLib_netCore;

namespace DeployTestProjectPreLink
{
    class Program
    {
        static void Main(string[] args)
        {
            // Program expects ccNOosTest Directory as an input
            if (args.Length == 3)
            {

                string ccNOosTestAppDir = Path.GetFullPath(args[0]);
                if (!Directory.Exists(ccNOosTestAppDir))
                {
                    Console.WriteLine("FAILURE:( " + ccNOosTestAppDir + " does not exist!");
                    return;
                }


                string ccNOosTestsDIR = Path.GetFullPath(ccNOosTestAppDir + "\\..\\..\\..\\..\\");
                if (Directory.Exists(ccNOosTestsDIR))
                {
                    string ccNOosDIR = ccNOosTestsDIR + "ccNOos";
                    if (!Directory.Exists(ccNOosDIR))
                    {
                        Console.WriteLine("FAILURE:( " + ccNOosDIR + " does not exist!");
                        return;
                    }


                    //string ArduinoDIR = ccNOosTestsDIR + "Arduino\\" + ccNOosTestAppDir.Substring(ccNOosTestAppDir.LastIndexOf("\\")+1);
                    string ArduinoDIR = Path.GetFullPath(args[1]);
                    if (!Directory.Exists(ArduinoDIR))
                    {
                        Directory.CreateDirectory(ArduinoDIR);
                    }
                    else
                    {
                        // loop through all like named directories, 
                        // loop through all files, 
                        // delete from "arduino", 
                        // copy and modify from "ccNOos" 
                        RepoManager.DeleteFilesAndFoldersRecursively(ArduinoDIR);
                        Directory.CreateDirectory(ArduinoDIR);
                    }



                    foreach (string dstring in Directory.GetDirectories(ccNOosDIR))
                    {
                        RepoManager.CopyModifyDirectoryRecursively(dstring, ArduinoDIR);

                    }
                    Console.WriteLine("Arduino Source Generated!");
                }
                else
                {
                    Console.WriteLine("FAILURE:( " + ccNOosTestsDIR + " does not exist!");
                    return;
                }





                ////////////////////////////////////////////////////////
                //// this is for the FlatFiles project
                string objDIR = Path.GetFullPath(args[2]);
                bool notOnce = true;
                if (Directory.Exists(objDIR))
                {
                    foreach (string filenamestring in Directory.GetFiles(objDIR))
                    {
                        if (filenamestring.EndsWith(".i"))
                        {
                            Console.Write($"Processing {filenamestring} ...");
                            notOnce = false;
                            char[] seps = { '\n', '\r' };
                            string[] lines = File.ReadAllText(filenamestring).Split(seps, StringSplitOptions.RemoveEmptyEntries);
                            string outText = "";
                            foreach (string lString in lines)
                            {
                                if (!String.IsNullOrWhiteSpace(lString))
                                    outText += lString + "\n";
                            }
                            Console.Write("Saving ...");
                            File.WriteAllText(filenamestring.Replace(".i", ".cpp"), outText);
                            Console.Write("Done!\n");
                        }
                    }

                    if (notOnce)
                        Console.WriteLine("No .i Files to Process this Time.");
                    else
                        Console.WriteLine("Pre-Processed Files Cleaned and Saved for Flat Console Test!");
                    return;

                }
                else
                {
                    Console.WriteLine("FAILURE:( " + objDIR + " does not exist!");
                    return;
                }
            }
            else
            {
                Console.WriteLine("FAILURE:( Correct Directories were not entered as program inputs!");
                return;
            }
        }
    }
}
