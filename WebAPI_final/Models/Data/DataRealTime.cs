using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace WebAPI_final.Models.Data
{
    public class JsonMapper
    {
        // Map the JSON data downloaded from a webservice into a C# object T
        public static T _download_serialized_json_data<T>(string url) where T : new()
        {
            // attempt to download JSON data as a string
            string responseFromServer = string.Empty;
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Clean up the streams and the response.
                reader.Close();
            }
            catch (Exception)
            {
                //TODO
            }
            // if string with JSON data is not empty, deserialize it to class and return its instance 
            return !string.IsNullOrEmpty(responseFromServer) ? JsonConvert.DeserializeObject<T>(responseFromServer) : new T();
        }
    }

    public class Meta
    {
        public string type { get; set; }
        public int start { get; set; }
        public int count { get; set; }
    }

    public class Fields
    {
        public string name { get; set; }
        public string price { get; set; }
        public string symbol { get; set; }
        public string ts { get; set; }
        public string type { get; set; }
        public string utctime { get; set; }
        public string volume { get; set; }
    }

    public class Resource2
    {
        public string classname { get; set; }
        public Fields fields { get; set; }
    }

    public class Resource
    {
        public Resource2 resource { get; set; }
    }

    public class List
    {
        public Meta meta { get; set; }
        public List<Resource> resources { get; set; }
    }

    public class RootObject
    {
        public List list { get; set; }
    }

}
