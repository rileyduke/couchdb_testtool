using PortCMIS.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace TextFileRead
{
    class Program
    {
        //folder where files are saved
        public static string FileLocation = ConfigurationManager.AppSettings["SavedFileFolder"];
        
        //beginning of file name for all archived files
        public static string FileNamePre = ConfigurationManager.AppSettings["FileNamePre"];

        public static string RelationshipStringPre = ConfigurationManager.AppSettings["RelationshipStringPre"];

        public static string OldRelationshipString = ConfigurationManager.AppSettings["OldRelationshipString"];

        public static string NewRelationshipString = ConfigurationManager.AppSettings["NewRelationshipString"];

        public static NemakiServer ns = new NemakiServer();

        public static string CouchDbUrl = ns.GetCouchDbBaseUrl();

        private static WebClient _webClient = new WebClient();

        //get sorted list of files in specified folder
        public static string GetLatestFile()
        {
            DirectoryInfo d = new DirectoryInfo(FileLocation);
            List<string> Files = d.GetFiles("*.txt")
                .Where(x=>x.Name.Substring(0,x.Name.Length < FileNamePre.Length ? x.Name.Length : FileNamePre.Length) == FileNamePre)
                .Select(x=>x.Name)
                .OrderBy(x=>x)
                .ToList(); 
            
            if(Files.Count > 0)
            {
                return Files[Files.Count - 1];
            }else
            {
                return "";
            }
        }

        //gets a list of every line in the file
        public static List<string> GetRelationshipList(string Path){
            List<string> Contents = new List<string>();

            if(File.Exists(Path))
            {
                string FileContentsRaw = File.ReadAllText(Path);
                foreach(string Line in FileContentsRaw.Split("\n"))
                {
                    var words = Line.Split(" ");
                    for(var i=0; i<words.Length; i++)
                    {
                        if(words[i] == RelationshipStringPre)
                        {
                            Contents.Add(words[i+1].Substring(0,words[i+1].Length-1));
                        }
                    }
                }
            }

            return Contents;
        }

        /// <summary>
        /// Gets the document from the specified Url
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string GetDocument(string Id)
        {
            var RelJSONString = _webClient.DownloadString(CouchDbUrl + Id);

            return RelJSONString;
        }

        /// <summary>
        /// renames the document type to a pseudo-deleted type
        /// </summary>
        /// <param name="Id"></param>
        public static void PseudoDeleteDocument(string Id)
        {
            string Doc = GetDocument(Id);

            Doc = Doc.Replace(OldRelationshipString, NewRelationshipString);

            _webClient.UploadString(CouchDbUrl + Id + "/","PUT", Doc);
        }

        static void Main(string[] args)
        {
            //get relationship list from file
            var LatestFile = GetLatestFile();
            var RelationshipList = GetRelationshipList(FileLocation + LatestFile);

            //create session with nemakiware through portcmis
            //not going to use portCMIS here

            PseudoDeleteDocument("4b6e795f8fb84d4531e6f59f04000d83");

            //delete each relationship coming from the text file.
            var i = 0;
            foreach(string r in RelationshipList)
            {
                i++;
                Console.WriteLine("Deleting Relationship: " + r + " (" + i.ToString() + " of " + RelationshipList.Count + ")");
                //delete
                //session.Delete(new ObjectId(r)); // not actually going to delete here
                //instead of delete, do a pseudo-delete and change the document type from nemaki-relationship to nemaki-relationship-deleted
                
            }

            Console.ReadKey();
        }
    }
}
