using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ccLib_netCore
{
    public class Platform_Serialization
    {
        public static RepoNodeStruct tryParseRepoNodeStruct(ref byte[] refJsonString)
        {
            var options = new JsonSerializerOptions { IncludeFields = true };
            Utf8JsonReader utf8Reader = new Utf8JsonReader(refJsonString);
            return JsonSerializer.Deserialize<RepoNodeStruct>(ref utf8Reader, options);
        }
        public static byte[] packageRepoNodeStruct(ref RepoNodeStruct refRepoNode)
        {
            var options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
            return JsonSerializer.SerializeToUtf8Bytes(refRepoNode, options);
        }
        public static IMSConfigStruct tryParseIMSConfigStruct(ref byte[] refJsonString)
        {
            var options = new JsonSerializerOptions { IncludeFields = true };
            Utf8JsonReader utf8Reader = new Utf8JsonReader(refJsonString);
            return JsonSerializer.Deserialize<IMSConfigStruct>(ref utf8Reader, options);
        }
        public static byte[] packageIMSConfigStruct(ref IMSConfigStruct refRepoNode)
        {
            var options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
            byte[] tempBytes = JsonSerializer.SerializeToUtf8Bytes<IMSConfigStruct>(refRepoNode, options);
            return tempBytes;
        }
    }
}
