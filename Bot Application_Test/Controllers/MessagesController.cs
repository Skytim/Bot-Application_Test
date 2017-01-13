using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Bot_Application_Test.Models;
using System.Configuration;
using System.Web;
using System.IO;

namespace Bot_Application_Test
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var strLuisKey = ConfigurationManager.AppSettings["LUISAPIKey"].ToString();
                var strLuisAppId = ConfigurationManager.AppSettings["LUISAppId"].ToString();
                var strMessage = HttpUtility.UrlEncode(activity.Text);
                var strLuisUrl = $"https://api.projectoxford.ai/luis/v2.0/apps/{strLuisAppId}?subscription-key={strLuisKey}&q={strMessage}";

                // 收到文字訊息後，往LUIS送
                var request = WebRequest.Create(strLuisUrl);
                var responses = (HttpWebResponse)request.GetResponse();
                var dataStream = responses.GetResponseStream();
                using (var reader = new StreamReader(dataStream))
                {
                    var json = reader.ReadToEnd();
                    var objLUISRes = JsonConvert.DeserializeObject<LUISResult>(json);

                    string strReply = "無法識別的內容";


                    string strIntent = objLUISRes.topScoringIntent.intent;
                    if (strIntent == "詢問")
                    {
                        string strDate = objLUISRes.entities.Find((x => x.type == "日期")).entity;
                        string strAir = objLUISRes.entities.Find((x => x.type == "航空公司")).entity;
                        string strService = objLUISRes.entities.Find((x => x.type == "服務")).entity;

                        strReply = $"您要詢問的航空公司:{strAir}，日期:{strDate}，相關服務是:{strService}。我馬上幫您找出資訊";
                        strReply += ".....這裡加上後續資料的呈現.....";
                    }

                    if (strIntent == "只是打招呼")
                    {
                        strReply = "您好，有什麼能幫得上忙的呢?";
                    }

                    if (strIntent == "None")
                    {
                        strReply = "您在說什麼，我聽不懂~~~(轉圈圈";
                    }


                    Activity reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}