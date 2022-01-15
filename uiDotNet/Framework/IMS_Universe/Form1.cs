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
        RepoManager FormsRepoManager;       // Repo Manager Object from Library
        IMSConfigStruct IMSConfiguration;   // from Library

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
                jsonString = File.ReadAllBytes(Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json"));
            else
                jsonString = new byte[0];
            // otherwise prompt for location

            tempConfig = RepoManager.CreateIMSConfigStructJSON(ref jsonString);
            if (File.Exists(Path.GetFullPath(tempConfig.Path2RootRepository + "\\imsConf.json")))
                IMSConfiguration = tempConfig;
        }
        void saveConfigtoFile()
        {
            // save to default location
            //IMSConfigStruct tempStruct = RepoManager.CreateDefaultIMSConfigStruct();
            byte[] jsonString = RepoManager.SerializeIMSConfigStructJSON(ref IMSConfiguration);
            File.WriteAllBytes(Path.GetFullPath(IMSConfiguration.Path2RootRepository + "\\imsConf.json"), jsonString);
        }


        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadConfigFromFile();
        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            loadConfigFromFile();
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveConfigtoFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveConfigtoFile();
        }
    }
}
