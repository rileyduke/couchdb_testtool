using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDBQuery
{
    //total_rows":3,"offset":0,"rows":[
    /// <summary>
    /// Generic JSON Response
    /// </summary>
    class GenJsonResponse
    {
        public int total_rows { get; set; }
        public int offset { get; set; }
    }

    /// <summary>
    /// Relationsihp JSON Response
    /// </summary>
    class RelJsonResponse : GenJsonResponse
    {
        public List<RelRowObject> rows { get; set; }
    }

    /// <summary>
    /// Document JSON Response
    /// </summary>
    class DocJsonResponse : GenJsonResponse
    {
        public List<DocRowObject> rows { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    class RowObject
    {
        public string id { get; set; }
        public string key { get; set; }
    }

    /// <summary>
    /// relationship row object
    /// </summary>
    class RelRowObject : RowObject
    {
        public Relationship value { get; set; }
    }

    /// <summary>
    /// Document row object
    /// </summary>
    class DocRowObject : RowObject
    {
        public Document value { get; set; }
    }

    /// <summary>
    /// relationship
    /// </summary>
    /// "id":"96950dce8eb20fd5c5d3992835019581","key":"96950dce8eb20fd5c5d3992835019581","value":{"_id":"96950dce8eb20fd5c5d3992835019581","_rev":"2-2a858bac17956ab5b3bf272cd3e019c1","type":"cmis:relationship","created":"2018-03-28T08:24:26.678+0000","creator":"admin","modified":"2018-03-28T08:24:26.678+0000","modifier":"admin","name":"Riley","acl":{"entries":[]},"aclInherited":true,"subTypeProperties":[],"aspects":[],"objectType":"nemaki:bidirectionalRelationship","changeToken":"96950dce8eb20fd5c5d399283501a3b9","sourceId":"862a5fe26cfc4c0f5101dd1701cf4824","targetId":"862a5fe26cfc4c0f5101dd1701cf5344","content":true,"document":false,"folder":false,"attachment":false,"relationship":true,"policy":false
    class Relationship
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string type { get; set; }
        public string created { get; set; }
        public string creator { get; set; }
        public string modified { get; set; }
        public string modifier { get; set; }
        public string name { get; set; }
        public string objectType { get; set; }
        public string changeToken { get; set; }
        public string sourceId { get; set; }
        public string targetId { get; set; }
    }

    /// <summary>
    /// Document
    /// </summary>
    class Document
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string type { get; set; }
        public string created { get; set; }
        public string creator { get; set; }
        public string modified { get; set; }
        public string modifier { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string parentId { get; set; }
        //public string aclInherited { get; set; }
        //public string subTypeProperties { get; set; }
        //public string aspects { get; set; }
        //public string objectType { get; set; }
        //public string changeToken { get; set; }
        //public string attachmentNodeId { get; set; }
        //public string versionSeriesId { get; set; }
        //public string latestMajorVersion { get; set; }
        //public string majorVersion { get; set; }
        //public string versionLabel { get; set; }
        //public string privateWorkingCopy { get; set; }
        //public string content { get; set; }
        //public string document { get; set; }
        //public string folder { get; set; }
        //public string attachment { get; set; }
        //public string relationship { get; set; }
        //public string policy { get; set; }
        //public string latestVersion { get; set; }
    }

    /// <summary>
    /// Program / logic
    /// </summary>
    class Program
    {
        /// <summary>
        /// couchdb url
        /// </summary>
        public static string CouchDBUrl = "http://localhost:5984/";

        /// <summary>
        /// relationship URL
        /// </summary>
        public static string GetRelationshipUrl = "http://localhost:5984/bedroom/_design/_repo/_view/relationships";

        /// <summary>
        /// relationships count
        /// </summary>
        public static string GetRelationshipCountUrl = "http://localhost:5984/bedroom/_design/files/_view/relationships_count";

        /// <summary>
        /// documents url
        /// </summary>
        public static string GetDocumentUrl = "http://localhost:5984/bedroom/_design/_repo/_view/documents?key=";

        /// <summary>
        /// relationship count (for paging)
        /// </summary>
        public static int RelationshipCount = 0;

        /// <summary>
        /// page size
        /// </summary>
        public static int PageSize = 2;

        /// <summary>
        /// tasks for querying
        /// </summary>
        public static List<Task> TaskList = new List<Task>();

        /// <summary>
        /// 
        /// </summary>
        public static List<string> MissingRelationships = new List<string>();

        /// <summary>
        /// http client
        /// </summary>
        public static HttpClient client = new HttpClient();

        public static async Task<bool> CheckDocument(string id, string RelationshipId)
        {
            HttpResponseMessage response = await client.GetAsync(GetDocumentUrl + "%22" + id + "%22");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            DocJsonResponse CheckedDoc = JsonConvert.DeserializeObject<DocJsonResponse>(responseBody);

            if(CheckedDoc.rows.Count == 0)
            {
                MissingRelationships.Add(RelationshipId);
            }

            return true;
        }

        /// <summary>
        /// get a document from couchdb
        /// </summary>
        /// <param name="r"></param>
        public static async Task GetDocument(RelRowObject r)
        {
            await CheckDocument(r.value.sourceId, r.id);
            await CheckDocument(r.value.targetId, r.id);
        }

        /// <summary>
        /// main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            //download the list of relationships
            var RelJSONString = new WebClient().DownloadString(GetRelationshipUrl);
            RelJsonResponse Relationships = JsonConvert.DeserializeObject<RelJsonResponse>(RelJSONString);

            //check parentId and targetId in each relationship
            foreach (RelRowObject r in Relationships.rows)
            {
                TaskList.Add(GetDocument(r));
            }

            //wait for all threads to finish
            Task.WaitAll(TaskList.ToArray());

            foreach (string rel in MissingRelationships)
            {
                Console.WriteLine("Missing Relationship: " + rel);
            }

            Console.WriteLine("done");

            Console.ReadKey();
        }
    }
}
