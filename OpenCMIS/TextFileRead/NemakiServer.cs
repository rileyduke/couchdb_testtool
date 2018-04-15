using PortCMIS;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace TextFileRead
{
    class NemakiServer
    {
        public NemakiServer()
        {

        }

        public ISession CreateSession()
        {

            //var prop = Properties.Settings.Default;
            var parameters = new Dictionary<string, string>();

            // user credentials
            //本当はヘッダ認証を通す必要があるがいまはadminで
            parameters[SessionParameter.User] = "admin";
            parameters[SessionParameter.Password] = "admin";

            // connection settings
            var url = GetCmisAtomEndpoint();
            parameters[SessionParameter.AtomPubUrl] = url;
            parameters[SessionParameter.BindingType] = BindingType.AtomPub;
            parameters[SessionParameter.RepositoryId] = ConfigurationManager.AppSettings["SubaruRepositoryName"];

            parameters[ConfigurationManager.AppSettings["SubaruHeaderAuthenticatedRemoteUser"]] = ConfigurationManager.AppSettings["SubaruUserId"];


            // create session
            var session = SessionFactory.NewInstance().CreateSession(parameters);

            return session;
        }


        public static String GetRestEndpoint()
        {
            //var prop = Properties.Settings.Default;
            var scheme = ConfigurationManager.AppSettings["SubaruServerProtocol"];
            var host = ConfigurationManager.AppSettings["SubaruServerHost"];
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["SubaruServerPort"]);
            var context = ConfigurationManager.AppSettings["SubaruRepositoryName"] + ConfigurationManager.AppSettings["SubaruServerRestContext"];

            var uri = new UriBuilder(scheme, host, port, context);

            return uri.Uri.ToString();
        }


        public static String GetCmisAtomEndpoint()
        {
            //var prop = Properties.Settings.Default;
            var scheme = ConfigurationManager.AppSettings["SubaruServerProtocol"];
            var host = ConfigurationManager.AppSettings["SubaruServerHost"];
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["SubaruServerPort"]);
            var context = ConfigurationManager.AppSettings["SubaruServerCmisContext"] + ConfigurationManager.AppSettings["SubaruRepositoryName"];

            var uri = new UriBuilder(scheme, host, port, context);

            return uri.Uri.ToString();

        }

        /// <summary>
        /// Gets the base url for the nemakiware couchdb server
        /// </summary>
        /// <returns></returns>
        public String GetCouchDbBaseUrl()
        {
            //var prop = Properties.Settings.Default;
            var scheme = ConfigurationManager.AppSettings["SubaruServerProtocol"];
            var host = ConfigurationManager.AppSettings["SubaruServerHost"];
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["CouchDbServerPort"]);
            var context = ConfigurationManager.AppSettings["SubaruRepositoryName"] + "/";

            var uri = new UriBuilder(scheme, host, port, context);

            return uri.Uri.ToString();

        }
    }
}
