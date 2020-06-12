using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PromisConnector
{
    public class Promis
    {
        string access_token = null;
        public string entityId = null;
        public dynamic config = null;

        public Promis()
        {
            this.config = this.getConfig();
        }

        public dynamic getConfig()
        {
            string configpath = @"./config.json";
            if (System.Diagnostics.Debugger.IsAttached)
            {
                configpath = @"../../../config.json";
            }
            string configJson = File.ReadAllText(configpath);
            dynamic json = JObject.Parse(configJson.ToString());
            return json;
        }

        public string generatePassword(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
            return passwordHash;
        }

        public string login(string username, string password, string entityid)
        {
            string url = this.config["api_host"] + "/login";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"UserName\": \"" + username + "\",\"Password\": \"" + password + "\",\"EntityID\": \"" + entityid + "\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic o = JObject.Parse(response.Content.ToString());

            dynamic access_token = o["response"]["data"]["access_token"];
            this.access_token = access_token.Value;
            this.entityId = entityid;
            return access_token.Value;
        }

        public dynamic createUser(string username, string password, string firstname = "", string middlename = "", string lastname = "")
        {
            string xml = @"<User>
            <FirstName>" + firstname + @"</FirstName>
            <MiddleName>" + middlename + @"</MiddleName>
            <LastName>" + lastname + @"</LastName>
            <UserName>" + username + @"</UserName>
            <Password>" + password + @"</Password>
            <SystemName>Tomsoft</SystemName>
            </User>";
            string url = this.config["api_host"] + "/user";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic register(string entityName, string username, string password, string firstname = "", string middlename = "", string lastname = "")
        {
            string passwordHash = this.generatePassword(password);
            dynamic user = this.createUser(username, passwordHash, firstname, middlename, lastname);

            string xml = @"
            <Register>
                <User>
                    <UserID>" + user._id + @"</UserID>
                    <FirstName>Neil</FirstName>
                    <LastName>Rogers</LastName>
                    <DefaultRole>Subscriber</DefaultRole>
                    <Entities>
                        <Entity>
                            <EntityForeignID>YourUniqueID</EntityForeignID>
                            <OrgUser></OrgUser>
                            <EntityName>" + entityName + @"</EntityName>
                            <LegalEntityName>Testing Org1 Pty Ltd</LegalEntityName>
                            <ABN></ABN>
                            <GSTNumber></GSTNumber>
                            <BranchCode></BranchCode>
                            <EntityVerified>Yes</EntityVerified>
                            <Tax>1</Tax>
                            <HasABN>false</HasABN>
                            <Country>AU</Country>
                            <MainCurrency>AUD</MainCurrency>
                            <Phone>12345678</Phone>
                            <Mobile></Mobile>
                            <Fax></Fax>                            
                            <SystemURL></SystemURL>
                            <MultiCurrency>false</MultiCurrency>
                            <LineAmountType></LineAmountType>
                            <SystemType>" + this.config["systemtypeid"] + @"</SystemType>
                            <PostalAddress>
                                <Street>123 Test Street</Street>
                                <Suburb>Testville</Suburb>
                                <PostCode>1234</PostCode>
                                <State>TEST</State>
                                <Country>Australia</Country>
                            </PostalAddress>
                            <PhysicalAddress>
                            <Street>123 Test Street</Street>
                                <Suburb>Testville</Suburb>
                                <PostCode>1234</PostCode>
                                <State>TEST</State>
                                <Country>Australia</Country>
                            </PhysicalAddress>
                        </Entity>
                    </Entities>
                </User>
            </Register>";

            string url = this.config["api_host"] + "/register";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic getConnector()
        {
            string url = this.config["api_host"] + "/connector";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("Accept", "application/json");

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic getPayments(string querystring = "?")
        {
            string url = this.config["api_host"] + "/payments" + querystring;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("Accept", "application/json");

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic postInvoice(string xml)
        {
            string url = this.config["api_host"] + "/invoices";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic postContacts(string xml)
        {
            string url = this.config["api_host"] + "/contacts";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic postTaxCodes(string xml)
        {
            string url = this.config["api_host"] + "/taxcodes";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public dynamic postAccounts(string xml)
        {
            string url = this.config["api_host"] + "/accounts";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/xml", xml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            dynamic json = null;
            if (response.ContentLength > 0)
            {
                json = JObject.Parse(response.Content.ToString());
            }
            return json;
        }

        public IRestResponse sendPDF(string pdfPath, string invoiceid)
        {
            string url = this.config["api_host"] + "/document/attach?invoiceid=" + invoiceid;
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("token", this.access_token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("content-type", "text/plain");

            byte[] content = File.ReadAllBytes(pdfPath);
            request.AddFileBytes("pdf", content, "invoice.pdf", "application/pdf");            

            IRestResponse response = client.Execute(request);
            return response;
        }

    }
}
