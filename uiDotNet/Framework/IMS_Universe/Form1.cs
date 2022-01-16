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
        bool updateConfigflag = true;
        IMSConfigStruct IMSConfiguration = new IMSConfigStruct();   // from Library
        repoTreeNode RepositoryTreeRootNode = new repoTreeNode();

        public Form1()
        {
            IMSConfiguration.Path2RootRepository = Path.GetFullPath("C:\\IMS");

            InitializeComponent();
        }

        void loadConfigFromFile()
        {
            byte[] jsonString;
            IMSConfigStruct tempConfig;

            // check default location
            if (File.Exists(Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json")))
            {
                jsonString = File.ReadAllBytes(Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json"));
                tempConfig = RepoManager.CreateIMSConfigStructJSON(ref jsonString);
                if (File.Exists(Path.GetFullPath(tempConfig.Path2RootRepository + "\\imsConf.json")))
                {
                    IMSConfiguration = tempConfig;
                }
            }
            else
            {
                MessageBox.Show("The imsConf.json files was not found at the directory:\n" + IMSConfiguration.Path2RootRepository + "\nTrying Default Directory C:\\IMS\\imsConf.json", "Not Found" );
                if (File.Exists(Path.GetFullPath("C:\\IMS\\imsConf.json")))
                {
                    jsonString = File.ReadAllBytes(Path.GetFullPath("C:\\IMS\\imsConf.json"));
                    tempConfig = RepoManager.CreateIMSConfigStructJSON(ref jsonString);
                    if (File.Exists(Path.GetFullPath(tempConfig.Path2RootRepository + "\\imsConf.json")))
                    {
                        IMSConfiguration = tempConfig;
                        updateConfigflag = true;
                        configLoaded = true;
                        return;
                    }
                }
                MessageBox.Show("The imsConf.json files was not found at the default directory.\nCreating Default Configuration to be Saved", "Not Found");
                IMSConfiguration = RepoManager.CreateDefaultIMSConfigStruct();

            }
            updateConfigflag = true;
            configLoaded = true;
            // otherwise prompt for location
        }
        void unloadConfig()
        {
            IMSConfiguration.Path2RootRepository = "";
            updateConfigflag = true;
            configLoaded = false;
        }
        void saveConfigtoFile()
        {
            // save to default location
            //IMSConfigStruct tempStruct = RepoManager.CreateDefaultIMSConfigStruct();
            byte[] jsonString = RepoManager.SerializeIMSConfigStructJSON(ref IMSConfiguration);
            File.WriteAllBytes(Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json"), jsonString);
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
            if(updateConfigflag)
            {
                treeView1.Nodes.Clear();

                if (configLoaded)
                {

                    guiTreeNode IMSConfigNode = RepoManager.createGUItreeNodefromConfig(IMSConfiguration);
                    treeView1.Nodes.Add(fromGUItreeNode(IMSConfigNode));

                    RepositoryTreeRootNode = RepoManager.createREPOtreeNodefromRepoList(IMSConfiguration.Repositories);
                    treeView1.Nodes.Add(fromGUItreeNode(RepositoryTreeRootNode));

                    treeView1.ExpandAll();
                }
                    
                treeView1.Refresh();
            }
        }

        void updateStatusStrip()
        {
            if(updateConfigflag)
            {
                if (configLoaded)
                    toolStripStatusLabel1.Text = Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json");
                else
                    toolStripStatusLabel1.Text = "Not Loaded";
            }
        }
        void updateMenuBar()
        {
            if(updateConfigflag)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateRepoManagerTreeView();
            updateStatusStrip();
            updateMenuBar();
            updateConfigflag = false;
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = treeView1.SelectedNode.Tag;
            propertyGrid1.Refresh();
        }
    }
}
