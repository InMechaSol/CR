using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary_netFramework
{
    public struct imsRepoStruct
    {
        public string Name;
        public string URL;
        public List<string> SubModNames;
    }
    public struct imsRepoConfigStruct
    {
        public string Path2GitBin;
        public string Path2DoxyGenBin;
        public string Path2GraphVizBin;
        public string Path2RepoRootDir;
        public List<imsRepoStruct> Repositories;
    }
    
    public class RepoManager
    {
        public static imsRepoStruct createimsConfigStruct()
        {
            imsRepoStruct outStruct;
            outStruct.Name = "";
            outStruct.URL = "";
            outStruct.SubModNames = new List<string>();
            return outStruct;
        }
        public static imsRepoStruct createimsConfigStructJSON(string JSONstring)
        {
            imsRepoStruct outStruct = createimsConfigStruct();
            outStruct.Name = "";
            outStruct.URL = "";
            outStruct.SubModNames = new List<string>();
            return outStruct;
        }

    }
}
