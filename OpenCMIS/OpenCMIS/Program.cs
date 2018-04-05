using PortCMIS;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using PortCMIS.Data;
using System;
using System.Collections.Generic;

namespace OpenCMIS
{
    class Program
    {
        static void Main(string[] args)
        {
            // default factory implementation
            SessionFactory factory = SessionFactory.NewInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // user credentials
            parameters[SessionParameter.User] = "admin";
            parameters[SessionParameter.Password] = "admin";

            // connection settings
            parameters[SessionParameter.AtomPubUrl] = "http://localhost:8080/core/atom/bedroom";
            parameters[SessionParameter.BindingType] = BindingType.AtomPub;
            parameters[SessionParameter.RepositoryId] = "bedroom";

            // create session
            ISession session = factory.CreateSession(parameters);
            Console.WriteLine("Repository: " + session.GetRootFolder().Name);

            // get root folder
            var root = session.GetRootFolder();

            // objects
            Console.WriteLine("Object ids");
            foreach(ICmisObject o in root.GetChildren())
            {
                Console.WriteLine(o.Id);
                if (o.Relationships != null)
                {
                    foreach (IRelationship r in o.Relationships)
                    {
                        Console.WriteLine("rel:" + r.Id);
                    }

                    //foreach(IRelationship r in session.GetRelationships(o.Id,true))
                }
            }

            //relationships
            Console.WriteLine("rel ids");
            IItemEnumerable<IQueryResult> results = session.Query("SELECT * FROM cmis:document", false);

            foreach (IQueryResult hit in results)
            {
                foreach (PropertyData property in hit.Properties)
                {

                    string queryName = property.QueryName;
                    object value = property.FirstValue;

                    Console.WriteLine(queryName + ": " + value);
                }
                Console.WriteLine("--------------------------------------");
            }


            Console.ReadKey();
        }
    }
}
