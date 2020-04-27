using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Web;

namespace isRock.Template
{
    public class LineLUISWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        const string key = "_________LUIS_KEY____________";
        const string endpoint = "______westus.api.cognitive.microsoft.com_______";
        const string appId = "______32fxxx98-b3d4-4a66-x2f9-8104dxxxxx2f_____";

        [Route("api/LineLUIS")]
        [HttpPost]
        public IActionResult POST()
        {
            var AdminUserId = "_______U5e60294b8c__AdminUserId__02d6295b621a_____";

            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = "_____________ChannelAccessToken___________________";
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                var responseMsg = "";
                //準備回覆訊息
                if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    var ret = MakeRequest(LineEvent.message.text);
                    responseMsg = $"你說了: {LineEvent.message.text}";
                    responseMsg += $"\ntopScoringIntent: {ret.topScoringIntent.intent} ";
                    foreach (var item in ret.intents)
                    {
                        responseMsg += $"\n intent: {item.intent}({item.score}) ";
                    }

                }
                else if (LineEvent.type.ToLower() == "message")
                    responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
                else
                    responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, responseMsg);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //回覆訊息
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }

        static LUISResult MakeRequest(string utterance)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["query"] = utterance;

            // These optional request parameters are set to their default values
            queryString["verbose"] = "true";
            queryString["show-all-intents"] = "true";
            queryString["staging"] = "false";
            queryString["timezoneOffset"] = "0";

            var endpointUri = String.Format(
                "https://{0}/luis/v2.0/apps/{1}?verbose=true&timezoneOffset=0&subscription-key={3}&q={2}",
                endpoint, appId, utterance, key);

            var response = client.GetAsync(endpointUri).Result;

            var strResponseContent = response.Content.ReadAsStringAsync().Result;
            var Result = Newtonsoft.Json.JsonConvert.DeserializeObject<LUISResult>(strResponseContent);
            // Display the JSON result from LUIS
            return Result;
        }
    }

    #region "LUIS Model"

    public class TopScoringIntent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Resolution
    {
        public string value { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
        public Resolution resolution { get; set; }
    }

    public class LUISResult
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Intent> intents { get; set; }
        public List<Entity> entities { get; set; }
    }
    #endregion
}