using Newtonsoft.Json;
using PortCMIS;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using PortCMIS.Data;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace OpenCMIS
{
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
        public LocalRelationship value { get; set; }
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
    class LocalRelationship
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

    class Program
    {
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
        /// Progress bar
        /// </summary>
        public static ProgressBar Progress;

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
            var RelationshipCount = Relationships.rows.Count;

            //create session
            var ns = new NemakiServer();
            ISession session = ns.CreateSession();
            Console.WriteLine("Repository: " + session.GetRootFolder().Name);

            List<string> OutputLines = new List<string>();
            int i = 0;
            Console.WriteLine("Checking each relationship...");
            Progress = new ProgressBar();
            foreach (var lr in Relationships.rows)
            {
                i++;
                try
                {
                    var Source = session.GetObject(lr.value.sourceId);
                }
                catch (CmisObjectNotFoundException e)
                {
                    OutputLines.Add("Relationship: " + lr.id + ", Document: " + lr.value.sourceId);
                    //session.Delete(new ObjectId(lr.id));
                }
                try
                {
                    var Target = session.GetObject(lr.value.targetId);
                }
                catch (CmisObjectNotFoundException e)
                {
                    OutputLines.Add("Relationship: " + lr.id + ", Document: " + lr.value.targetId);
                    session.Delete(new ObjectId(lr.id));
                }
                Progress.Report((double)i / RelationshipCount);
            }
            Progress.Dispose();

            //output to file
            System.IO.File.WriteAllLines(ConfigurationManager.AppSettings["SaveFilePath"] + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt", OutputLines);

            Console.WriteLine("Done.");

            Console.ReadKey();
        }

        /// <summary>
        /// Create session from app.config data
        /// </summary>
        /// <returns></returns>
        public static ISession CreateSession_riley()
        {
            // default factory implementation
            SessionFactory factory = SessionFactory.NewInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // user credentials
            parameters[SessionParameter.User] = ConfigurationManager.AppSettings["Username"];
            parameters[SessionParameter.Password] = ConfigurationManager.AppSettings["Password"];

            // connection settings
            parameters[SessionParameter.RepositoryId] = ConfigurationManager.AppSettings["Repository"];
            parameters[SessionParameter.AtomPubUrl] = ConfigurationManager.AppSettings["CMISUrl"];
            parameters[SessionParameter.BindingType] = BindingType.AtomPub;

            // create session
            var session = factory.CreateSession(parameters);

            return session;
        }

        public static void RelationshipTest(ISession session)
        {
            // Check if the repo supports relationships
            IObjectType relationshipType = null;
            try
            {
                relationshipType = session.GetTypeDefinition("cmis:relationship");
            }
            catch (CmisObjectNotFoundException e)
            {
                relationshipType = null;
            }

            if (relationshipType == null)
            {
                Console.WriteLine("Repository does not support cmis:relationship objects");
            }
            else
            {
                IObjectType cmiscustomRelationshipType = null;
                try
                {
                    cmiscustomRelationshipType = session.GetTypeDefinition("nemaki:bidirectionalRelationship");
                }
                catch (CmisObjectNotFoundException e)
                {
                    cmiscustomRelationshipType = null;
                }

                if (cmiscustomRelationshipType == null)
                {
                    Console.WriteLine("Repository does not support nemaki:bidirectionalRelationship objects");
                }
                //else
                //{
                //    Console.WriteLine("Creating folders for relationships example");

                //    newFolderProps = new HashMap<String, String>();
                //    newFolderProps.put(PropertyIds.OBJECT_TYPE_ID, "cmis:folder");
                //    newFolderProps.put(PropertyIds.NAME, "ADGFolderAssociations");
                //    Folder folderAssociations = root.createFolder(newFolderProps);

                //    newFileProps = new HashMap<String, String>();
                //    newFileProps.put(PropertyIds.OBJECT_TYPE_ID, "D:cmiscustom:document");
                //    newFileProps.put(PropertyIds.NAME, "ADGFileSource");
                //    Document sourceDoc = folderAssociations.createDocument(newFileProps, null, VersioningState.MAJOR);

                //    newFileProps.put(PropertyIds.OBJECT_TYPE_ID, "cmis:document");
                //    newFileProps.put(PropertyIds.NAME, "ADGFileTarget");
                //    Document targetDoc = folderAssociations.createDocument(newFileProps, null, VersioningState.MAJOR);

                //    Map<String, String> relProps = new HashMap<String, String>();
                //    relProps.put("cmis:sourceId", sourceDoc.getId());
                //    relProps.put("cmis:targetId", targetDoc.getId());
                //    relProps.put("cmis:objectTypeId", "R:cmiscustom:assoc");
                //    ObjectId relId = session.createRelationship(relProps, null, null, null);
                //    System.out.println("created relationship");
                //}
            }
        }
    }
}


//string sourceId = "96950dce8eb20fd5c5d3992835009186";
//string targetId = "96950dce8eb20fd5c5d399283500235e";

//IDictionary<string, object> properties = new Dictionary<string, object>();
//properties[PropertyIds.Name] = "a new relationship";
//properties[PropertyIds.ObjectTypeId] = "nemaki:bidirectionalRelationship";
//properties[PropertyIds.SourceId] = sourceId;
//properties[PropertyIds.TargetId] = targetId;

//IObjectId newRelId = session.CreateRelationship(properties);