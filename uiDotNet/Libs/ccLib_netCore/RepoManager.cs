using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ccLib_netCore
{
    public class RepoNodeStruct
    {
        [Category("Repository Node Structure")]
        [Description("The 'Name' keystring identifying the Repository")]
        [DisplayName("Name")]
        public string name { get; set; }
        [Category("Repository Node Structure")]
        [Description("The 'URL' keystring locating the Repository remote origin")]
        [DisplayName("URL")]
        public string url { get; set; }
        [Category("Repository Node Structure")]
        [Description("Submodule keystrings of the Repository")]
        [DisplayName("SubModuleNames")]
        public string[] submodnames { get; set; }
    }
    public class IMSConfigStruct
    {
        [Category("IMS Configuration Structure")]
        [Description("Path to git.exe, binary file")]
        [DisplayName("Path2GitBin")]
        public string Path2GitBin { get; set; }
        [Category("IMS Configuration Structure")]
        [Description("Path to doxygen.exe, binary file")]
        [DisplayName("Path2DoxygenBin")]
        public string Path2DoxygenBin { get; set; }
        [Category("IMS Configuration Structure")]
        [Description("Path to dot.exe, binary file")]
        [DisplayName("Path2GraphVizDotBin")]
        public string Path2GraphVizDotBin { get; set; }
        [Category("IMS Configuration Structure")]
        [Description("Path to Root Repository Directory")]
        [DisplayName("Path2RootRepository")]
        public string Path2RootRepository { get; set; }
        [Category("IMS Configuration Structure")]
        [Description("Flat Collection of all unique Universe Repositories")]
        [DisplayName("Repositories")]
        public List<RepoNodeStruct> Repositories { get; set; }
    }
    public class guiTreeNode
    {
        [Category("gui Tree Node")]
        [Description("References to Parent Node Objects, [0]-rootnode [1]-immediate parent")]
        [DisplayName("ParentNodes")]
        public List<guiTreeNode> ParentNodes { set; get; }
        [Category("gui Tree Node")]
        [Description("'Name' keystring, unique identifier of node")]
        [DisplayName("Name")]
        public string Name { set; get; }
        [Category("gui Tree Node")]
        [Description("Text for Display in Tree View")]
        [DisplayName("Text")]
        public string Text { set; get; }
        [Category("gui Tree Node")]
        [Description("Text for Display in Tree View Tool Tip")]
        [DisplayName("ToolTipText")]
        public string ToolTipText { set; get; }
        [Category("gui Tree Node")]
        [Description("Tag for linking gui node with real data elements")]
        [DisplayName("Tag")]
        public object Tag { set; get; }
        [Category("gui Tree Node")]
        [Description("References to Child Nodes")]
        [DisplayName("Nodes")]
        public List<guiTreeNode> Nodes { set; get; }
    }
    public class repoTreeNode:guiTreeNode
    {
        [Category("repo Tree Node")]
        [Description("Configuration element from file")]
        [DisplayName("RepoConfig")]
        public RepoNodeStruct RepoConfig { set; get; }
        [Category("repo Tree Node")]
        [Description("Active Branch of the git Repository")]
        [DisplayName("ActiveBranch")]
        public string ActiveBranch { set; get; }
        [Category("repo Tree Node")]
        [Description("Indication of a 'dirty' working directory")]
        [DisplayName("hasChanges")]
        public bool hasChanges { set; get; }
        [Category("repo Tree Node")]
        [Description("Indication of a working connection to Repository remote origin")]
        [DisplayName("isReachable")]
        public bool isReachable { set; get; }
        [Category("repo Tree Node")]
        [Description("Path to local working directory")]
        [DisplayName("workingDir")]
        public string workingDir { set; get; }
        [Category("repo Tree Node")]
        [Description("Sub-Level Depth (down from root repository node)")]
        [DisplayName("Depth")]
        public int Depth { get; set; }
        public repoTreeNode() {; }
        public repoTreeNode(repoTreeNode parentNode, RepoNodeStruct configStruct)
        {
            Nodes = new List<guiTreeNode>();
            ParentNodes = new List<guiTreeNode>(2);
            RepoConfig = configStruct;
            if (parentNode.ParentNodes != null) 
            { 
                if (parentNode.ParentNodes.Count > 0) 
                { 
                    ParentNodes.Add(parentNode.ParentNodes[0]); 
                } 
            }
            if (ParentNodes.Count == 0)
                ParentNodes.Add(parentNode);
            ParentNodes.Add(parentNode);

            Name = RepoConfig.name;
            Text = RepoConfig.name;
            Tag = this;
            ToolTipText = RepoConfig.url;
        }        
    }
    public class RepoManager : ComputeModule
    {

        public IMSConfigStruct IMSConfiguration { set; get; }
        public guiTreeNode IMSConfigNode { set; get; }
        public repoTreeNode RepositoryTreeRootNode { set; get; }
        public bool newConfigLoaded { get; set; } = false;
        public bool updateConfigflag = true;
        public bool buildReposFromRemotes { get; set; } = false;
        public bool pushTempRepos2Remotes { get; set; } = false;
        string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RepoManager";
        string ConfigReposDir;
        string ReposDir;
        string ReposDirUverseRoot;
        List<List<repoTreeNode>> RepositoryTreeLevelLists = new List<List<repoTreeNode>>();
        public RepoManager()
        {
            IMSConfiguration = new IMSConfigStruct();
            IMSConfiguration.Path2RootRepository = Path.GetFullPath("C:\\IMS");
            ConfigReposDir = tempDir + "\\ConfigurationRepos";
            ReposDir = tempDir + "\\UniverseRepos";
        }
        protected override void Loop()
        {
            // if a new file has been loaded, parse and build nodes
            if(newConfigLoaded)
            {
                BuildNodes();
                newConfigLoaded = false;
                updateConfigflag = true;
            }

            if(buildReposFromRemotes)
            {
                buildReposFromRemotes = false;
                BuildRepos();
            }

            if(pushTempRepos2Remotes)
            {
                pushTempRepos2Remotes = false;
                PushTemp2Remotes();
            }
        }
        protected override void Setup()
        {
            NoAlarmsNoWarnings = true;
        }
        public override void SysTick()
        {
            ;
        }
        public static void DeleteFilesAndFoldersRecursively(string target_dir)
        {
            foreach (string file in Directory.GetFiles(target_dir))
            {
                File.SetAttributes(file,FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string subDir in Directory.GetDirectories(target_dir))
            {
                DeleteFilesAndFoldersRecursively(subDir);
            }

            System.Threading.Thread.Sleep(1); // This makes the difference between whether it works or not. Sleep(0) is not enough.
            File.SetAttributes(target_dir, FileAttributes.Directory);
            Directory.Delete(target_dir);
        }
        void copyFilesNFoldersRecurrsive(string sourceDir, string targetDir)
        {
            if(!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            foreach(string fstring in Directory.GetFiles(sourceDir))
            {
                File.SetAttributes(fstring, FileAttributes.Normal);
                File.Copy(fstring, targetDir+$"\\{Path.GetFileName(fstring)}");
            }
            foreach (string dstring in Directory.GetDirectories(sourceDir))
                copyFilesNFoldersRecurrsive(dstring, targetDir+$"\\{dstring.Replace(sourceDir,"")}");
        }
        void UniversefromConfig(string ParentDirPath, repoTreeNode Node2Transfer)
        {
            string configDirPath = ConfigReposDir+$"\\{Node2Transfer.Name}";
            
            foreach(string f in Directory.GetFiles(configDirPath))
            {
                File.SetAttributes(f, FileAttributes.Normal);
                string frepo = ParentDirPath + $"\\{Node2Transfer.Name}\\{Path.GetFileName(f)}";                
                if (!Directory.Exists(Path.GetDirectoryName(frepo)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(frepo));
                }
                File.SetAttributes(Path.GetDirectoryName(frepo), FileAttributes.Directory);                  
                File.Copy(f, frepo);                
            }
            foreach (string dstring in Directory.GetDirectories(configDirPath))
            {
                string destName = ParentDirPath + $"\\{Node2Transfer.Name}\\{Path.GetFileName(dstring)}";
                if (dstring.EndsWith(".git"))
                {
                    exeSysLink.uComms.EnqueMsgString($"Universe Building: {destName}");
                    string strippedString = destName.Replace(ReposDir,"").Replace("\\.git","");
                    if (strippedString.LastIndexOf("\\")==0)
                    {
                        ReposDirUverseRoot = strippedString;//this is the root universe repository
                        copyFilesNFoldersRecurrsive(dstring, destName);
                    }
                    else
                    {
                        string submodulegitpath = destName.Replace(ReposDirUverseRoot, $"{ReposDirUverseRoot}\\.git\\modules");
                        copyFilesNFoldersRecurrsive(dstring, submodulegitpath);
                        string gitText = "gitdir: ";
                        gitText += Path.GetRelativePath(Path.GetDirectoryName(destName), submodulegitpath);
                        File.WriteAllText(destName,gitText);                        
                    }
                    if(Node2Transfer.Nodes.Count>0)
                    {
                        string moduleFname = destName.Replace(".git", ".gitmodules");
                        if (File.Exists(moduleFname))
                            File.Delete(moduleFname);
                        string modText = "";
                        foreach (repoTreeNode rNode in Node2Transfer.Nodes)
                        {
                            modText += $"[submodule \"{rNode.RepoConfig.name}\"]\r\n";
                            modText += $"\tpath = {rNode.RepoConfig.name}\r\n";
                            modText += $"\turl = {rNode.RepoConfig.url}\r\n";
                        }
                        File.WriteAllText(moduleFname, modText);
                    }
                    
                }
                else
                {
                    copyFilesNFoldersRecurrsive(dstring, destName);
                }
            }
                
        }
        void RecursiveUniversefromConfig(string ParentDirPath, repoTreeNode Node2Transfer)
        {
            if (!Directory.Exists(ParentDirPath))
            {
                Directory.CreateDirectory(ParentDirPath);
            }
            File.SetAttributes(ParentDirPath, FileAttributes.Directory);            
            UniversefromConfig(ParentDirPath, Node2Transfer);            
            foreach (repoTreeNode rNode in Node2Transfer.Nodes)
            {
                RecursiveUniversefromConfig(ParentDirPath+$"\\{Node2Transfer.Name}", rNode);
            }

        }
        public void BuildRepos()
        {
            List<ExtProcCmdStruct> Cmds = new List<ExtProcCmdStruct>();
            ExtProcCmdStruct thisCmd;
            if (IMSConfigNode!=null)
            {
                if(IMSConfigNode.Nodes.Count>0)
                {

                    if (Directory.Exists(ConfigReposDir))
                    {
                        exeSysLink.uComms.EnqueMsgString($"Deleting: Begins {ConfigReposDir}");
                        DeleteFilesAndFoldersRecursively(ConfigReposDir);
                        exeSysLink.uComms.EnqueMsgString($"Deleting: Completed {ConfigReposDir}");
                    }
                    Directory.CreateDirectory(ConfigReposDir);

                    // Clone all remotes from config file to flat config directory
                    Cmds.Clear();
                    foreach (guiTreeNode tNode in IMSConfigNode.Nodes.Find(n => n.Name == "Repositories").Nodes)
                    {
                        // build commands to clone into config repos dir
                        thisCmd = new ExtProcCmdStruct();
                        thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                        thisCmd.cmdArguments = $"clone {((RepoNodeStruct)tNode.Tag).url} {Path.GetFullPath(ConfigReposDir + "\\" + ((RepoNodeStruct)tNode.Tag).name)}";
                        thisCmd.workingDirString = ConfigReposDir;
                        Cmds.Add(thisCmd);
                    }

                    //Execute Clone Commands
                    exeSysLink.ThirdPartyTools.executeCMDS(Cmds);


                    // fully remove, unlink, and delete subs from config
                    // then create submodules according to config file
                    string[] rDirs = Directory.GetDirectories(ConfigReposDir);
                    List<bool> rDone = new List<bool>(rDirs.Length);
                    List<RepoNodeStruct> lastDoneList = new List<RepoNodeStruct>(rDirs.Length);
                    List<RepoNodeStruct> nextToDoList = new List<RepoNodeStruct>(rDirs.Length);
                    RepoNodeStruct thisRnode;
                    foreach (string rName in rDirs)
                    {
                        // get the list of subs from the config
                        thisRnode = IMSConfiguration.Repositories.Find(x => x.name == rName.Substring(rName.LastIndexOf("\\")+1));
                        if(thisRnode.submodnames!=null)
                            foreach(string sName in thisRnode.submodnames)
                            {
                                Cmds.Clear();

                                // build commands to rm 
                                thisCmd = new ExtProcCmdStruct();
                                thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                                thisCmd.cmdArguments = $"rm {sName}";
                                thisCmd.workingDirString = ConfigReposDir + $"\\{thisRnode.name}";
                                Cmds.Add(thisCmd);

                                // build commands to commit 
                                thisCmd = new ExtProcCmdStruct();
                                thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                                thisCmd.cmdArguments = $"commit -am \"*RepoManager: Removing subs from config\" ";
                                thisCmd.workingDirString = ConfigReposDir + $"\\{thisRnode.name}";
                                Cmds.Add(thisCmd);

                                //Execute Commands
                                exeSysLink.ThirdPartyTools.executeCMDS(Cmds);
                                Cmds.Clear();

                                // remove entry from config file
                                string confText = File.ReadAllText(ConfigReposDir + $"\\{thisRnode.name}\\.git\\config");
                                if (confText.Contains($"[submodule {sName}]") || confText.Contains($"[submodule \"{sName}\"]"))
                                    ;

                                // build commands to rmdir 
                                if(Directory.Exists(ConfigReposDir + $"\\{thisRnode.name}\\.git\\modules\\{sName}"))
                                    DeleteFilesAndFoldersRecursively(ConfigReposDir + $"\\{thisRnode.name}\\.git\\modules\\{sName}");


                            }

                        if (thisRnode.submodnames != null)
                            foreach (string sName in thisRnode.submodnames)
                            {
                                Cmds.Clear();

                                RepoNodeStruct thatRnode = IMSConfiguration.Repositories.Find(x => x.name == sName);
                                // build commands to submodule add 
                                thisCmd = new ExtProcCmdStruct();
                                thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                                thisCmd.cmdArguments = $"submodule add {thatRnode.url} {thatRnode.name}";
                                thisCmd.workingDirString = ConfigReposDir + $"\\{thisRnode.name}";
                                Cmds.Add(thisCmd);

                                // build commands to commit 
                                thisCmd = new ExtProcCmdStruct();
                                thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                                thisCmd.cmdArguments = $"commit -am \"*RepoManager: Adding subs from config\" ";
                                thisCmd.workingDirString = ConfigReposDir + $"\\{thisRnode.name}";
                                Cmds.Add(thisCmd);

                                //Execute Commands
                                exeSysLink.ThirdPartyTools.executeCMDS(Cmds);

                            }

                        if (thisRnode.submodnames == null)
                        { rDone.Add(true); lastDoneList.Add(thisRnode); nextToDoList.Add(thisRnode); }
                        else if (thisRnode.submodnames.Length == 0)
                        { rDone.Add(true); lastDoneList.Add(thisRnode); nextToDoList.Add(thisRnode); }
                        else
                        { rDone.Add(false); }
                    }
                                        
                    bool keepGoing = false;
                    do
                    {
                        // loop through the process list and mark as done
                        foreach (RepoNodeStruct r in nextToDoList)
                        {
                            Cmds.Clear();// process the repo
                            if (!(nextToDoList.FindIndex(x => x == r) == lastDoneList.FindIndex(x => x == r)))                            
                            {
                                // submodule update recursive remote

                                
                            }

                            ;// commit and push
                             // build commands to commit 
                            thisCmd = new ExtProcCmdStruct();
                            thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                            thisCmd.cmdArguments = $"commit -am \"*RepoManager: Building sub module tree\" ";
                            thisCmd.workingDirString = ConfigReposDir + $"\\{r.name}";
                            Cmds.Add(thisCmd);

                            //Execute Commands
                            exeSysLink.ThirdPartyTools.executeCMDS(Cmds);

                            int asdfas=0;
                            foreach(string st in rDirs)
                            {
                                if (st.Substring(st.LastIndexOf("\\")) == r.name)
                                {
                                    rDone[asdfas] = true;// mark as done
                                }
                                asdfas++;
                            }
                                
                                    

                        }

                        keepGoing = false;
                        foreach (bool b in rDone)
                            keepGoing |= !b;

                        if(keepGoing)
                        {
                            // transfer the next-to-process list over to the lastDoneList
                            lastDoneList.Clear();
                            foreach (RepoNodeStruct r in nextToDoList)
                                lastDoneList.Add(r);

                            // loop through to determine next to process list
                            nextToDoList.Clear();
                            foreach (RepoNodeStruct r in IMSConfiguration.Repositories)
                            {
                                List<string> subModNames = new List<string>(r.submodnames);
                                foreach (RepoNodeStruct q in lastDoneList)
                                    if (subModNames.Contains(q.name))
                                        if (!nextToDoList.Contains(q))
                                            nextToDoList.Add(q);
                            }
                        }
                        


                    } while (keepGoing);
                }
            }




            if(RepositoryTreeRootNode!=null)
            {
                if(RepositoryTreeRootNode.Nodes.Count>0)
                {
                    
                    if (Directory.Exists(ReposDir))
                    {
                        exeSysLink.uComms.EnqueMsgString($"Deleting: Begins {ReposDir}");
                        DeleteFilesAndFoldersRecursively(ReposDir);
                        exeSysLink.uComms.EnqueMsgString($"Deleting: Completed {ReposDir}");
                    }
                    Directory.CreateDirectory(ReposDir);



                    ///
                    exeSysLink.uComms.EnqueMsgString($"Universe Building: Begins {ReposDir}");
                    //RecursiveUniversefromConfig(ReposDir, (repoTreeNode)RepositoryTreeRootNode.Nodes[0]);
                    exeSysLink.uComms.EnqueMsgString($"Universe Building: Completed {ReposDir}");
                    ///


                    thisCmd = new ExtProcCmdStruct();
                    thisCmd.cmdString = "explorer";
                    thisCmd.cmdArguments = ReposDir;
                    thisCmd.workingDirString = ReposDir;
                    Cmds.Add(thisCmd);
                    exeSysLink.ThirdPartyTools.executeCMDS(Cmds);

                }
            }
        }
        public void PushTemp2Remotes()
        {
            for(int i = RepositoryTreeLevelLists.Count - 1; i>=0; i--)
            {
                List<repoTreeNode> ChangesList = new List<repoTreeNode>();
                List<repoTreeNode> NoChangeList = new List<repoTreeNode>();
                List<ExtProcCmdStruct> cmds = new List<ExtProcCmdStruct>();
                ExtProcCmdStruct thisCmd = new ExtProcCmdStruct();
                thisCmd.cmdString = IMSConfiguration.Path2GitBin;
                thisCmd.cmdArguments = "status --short";
                string tempV = IMSConfiguration.Path2RootRepository;
                foreach (repoTreeNode rNode in RepositoryTreeLevelLists[i])
                {
                    // check for local changes
                    cmds.Clear();// one at a time...
                    IMSConfiguration.Path2RootRepository = Directory.GetDirectories(ReposDir)[0];
                    thisCmd.workingDirString = expectedWorkingDirectoryPath(rNode);                    
                    cmds.Add(thisCmd);
                    exeSysLink.ThirdPartyTools.executeCMDS(cmds);
                    if (cmds[0].outANDerrorResults.Contains("up to date") && cmds[0].outANDerrorResults.Contains("Untracked files:  ("))
                    {
                        NoChangeList.Add(rNode);
                    }
                    else
                    {
                        ChangesList.Add(rNode);
                    }
                        
                }
                IMSConfiguration.Path2RootRepository = tempV;
                


                foreach (repoTreeNode rNode in ChangesList)
                {

                    // commit local
                    //thisCmd.cmdArguments = "commit"
                    //cmds.Clear();// one at a time...
                    //IMSConfiguration.Path2RootRepository = Directory.GetDirectories(ReposDir)[0];
                    //thisCmd.workingDirString = expectedWorkingDirectoryPath(rNode);
                    //cmds.Add(thisCmd);
                    //exeSysLink.ThirdPartyTools.executeCMDS(cmds);
                    //if (cmds[0].outANDerrorResults.Contains("up to date") && cmds[0].outANDerrorResults.Contains("Untracked files:  ("))
                    //{
                    //    NoChangeList.Add(rNode);
                    //}
                    //else
                    //{
                    //    ChangesList.Add(rNode);
                    //}

                    // pull from remote


                    // push remote

                    //
                    ;

                }

                foreach (repoTreeNode rNode in NoChangeList)
                {

                    // pull from remote no conflict
                    ;
                    // 
                }
            }
        }
        public string expectedWorkingDirectoryPath(repoTreeNode rNode)
        {
            string outstrig = "";

            List<string> parentNames = new List<string>();

            bool keepGoing = true;
            repoTreeNode thisNode = rNode;
            do
            {
                parentNames.Add(thisNode.Name);
                if (thisNode.ParentNodes != null)
                {
                    if (thisNode.ParentNodes[0] == thisNode.ParentNodes[1])
                    {
                        keepGoing = false;
                    }
                    else
                        thisNode = (repoTreeNode)thisNode.ParentNodes[1];
                }
                else
                    keepGoing = false;

            } while (keepGoing);

            outstrig += IMSConfiguration.Path2RootRepository;
            for (int i = parentNames.Count - 2; i >= 0; i--)
            {
                outstrig += "\\" + parentNames[i];
            }
            return outstrig;
        }
        private void BuildNodes()
        {            
            IMSConfigNode = createGUItreeNodefromConfig(IMSConfiguration);
            RepositoryTreeRootNode = createREPOtreeNodefromRepoList(IMSConfiguration.Repositories);
            ((repoTreeNode)RepositoryTreeRootNode.Nodes[0]).Depth = 0;
            RecursiveDetectWorkingDirectoryStatus((repoTreeNode)RepositoryTreeRootNode.Nodes[0]);            
        }

        public void DetectWorkingDirectoryStatus(repoTreeNode rootRepoNode)
        {
            rootRepoNode.isReachable = true;
            rootRepoNode.hasChanges = true;

            string sName = expectedWorkingDirectoryPath(rootRepoNode);

            if (Directory.Exists(sName))
            {
                rootRepoNode.workingDir = sName;
            }            
            else
                rootRepoNode.workingDir = "Not Detected";


            if (RepositoryTreeLevelLists.Count <= rootRepoNode.Depth)
            {
                RepositoryTreeLevelLists.Add(new List<repoTreeNode>());
            }
            if (!RepositoryTreeLevelLists[rootRepoNode.Depth].Contains(rootRepoNode))
                RepositoryTreeLevelLists[rootRepoNode.Depth].Add(rootRepoNode);


        }
        public void RecursiveDetectWorkingDirectoryStatus(repoTreeNode rootRepoNode)
        {
            // detect status for self
            DetectWorkingDirectoryStatus(rootRepoNode);

            // detect status of branches recursively
            foreach (repoTreeNode rNode in rootRepoNode.Nodes)
            {
                rNode.Depth = rootRepoNode.Depth + 1;                
                RecursiveDetectWorkingDirectoryStatus(rNode);
            }
        }
        public static guiTreeNode createGUItreeNodefromConfig(IMSConfigStruct refConfig)
        {
            guiTreeNode tempNode = new guiTreeNode();
            tempNode.Nodes = new List<guiTreeNode>();
            
            tempNode.Name = "RepoConfigList";
            tempNode.Text = "IMS Configuration List";
            tempNode.ToolTipText = refConfig.Path2RootRepository + "\\imsConf.json";
            tempNode.Tag = refConfig;

            guiTreeNode tempSubNode = new guiTreeNode();            
            tempSubNode.ParentNodes = new List<guiTreeNode>(2);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.Name = "RootRepoPath";
            tempSubNode.Text = "Path to Repository Root";
            tempSubNode.ToolTipText = refConfig.Path2RootRepository;
            tempSubNode.Tag = refConfig.Path2RootRepository;
            
            tempNode.Nodes.Add(tempSubNode);

            tempSubNode = new guiTreeNode();
            tempSubNode.ParentNodes = new List<guiTreeNode>(2);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.Name = "DoxygenPath";
            tempSubNode.Text = "Path to doxygen.exe";
            tempSubNode.ToolTipText = refConfig.Path2DoxygenBin;
            tempSubNode.Tag = refConfig.Path2DoxygenBin;
            tempNode.Nodes.Add(tempSubNode);

            tempSubNode = new guiTreeNode();
            tempSubNode.ParentNodes = new List<guiTreeNode>(2);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.Name = "GitPath";
            tempSubNode.Text = "Path to git.exe";
            tempSubNode.ToolTipText = refConfig.Path2GitBin;
            tempSubNode.Tag = refConfig.Path2GitBin;
            tempNode.Nodes.Add(tempSubNode);

            tempSubNode = new guiTreeNode();
            tempSubNode.ParentNodes = new List<guiTreeNode>(2);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.Name = "GraphvizDotPath";
            tempSubNode.Text = "Path to dot.exe";
            tempSubNode.ToolTipText = refConfig.Path2GraphVizDotBin;
            tempSubNode.Tag = refConfig.Path2GraphVizDotBin;
            tempNode.Nodes.Add(tempSubNode);

            tempSubNode = new guiTreeNode();
            tempSubNode.Nodes = new List<guiTreeNode>();
            tempSubNode.ParentNodes = new List<guiTreeNode>(2);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.ParentNodes.Add(tempNode);
            tempSubNode.Name = "Repositories";
            tempSubNode.Text = "Configuration Repositories";
            tempSubNode.ToolTipText = $"{refConfig.Repositories.Count} Repository List";
            tempSubNode.Tag = refConfig.Repositories;
            

            int counter = 0;            
            foreach (RepoNodeStruct rStruct in refConfig.Repositories)
            {
                guiTreeNode tempSubSubNode = new guiTreeNode();
                tempSubSubNode.ParentNodes = new List<guiTreeNode>(2);
                tempSubSubNode.ParentNodes.Add(tempNode);
                tempSubSubNode.ParentNodes.Add(tempSubNode);
                tempSubSubNode.Name = $"RepoNode{counter}";
                tempSubSubNode.Text = rStruct.name;
                tempSubSubNode.ToolTipText = "";
                if (refConfig.Repositories[counter].submodnames != null)
                {
                    if (refConfig.Repositories[counter].submodnames.Length > 0)
                    {
                        tempSubSubNode.ToolTipText += "Subs: ";
                        foreach (string s in refConfig.Repositories[counter].submodnames)
                            tempSubSubNode.ToolTipText += $"{s}, ";
                    }
                    else
                        tempSubSubNode.ToolTipText += "No Subs ";
                }
                else
                    tempSubSubNode.ToolTipText += "No Subs ";
                tempSubSubNode.Tag = refConfig.Repositories[counter];                
                tempSubNode.Nodes.Add(tempSubSubNode);
                counter++;
            }
            tempNode.Nodes.Add(tempSubNode);
            return tempNode;
        }

        public repoTreeNode createREPOtreeNodefromRepoList(List<RepoNodeStruct> refList)
        {
            
            repoTreeNode tempNode = new repoTreeNode();
            tempNode.Nodes = new List<guiTreeNode>();
            tempNode.Name = "RepoDirectoryTree";
            tempNode.Text = "Repository Directories";


            List<string> Names = new List<string>();
            List<string> SubNames = new List<string>();
            List<bool> NamesMatched = new List<bool>();
            foreach(RepoNodeStruct rStruct in refList)
            {
                Names.Add(rStruct.name);
                NamesMatched.Add(false);
                if(rStruct.submodnames!=null)
                    foreach (string s in rStruct.submodnames)
                        SubNames.Add(s);
            }
            int counter = 0;
            foreach(string s in Names)
            {                
                foreach(string S in SubNames)
                {
                    if(s == S)
                    {
                        NamesMatched[counter] = true;
                        break;
                    }    
                }
                if (!NamesMatched[counter])
                    break;
                counter++;
            }

            repoTreeNode tempSubNode = new repoTreeNode(tempNode, refList[counter]);

            ///////////////
            //// Recursive Function Call - Build all Repo Nodes
            //////////////
            BuildallRepoNodes(tempSubNode, refList);
            
            tempNode.Nodes.Add(tempSubNode);
            return tempNode;
        }
        public void BuildallRepoNodes(repoTreeNode tempSubNode, List<RepoNodeStruct> refList)
        {
            // Loop through all child nodes of current node
            repoTreeNode currentNode = tempSubNode;
            

            if (currentNode.RepoConfig.submodnames != null)
            {
                int counter = 0;
                foreach (string s in currentNode.RepoConfig.submodnames)
                {
                    RepoNodeStruct childConfig = refList.Find(r => r.name == s);
                    repoTreeNode nextChildNode = new repoTreeNode(currentNode, childConfig);
                    

                    if(childConfig.submodnames!=null)
                    {
                        if (childConfig.submodnames.Length > 0)
                        {
                            BuildallRepoNodes(nextChildNode, refList);
                            
                        }
                    }

                    currentNode.Nodes.Add(nextChildNode);
                    counter++;
                }
            }
        }
        public static IMSConfigStruct CreateDefaultIMSConfigStruct()
        {
            IMSConfigStruct outStruct = new IMSConfigStruct();
            outStruct.Path2RootRepository = "C:\\IMS";
            outStruct.Path2DoxygenBin = "C:\\Program Files\\doxygen\\bin\\doxygen.exe";
            outStruct.Path2GitBin = "C:\\Program Files\\Git\\bin\\git.exe";
            outStruct.Path2GraphVizDotBin = "C:\\Program Files\\Graphviz\\bin\\dot.exe";
            outStruct.Repositories = new List<RepoNodeStruct>();            

            RepoNodeStruct tempNode = new RepoNodeStruct();
            tempNode.name = "IMS";
            tempNode.url = "https://github.com/InMechaSol/IMS.git";
            tempNode.submodnames = new string[] { "CR", "CS", "P" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "CR";
            tempNode.url = "https://github.com/InMechaSol/CR.git";
            tempNode.submodnames = new string[] { "ccACU", "ccACU_Tests", "ccOS", "ccOS_Tests", "ccNOos", "ccNOos_Tests" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "CS";
            tempNode.url = "https://github.com/InMechaSol/CS.git";
            tempNode.submodnames = new string[] { "TS4900ACU" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "P";
            tempNode.url = "https://github.com/InMechaSol/P.git";
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccACU";
            tempNode.url = "https://github.com/InMechaSol/ccACU.git";
            tempNode.submodnames = new string[] { "ccOS" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccACU_Tests";
            tempNode.url = "https://github.com/InMechaSol/ccACU_Tests.git";
            tempNode.submodnames = new string[] { "ccACU" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccNOos";
            tempNode.url = "https://github.com/InMechaSol/ccNOos.git";
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccNOos_Tests";
            tempNode.url = "https://github.com/InMechaSol/ccNOos_Tests.git";
            tempNode.submodnames = new string[] { "ccNOos" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccOS";
            tempNode.url = "https://github.com/InMechaSol/ccOS.git";
            tempNode.submodnames = new string[] { "ccNOos" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "ccOS_Tests";
            tempNode.url = "https://github.com/InMechaSol/ccOS_Tests.git";
            tempNode.submodnames = new string[] { "ccOS" };
            outStruct.Repositories.Add(tempNode);

            tempNode = new RepoNodeStruct();
            tempNode.name = "TS4900ACU";
            tempNode.url = "https://github.com/InMechaSol/TS4900ACU.git";
            tempNode.submodnames = new string[] { "ccACU" };
            outStruct.Repositories.Add(tempNode);

            return outStruct;
        }
        public static RepoNodeStruct CreateRepoNodeStructJSON(ref byte[] jsonString)
        {            
            return Platform_Serialization.tryParseRepoNodeStruct(ref jsonString);
        }
        public static byte[] SerializeRepoNodeStructJSON(RepoNodeStruct outStruct)
        {            
            return Platform_Serialization.packageRepoNodeStruct(ref outStruct);
        }
        public static IMSConfigStruct CreateIMSConfigStructJSON(ref byte[] jsonString)
        {
            return Platform_Serialization.tryParseIMSConfigStruct(ref jsonString);
        }
        public static byte[] SerializeIMSConfigStructJSON(IMSConfigStruct outStruct)
        {
            return Platform_Serialization.packageIMSConfigStruct(ref outStruct);
        }







        //public static void DeleteFilesAndFoldersRecursively(string target_dir)
        //{
        //    foreach (string file in Directory.GetFiles(target_dir))
        //    {
        //        File.Delete(file);
        //    }

        //    foreach (string subDir in Directory.GetDirectories(target_dir))
        //    {
        //        DeleteFilesAndFoldersRecursively(subDir);
        //    }

        //    System.Threading.Thread.Sleep(1); // This makes the difference between whether it works or not. Sleep(0) is not enough.
        //    Directory.Delete(target_dir);
        //}

        // this is called for each directory in the ccNOos directory
        public static void CopyModifyDirectoryRecursively(string target_dir, string dirstring)
        {
            // process each file
            foreach (string fstring in Directory.GetFiles(target_dir))
            {
                // fstring - path to source file
                // filestring - path to destination file
                string filestring = dirstring + "\\" + fstring.Substring(fstring.LastIndexOf("\\") + 1);

                if (Path.GetExtension(fstring).Equals(".c"))
                {
                    Console.WriteLine("Processing: " + fstring);
                    // copy and change extension to hpp
                    File.Copy(fstring, filestring.Replace(".c", ".hpp"));
                }
                else if (Path.GetExtension(fstring).Equals(".cpp"))
                {
                    //if(fstring.Contains("Application_Solution"))
                    //{
                    //    // copy change name and extension
                    //    // copy and modify #include line within
                    //    string dcname = Path.GetDirectoryName(fstring);
                    //    dcname = dcname.Substring(dcname.LastIndexOf("\\") + 1);

                    //    string fctext = File.ReadAllText(fstring.Replace(".cpp", ".c"));
                    //    fctext = fctext.Replace("Application_Solution.h\"", "Application_Solution.hpp\"");                        

                    //    string ftext = File.ReadAllText(fstring);
                    //    ftext = ftext.Replace("#include \"Application_Solution.c\"", "");
                    //    string inostring = Path.GetFileName(filestring);
                    //    //inostring = odstring + "\\..\\Arduino\\" + dcname + "\\" + inostring;
                    //    inostring = Path.GetFullPath(inostring);
                    //    File.WriteAllText(filestring, fctext + ftext);
                    //}
                    //else if (fstring.Contains("PlatformApp_Serialization"))
                    //{
                    //    // copy change name and extension
                    //    // copy and modify #include line within
                    //    string dcname = Path.GetDirectoryName(fstring);
                    //    dcname = dcname.Substring(dcname.LastIndexOf("\\") + 1);

                    //    string fctext = File.ReadAllText(fstring.Replace(".cpp", ".c"));
                    //    fctext = fctext.Replace("PlatformApp_Serialization.h\"", "PlatformApp_Serialization.hpp\"");

                    //    string ftext = File.ReadAllText(fstring);
                    //    ftext = ftext.Replace("#include \"PlatformApp_Serialization.c\"", "");
                    //    string inostring = Path.GetFileName(filestring);
                    //    //inostring = odstring + "\\..\\Arduino\\" + dcname + "\\" + inostring;
                    //    inostring = Path.GetFullPath(inostring);
                    //    File.WriteAllText(inostring, fctext + ftext);
                    //}
                    //else
                    //{
                    Console.WriteLine("Processing: " + fstring);
                    // copy and modify #include line within
                    string ftext = File.ReadAllText(fstring);

                    Regex rx = new Regex(@"#include .*\.([c])");
                    MatchCollection matches = rx.Matches(ftext);
                    foreach (Match m in matches)
                    {
                        string[] toks = m.Value.Split('.');
                        ftext = rx.Replace(ftext, toks[0] + ".hpp");
                    }
                    File.WriteAllText(filestring, ftext);
                    //}

                }
                else if (Path.GetExtension(fstring).Equals(".h") || Path.GetExtension(fstring).Equals(".ino"))
                {
                    Console.WriteLine("Processing: " + fstring);
                    if (!fstring.Contains("Platform"))
                        File.Copy(fstring, filestring);
                    else if (fstring.Contains("Arduino"))
                        File.Copy(fstring, filestring);
                    else if (fstring.Contains("PlatformApp_Serialization"))
                        File.Copy(fstring, filestring);

                    //else if (fstring.Contains("Application_Solution") || fstring.Contains("PlatformApp_Serialization"))
                    //{
                    //    // copy change name and extension
                    //    string dcname = Path.GetDirectoryName(fstring);
                    //    dcname = dcname.Substring(dcname.LastIndexOf("\\") + 1);

                    //    string ftext = File.ReadAllText(fstring);
                    //    ftext = ftext.Replace("Application_Solution.h\"", "Application_Solution.hpp\"");
                    //    ftext = ftext.Replace("PlatformApp_Serialization.h\"", "PlatformApp_Serialization.hpp\"");

                    //    string inostring = Path.GetFileName(filestring);
                    //    //inostring = odstring + "\\..\\Arduino\\" + dcname + "\\" + inostring;
                    //    inostring = Path.GetFullPath(inostring).Replace(".h",".hpp");
                    //    if (File.Exists(inostring))
                    //        File.Delete(inostring);
                    //    File.WriteAllText(inostring, ftext);
                    //}
                    //else
                    //    File.Copy(fstring, filestring);

                }
            }

            // recursively process each sub-directory
            foreach (string subDir in Directory.GetDirectories(target_dir))
            {
                if (!subDir.Contains("tests\\testApps") && !subDir.Contains("tests\\testMainFiles"))
                    CopyModifyDirectoryRecursively(subDir, dirstring);
                else if (subDir.Contains("tests\\testApps"))
                {
                    if (subDir.Contains("apiModules") || subDir.Contains("deviceModules"))
                    {
                        CopyModifyDirectoryRecursively(subDir, dirstring);
                    }
                    else
                    {
                        string tempDirstring = subDir + "\\" + dirstring.Substring(dirstring.LastIndexOf("\\") + 1);
                        CopyModifyDirectoryRecursively(tempDirstring, dirstring);
                    }

                }
                else if (subDir.Contains("tests\\testMainFiles"))
                {
                    string tempDirstring = subDir + "\\" + dirstring.Substring(dirstring.LastIndexOf("\\") + 1) + "\\Arduino";
                    CopyModifyDirectoryRecursively(tempDirstring, dirstring);
                }
            }

        }




    }






}
