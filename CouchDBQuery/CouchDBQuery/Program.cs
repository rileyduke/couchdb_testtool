using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Configuration;

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
        public string sourceId { get; set; }
        public string targetId { get; set; }
    }

    /// <summary>
    /// Document
    /// </summary>
    class Document
    {
        public string _id { get; set; }
    }

    class MissingRelationship
    {
        public string RelationshipId { get; set; }
        public string DocumentId { get; set; }
    }

    /// <summary>
    /// Program / logic
    /// </summary>
    class Program
    {
        /// <summary>
        /// couchdb url
        /// </summary>
        public static string CouchDBUrl = ConfigurationManager.AppSettings["CouchDBUrl"];

        /// <summary>
        /// relationship URL
        /// </summary>
        public static string GetRelationshipUrl = ConfigurationManager.AppSettings["GetRelationshipUrl"];

        /// <summary>
        /// relationships count
        /// </summary>
        public static string GetRelationshipCountUrl = ConfigurationManager.AppSettings["GetRelationshipCountUrl"];

        /// <summary>
        /// documents url
        /// </summary>
        public static string GetDocumentUrl = ConfigurationManager.AppSettings["GetDocumentUrl"];

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
        public static List<Action> TaskList = new List<Action>();

        /// <summary>
        /// 
        /// </summary>
        public static List<MissingRelationship> MissingRelationships = new List<MissingRelationship>();

        /// <summary>
        /// http client
        /// </summary>
        public static HttpClient client = new HttpClient();


        public static int ThreadCount = 0;
        public static int CompletedThreadCount = 0;
        public static ProgressBar Progress;

        /// <summary>
        /// Check document - if nothing is returned then add to missing list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RelationshipId"></param>
        /// <returns></returns>
        public static void CheckDocument(string id, string RelationshipId)
        {
            try
            {
                WebClient wClient = new WebClient();
                //get the JSON response from couchdb
                var response = wClient.DownloadString(GetDocumentUrl + "%22" + id + "%22");

                //serialize 
                DocJsonResponse CheckedDoc = JsonConvert.DeserializeObject<DocJsonResponse>(response);

                //add to missing list
                if (CheckedDoc.rows.Count == 0)
                {
                    MissingRelationships.Add(new MissingRelationship()
                    {
                        DocumentId = id,
                        RelationshipId = RelationshipId
                    });
                }

                CompletedThreadCount++;
                Progress.Report((double)CompletedThreadCount / ThreadCount);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        /// <summary>
        /// get a document from couchdb
        /// </summary>
        /// <param name="r"></param>
        public static void GetDocument(RelRowObject r)
        {
             CheckDocument(r.value.sourceId, r.id);
             CheckDocument(r.value.targetId, r.id);
        }

        /// <summary>
        /// main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //download the list of relationships
            Console.WriteLine("Downloading relationships...");
            var RelJSONString = new WebClient().DownloadString(GetRelationshipUrl);
            RelJsonResponse Relationships = JsonConvert.DeserializeObject<RelJsonResponse>(RelJSONString);

            ThreadCount = Relationships.rows.Count;
            
            //check parentId and targetId in each relationship
            foreach (RelRowObject r in Relationships.rows)
            {
                TaskList.Add(() => GetDocument(r));
            }

            //invoke all tasks in parallel
            Console.WriteLine("Checking each relationship...");
            Progress = new ProgressBar();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(ConfigurationManager.AppSettings["MaxParallelism"]) };
            Parallel.Invoke(options, TaskList.ToArray());

            Progress.Dispose();
            
            List<string> OutputLines = new List<string>();
            foreach (MissingRelationship rel in MissingRelationships)
            {
                OutputLines.Add("Relationship: " + rel.RelationshipId + ", Document: " + rel.DocumentId);
            }

            System.IO.File.WriteAllLines(ConfigurationManager.AppSettings["SaveFilePath"] + DateTime.Now.ToString("yyyyMMddhhmmss") +  ".txt", OutputLines);

            Console.WriteLine("\n done");

            Console.ReadKey();
        }
    }
}
