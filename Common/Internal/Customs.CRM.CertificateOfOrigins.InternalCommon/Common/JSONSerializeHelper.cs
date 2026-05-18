using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public static class JSONSerializeHelper<T>
    {
        public static string JSONSerialize(object entity)
        {
            var setting = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("yyyy-MM-dd")
            };
            var memoryStream = new MemoryStream();
            var js = new DataContractJsonSerializer(typeof(T),setting);
            js.WriteObject(memoryStream, entity);
            byte[] json = memoryStream.ToArray();
            memoryStream.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        public static T JSONDeserialize(string json)
        {
            var setting = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("yyyy-MM-dd")
            };
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var js = new DataContractJsonSerializer(typeof(T),setting);
            var deserializedJson = (T)js.ReadObject(memoryStream);
            memoryStream.Close();
            return deserializedJson;
        }
    }

}
