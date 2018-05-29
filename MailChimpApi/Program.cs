using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MailChimp.Api.Net.Enum;
using MailChimp.Api.Net.Domain.Automations;
using MailChimp.Api.Net.Domain.Reports;
using MailChimp.Api.Net.Domain.Lists;

namespace MailChimpApi
{
    //This is the change made by second branch
    // cchange commit intro Master Branch

        // My Changes into services
        // Latestest senocd change

    static class Program
    {
        public static string apiKey = "d624ba77ca413588fa58a5b4c0173aed-us18"; //your API KEY created by you.
        public static string dataCenter = "us18";
        public static string listId = "f56eb5b4a1"; // your list Id

        static void Main(string[] args)
        {
            //  getListMemmber();
            //ashghsdf sdfhsd

            DataTable dt = GetUserFromDB();
            //   UpdateBulkUsers(dt);
            CreateCampaign();

        }
        public static void subscribeAddress(DataTable dtRes)
        {

            foreach (DataRow dr in dtRes.Rows)
            {
                var hashedEmailAddress = string.IsNullOrEmpty(dr["Email"].ToString()) ? "" : CalculateMD5Hash(dr["Email"].ToString().ToLower());
                SubscribeClassCreatedByMe subscribeRequest = new SubscribeClassCreatedByMe
                {
                    email_address = dr["Email"].ToString(),
                    status = (Convert.ToBoolean(dr["Status"]) ? SubscriberStatus.subscribed.ToString() : SubscriberStatus.unsubscribed.ToString())
                };
                subscribeRequest.merge_fields = new MergeFieldClassCreatedByMe();
                subscribeRequest.merge_fields.FNAME = dr["FName"].ToString();
                subscribeRequest.merge_fields.LNAME = dr["LName"].ToString();
                //     subscribeRequest.merge_fields.ADDRESS = dr["ADDRESS"].ToString();
                var uri = string.Format("https://{0}.api.mailchimp.com/3.0/lists/{1}/members/{2}", dataCenter, listId, hashedEmailAddress);
                var sampleListMember = JsonConvert.SerializeObject(subscribeRequest);
                var returnRes = "";
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Accept", "application/json");
                        webClient.Headers.Add("Authorization", "apikey " + apiKey);

                        returnRes = webClient.UploadString(uri, "PUT", sampleListMember);
                    }
                }
                catch (WebException we)
                {
                    using (var sr = new StreamReader(we.Response.GetResponseStream()))
                    {
                        returnRes = sr.ReadToEnd();
                    }
                }
                Console.WriteLine("User Data :" + subscribeRequest.email_address + ", Status :" + subscribeRequest.status.ToString());

            }
            // Console.ReadKey();
        }

        private static string CalculateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input.
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string.
            var sb = new StringBuilder();
            foreach (var @byte in hash)
            {
                sb.Append(@byte.ToString("X2"));
            }
            return sb.ToString();
        }

        public static DataTable GetUserFromDB()
        {
            DataTable dtRes = new DataTable();
            using (SqlConnection connection = new SqlConnection("Server=slinfy\\sqlexpress;Initial Catalog=MailChimpDB;User ID=sa;Password=123;"))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM tbl_User", connection))
                {
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    adp.Fill(dtRes);
                    return dtRes;
                }
            }
        }

        public static void getListMemmber() {

            var uri = string.Format("https://{0}.api.mailchimp.com/3.0/lists/{1}/members", dataCenter, listId);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + apiKey);
                    var outputRes = webClient.DownloadString(uri);
                    RootMember instance = JsonConvert.DeserializeObject<RootMember>(outputRes);
                    foreach (MCMember itemMember in instance.members)
                    {
                        Console.WriteLine("Email: " + itemMember.email_address + ", Status: " + itemMember.status);
                        DataTable dt = GetUserByEmail(itemMember.email_address);
                        if (dt.Rows.Count > 0)
                        {
                            if (itemMember.status != (Convert.ToBoolean(dt.Rows[0]["Status"]) ? SubscriberStatus.subscribed.ToString() : SubscriberStatus.unsubscribed.ToString()))
                            { subscribeAddress(dt); }
                        }
                    }
                }
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    Console.WriteLine("Error Message" + sr.ReadToEnd());
                }
            }
        }

        public static DataTable GetUserByEmail(string Email)
        {
            DataTable dtRes = new DataTable();
            using (SqlConnection connection = new SqlConnection("Server=slinfy\\sqlexpress;Initial Catalog=MailChimpDB;User ID=sa;Password=123;"))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM tbl_User where Email='" +
                     Email + "'", connection))
                {
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    adp.Fill(dtRes);
                    return dtRes;
                }
            }
        }
        public static void UpdateBulkUsers(DataTable dtRes)
        {
            List<Batch> lstBatch = new List<Batch>();

            foreach (DataRow dr in dtRes.Rows)
            {
                var status = (Convert.ToBoolean(dr["Status"]) ? "subscribed" : "unsubscribed");
                Batch objBatch = new Batch();
                 var hashedEmailAddress = string.IsNullOrEmpty(dr["Email"].ToString()) ? "" : CalculateMD5Hash(dr["Email"].ToString().ToLower());
                objBatch.method = "PUT";
                objBatch.path = "lists/" + listId + "/members/" + hashedEmailAddress;
                objBatch.body = "{\"email_address\":\""+ dr["Email"].ToString() + "\", \"status\":\""+ status + "\",\"merge_fields\":{\"FNAME\":\""+ dr["FName"].ToString() + "\",\"LNAME\":\"" + dr["LName"].ToString() + "\",\"PHONE\":\"8558874911\",\"BIRTHDAY\":\"12/11\",\"HADDRESS\":\""+dr["Address"]+ "\",\"ZIPCODE\":\"10001\",\"USERDATE\":\""+DateTime.Now.ToString("MM/dd/yyyy")+ "\",\"WEBURL\":\"http://igofx.com/ \",\"WEBLOGO\":\"http://igofx.com/wp-content/uploads/2016/08/logo-1.png \",\"ACTIVE\":\"True\"}}";
                lstBatch.Add(objBatch);
            }
            
            var uri = string.Format("https://{0}.api.mailchimp.com/3.0/batches", dataCenter);
            var sampleListMember = JsonConvert.SerializeObject(lstBatch);
            var returnRes = "";
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + apiKey);

                    returnRes = webClient.UploadString(uri, "POST", "{\"operations\":"+ sampleListMember+"}");
                }
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    returnRes = sr.ReadToEnd();
                }
            }          
            GetResponseBulkUsers((returnRes.ToString().Split('\"'))[3]);

        }
        public static void GetResponseBulkUsers(string bulkId)
        {

            var uri = string.Format("https://{0}.api.mailchimp.com/3.0/batches/{1}", dataCenter, bulkId);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + apiKey);
                    var outputRes = webClient.DownloadString(uri);
                    Console.WriteLine("Status :" + (outputRes.ToString().Split('\"'))[7]);
                    Console.WriteLine("Total Operations :" + (outputRes.ToString().Split('\"'))[10].Split(':')[1]);
                    Console.WriteLine("Finished Operations :" + ((outputRes.ToString().Split('\"')))[12].Split(':')[1]);
                    Console.WriteLine("Error Operations :" + (outputRes.ToString().Split('\"'))[14].Split(':')[1]);
                    Console.ReadKey();
                }
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    Console.WriteLine("Error Message" + sr.ReadToEnd());
                }
            }
           
        }
        public static void CreateCampaign()
        {
            var uri = string.Format("https://{0}.api.mailchimp.com/3.0/campaigns", dataCenter);
            var sampleListMember = "{\"type\":\"regular\",\"content_type\": \"template\",\"recipients\":{\"list_id\":\"" + listId+ "\"},\"settings\": {\"subject_line\": \"Test from code\",\"title\":\"New from code\", \"from_name\": \"Pargat\", \"reply_to\": \"pargat.singh@slinfy.com\"}}";
            var returnRes = "";
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + apiKey);
                    returnRes = webClient.UploadString(uri, "POST", sampleListMember );

                }
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    returnRes = sr.ReadToEnd();
                }
            }
            Console.WriteLine("Campaign Added successfully");
            SendMail(returnRes.ToString().Split('\"')[3]);
            
        }
        public static void SendMail(string campaignId)
        {
            var uri = string.Format("https://{0}.api.mailchimp.com/3.0//campaigns/{1}/actions/send", dataCenter, campaignId);           
            var returnRes = "";
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + apiKey);
                    returnRes = webClient.UploadString(uri, "GET");

                }
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    returnRes = sr.ReadToEnd();
                }
            }
        }
    }
    public class Batch
    {
        public string method { get; set; }
        public string path { get; set; }
        public string body { get; set; }
    }
}
