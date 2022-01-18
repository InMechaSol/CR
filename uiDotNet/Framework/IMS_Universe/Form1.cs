using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ccLib_netCore;
using System.IO;
using System.Text.RegularExpressions;

namespace IMS_Universe
{    
    public partial class Form1 : Form
    {
        bool configLoaded = false;
        

        
        RepoManager RepositoryManager;      // from Library
        ExecutionSystem UniversExeSys;      // from Library

        List<ComputeModule> exeSysModules;

        public Form1()
        {
            InitializeComponent();

            RepositoryManager = new RepoManager();

            exeSysModules = new List<ComputeModule>();
            exeSysModules.Add(RepositoryManager);


            UniversExeSys = new ExecutionSystem(exeSysModules);

            treeView2.Nodes[0].Nodes[0].Tag = new ExtProcCmdStruct();
            ((ExtProcCmdStruct)treeView2.Nodes[0].Nodes[0].Tag).workingDirString = "C:\\IMS";
            ((ExtProcCmdStruct)treeView2.Nodes[0].Nodes[0].Tag).cmdString = "C:\\Program Files\\Git\\bin\\git.exe";
            ((ExtProcCmdStruct)treeView2.Nodes[0].Nodes[0].Tag).cmdArguments = "remote -v";
            ((ExtProcCmdStruct)treeView2.Nodes[0].Nodes[0].Tag).timeOutms = 5000;
        }
#region System.Window.Forms Implementations of Interface Functions
        void loadConfigFromFile()
        {
            byte[] jsonString;
            IMSConfigStruct tempConfig;

            // check default location
            if (File.Exists(Path.GetFullPath(RepositoryManager.IMSConfiguration.Path2RootRepository + "\\imsConf.json")))
            {
                jsonString = File.ReadAllBytes(Path.GetFullPath(RepositoryManager.IMSConfiguration.Path2RootRepository + "\\imsConf.json"));
                tempConfig = RepoManager.CreateIMSConfigStructJSON(ref jsonString);
                if (File.Exists(Path.GetFullPath(tempConfig.Path2RootRepository + "\\imsConf.json")))
                {
                    RepositoryManager.IMSConfiguration = tempConfig;
                }
            }
            else
            {
                MessageBox.Show("The imsConf.json files was not found at the directory:\n" + RepositoryManager.IMSConfiguration.Path2RootRepository + "\nTrying Default Directory C:\\IMS\\imsConf.json", "Not Found" );
                if (File.Exists(Path.GetFullPath("C:\\IMS\\imsConf.json")))
                {
                    jsonString = File.ReadAllBytes(Path.GetFullPath("C:\\IMS\\imsConf.json"));
                    tempConfig = RepoManager.CreateIMSConfigStructJSON(ref jsonString);
                    if (File.Exists(Path.GetFullPath(tempConfig.Path2RootRepository + "\\imsConf.json")))
                    {
                        RepositoryManager.IMSConfiguration = tempConfig;
                        RepositoryManager.newConfigLoaded = true;
                        configLoaded = true;
                        return;
                    }
                }
                MessageBox.Show("The imsConf.json files was not found at the default directory.\nCreating Default Configuration to be Saved", "Not Found");
                RepositoryManager.IMSConfiguration = RepoManager.CreateDefaultIMSConfigStruct();

            }
            RepositoryManager.newConfigLoaded = true;
            configLoaded = true;
            // otherwise prompt for location
        }
        void unloadConfig()
        {
            RepositoryManager.IMSConfiguration.Path2RootRepository = "";
            RepositoryManager.updateConfigflag = true;
            configLoaded = false;
        }
        void saveConfigtoFile()
        {
            // save to default location
            //IMSConfigStruct tempStruct = RepoManager.CreateDefaultIMSConfigStruct();
            byte[] jsonString = RepoManager.SerializeIMSConfigStructJSON(RepositoryManager.IMSConfiguration);
            File.WriteAllBytes(Path.GetFullPath(RepositoryManager.IMSConfiguration.Path2RootRepository + "\\imsConf.json"), jsonString);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            updateRepoManagerTreeView();
            updateStatusStrip();
            updateMenuBar();
            RepositoryManager.updateConfigflag = false;
        }
        TreeNode fromGUItreeNode(guiTreeNode nodeIn)
        {
            TreeNode tempNode = new TreeNode();
            tempNode.Name = nodeIn.Name;
            tempNode.Text = nodeIn.Text;
            tempNode.ToolTipText = nodeIn.ToolTipText;
            tempNode.Tag = nodeIn.Tag;
            if (nodeIn.Nodes != null)
                if (nodeIn.Nodes.Count > 0)
                    foreach (guiTreeNode subNode in nodeIn.Nodes)
                        tempNode.Nodes.Add(fromGUItreeNode(subNode));
            return tempNode;
        }
        void updateRepoManagerTreeView()
        {
            if(RepositoryManager.updateConfigflag)
            {
                treeView1.Nodes.Clear();
                if (configLoaded)
                {                    
                    treeView1.Nodes.Add(fromGUItreeNode(RepositoryManager.IMSConfigNode));                    
                    treeView1.Nodes.Add(fromGUItreeNode(RepositoryManager.RepositoryTreeRootNode));  
                    treeView1.ExpandAll();
                }                    
                treeView1.Refresh();
            }
        }
        void updateStatusStrip()
        {
            if(RepositoryManager.updateConfigflag)
            {
                if (configLoaded)
                    toolStripStatusLabel1.Text = Path.GetFullPath(RepositoryManager.IMSConfiguration.Path2RootRepository + "\\imsConf.json");
                else
                    toolStripStatusLabel1.Text = "Not Loaded";
            }
        }
        void updateMenuBar()
        {
            if(RepositoryManager.updateConfigflag)
            {
                if (configLoaded)
                {
                    loadToolStripMenuItem.Text = "Un-Load";
                    saveToolStripMenuItem.Enabled = true ;
                    loadToolStripMenuItem1.Text = "Un-Load";
                    saveToolStripMenuItem1.Enabled = true;
                }
                else
                {
                    loadToolStripMenuItem.Text = "Load";
                    saveToolStripMenuItem.Enabled = false;
                    loadToolStripMenuItem1.Text = "Load";
                    saveToolStripMenuItem1.Enabled = false;
                }
            }
        }
#endregion
#region Callbacks for Systems.Windows.Forms objects
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!configLoaded)
                loadConfigFromFile();
            else
                unloadConfig();
        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!configLoaded)
                loadConfigFromFile();
            else
                unloadConfig();
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveConfigtoFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveConfigtoFile();
        }
        
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = treeView1.SelectedNode.Tag;
            propertyGrid1.Refresh();
        }


        private void treeView2_DoubleClick(object sender, EventArgs e)
        {
            propertyGrid2.SelectedObject = treeView2.SelectedNode.Tag;
            propertyGrid2.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExtProcCmdStruct thisCmd = (ExtProcCmdStruct)treeView2.Nodes[0].Nodes[0].Tag;

            if(thisCmd.workingDirString!="")
            {
                if (Directory.Exists(Path.GetFullPath(thisCmd.workingDirString)))
                {
                    if (thisCmd.cmdString != "")
                    {
                        if (File.Exists(Path.GetFullPath(thisCmd.cmdString)))
                        {
                            List<ExtProcCmdStruct> cmdsIn = new List<ExtProcCmdStruct>();
                            cmdsIn.Add(thisCmd);
                            UniversExeSys.ThirdPartyTools.executeCMDS(cmdsIn);
                            richTextBox1.Text = "";
                            richTextBox1.Text += thisCmd.outANDerrorResults;
                            return;
                        }  
                    }
                    MessageBox.Show($"The Process Command Specified Does Not Exist!\n{thisCmd.cmdString}", "Bad Command Path");
                    return;
                }
            }
            MessageBox.Show($"The Working Directory Specified Does Not Exist!\n{thisCmd.workingDirString}", "Bad Working Directory");
        }
#endregion
    }
}
