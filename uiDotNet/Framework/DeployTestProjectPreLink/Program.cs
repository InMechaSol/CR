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
        static void deployDirectoryRecursive(string srcDir, string dstDir)
        {
            if (Directory.Exists(dstDir))
                RepoManager.DeleteFilesAndFoldersRecursively(dstDir);
            RepoManager.copyFilesNFoldersRecurrsive(srcDir, dstDir);
        }
        static void deployDirectory(string srcDir, string dstDir)
        {
            if (Directory.Exists(dstDir))
                RepoManager.DeleteFilesAndFoldersRecursively(dstDir);
            RepoManager.copyFilesNFolders(srcDir, dstDir);
        }
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
            else if(args.Length == 1)
            {
                if(args[0] == "ccACU")
                {
                    //Console.WriteLine("ccACU: Deploying to CR/ccACU Repository");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccOS_Tests\\ccOS\\ccNOos\\tests\\testApps\\SatComACS", "C:\\IMS\\CR\\ccACU_Tests\\ccACU\\SatComACS");
                    //RepoManager.copyFilesNFolders("C:\\IMS\\CR\\ccOS_Tests\\ccOS\\tests\\testApps\\ccACU", "C:\\IMS\\CR\\ccACU_Tests\\ccACU");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccOS_Tests\\ccOS\\tests\\testApps\\ccACU\\apiModules", "C:\\IMS\\CR\\ccACU_Tests\\ccACU\\apiModules");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccOS_Tests\\ccOS\\tests\\testApps\\ccACU\\deviceModules", "C:\\IMS\\CR\\ccACU_Tests\\ccACU\\deviceModules");
                    //File.Copy("C:\\IMS\\CR\\ccOS_Tests\\ccOS\\ccNOos\\tests\\testPlatforms\\Platform_ccOS.hpp", "C:\\IMS\\CR\\ccACU_Tests\\ccACU\\Tests\\Platform_ccOS.hpp", true);
                    return;
                }
                else if (args[0] == "TS4900ACU")
                {
                    //Console.WriteLine("TS4900ACU: Deploying to CS/TS4900ACU Repository");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccACU_Tests\\TS4900\\CompInterfaces", "C:\\IMS\\CS\\TS4900ACU\\application\\CompInterfaces");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccACU_Tests\\TS4900\\ConsoleApp", "C:\\IMS\\CS\\TS4900ACU\\application\\ConsoleApp");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccACU_Tests\\TS4900\\ControllerApp", "C:\\IMS\\CS\\TS4900ACU\\application\\ControllerApp");                    
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccACU_Tests\\TS4900\\ExternalApps", "C:\\IMS\\CS\\TS4900ACU\\application\\ExternalApps");
                    //deployDirectoryRecursive("C:\\IMS\\CR\\ccACU_Tests\\TS4900\\TestApps", "C:\\IMS\\CS\\TS4900ACU\\application\\TestApps");
                    return;
                }
                else if(args[0] == "SatComACS")
                {
                    Console.WriteLine("SatComACS: Deploying to CR/SatComACS Repository");
                    
                    return;
                }
                else
                {
                    Console.WriteLine("FAILURE:( Unrecongnized deploy project name input");
                    return;
                }
            }
            else
            {
                Console.WriteLine("FAILURE:( exactly 3 nor 1 inputs were received");
                return;
            }
        }
    }
}
