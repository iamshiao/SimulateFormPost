using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimulateFormPost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            AccountInfo output = new AccountInfo();
            return View(output);
        }

        // return jsonStr if authenticate has passed
        public JsonResult JsonRes(AccountInfo input)
        {
            if (input.Account == "acc123" && input.Password == "acc123")
            {
                input.Name = "Smart Guy";
                input.Age = 18;
                input.LoginAt = DateTime.Now;

                return Json(input, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { message = "login failed!" });
        }

        // _GetWebResponse will mock a form post to action JsonRes and get jsonStr back
        public JsonResult FakePost()
        {
            // replace to ur target form parameters
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("Account", "acc123");
            nvc.Add("Password", "acc123");
            // replace to ur target action url
            string url = Url.Action("JsonRes", "Home", null, Request.Url.Scheme);
            string ret = _GetWebResponse(url, nvc);
            //var tmp = JsonConvert.DeserializeObject<AccountInfo>(ret);
            return Json(ret, JsonRequestBehavior.AllowGet);

        }        

        string _GetWebResponse(string url, NameValueCollection parameters)
        {
            // following line is to make sure when https certificate isn't valid the method will still work
            ServicePointManager.ServerCertificateValidationCallback =
                    delegate { return true; };

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";

            var sb = new StringBuilder();
            foreach (var key in parameters.AllKeys)
                sb.Append(key + "=" + parameters[key] + "&");
            sb.Length = sb.Length - 1;

            byte[] requestBytes = Encoding.UTF8.GetBytes(sb.ToString());
            httpWebRequest.ContentLength = requestBytes.Length;

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
            }

            Task<WebResponse> responseTask = Task.Factory.FromAsync<WebResponse>(
                httpWebRequest.BeginGetResponse, httpWebRequest.EndGetResponse, null);
            using (var responseStream = responseTask.Result.GetResponseStream())
            {
                var reader = new StreamReader(responseStream);
                return reader.ReadToEnd();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }        
    }

    public class AccountInfo
    {
        public string Account { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime LoginAt { get; set; }
    }
}