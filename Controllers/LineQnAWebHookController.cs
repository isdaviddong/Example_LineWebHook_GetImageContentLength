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
    public class LineQnAWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        const string channelAccessToken = "rLAQ98kPZLNdfzV4F7gzG9475QeuBltIiOSx32lNYnbguRc/WtIc7ooHRlkPUd18oRnjncKEot7WJP+OMPFVrhjzVlI2A3d5c0Calezx6mwKZ6HoO4fI6ycP50svBZt93R11+JPGwU4ukQELBHkJEgdB04t89/1O/w1cDnyilFU=";
        const string AdminUserId = "U6ca03b0fc595c152d361896126809a5f";
        const string QnAEndpoint = "https://test20191214qna.azurewebsites.net/qnamaker/knowledgebases/f8e85867-f342-4fb6-bcb7-64b7bb4dd9b4/generateAnswer";
        const string QnAKey = "8bfe9dc0-75b0-4cec-b69d-7ef06c186a8f";
        const string UnknowAnswer = "不好意思，您可以換個方式問嗎? 我不太明白您的意思...";

        [Route("api/TestQnA")]
        [HttpPost]
        public IActionResult POST()
        {
            try
            {
                //設定ChannelAccessToken(或抓取Web.Config)
                this.ChannelAccessToken = channelAccessToken;
                //取得Line Event(範例，只取第一個)
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    if (LineEvent.message.type == "text") //收到文字
                    {
                        //建立 MsQnAMaker Client
                        var helper = new isRock.MsQnAMaker.Client(
                            new Uri(QnAEndpoint), QnAKey);
                        var QnAResponse = helper.GetResponse(LineEvent.message.text.Trim());
                        var ret = (from c in QnAResponse.answers
                                   orderby c.score descending
                                   select c
                                ).Take(1);

                        var responseText = UnknowAnswer;
                        if (ret.FirstOrDefault().score > 0)
                            responseText = ret.FirstOrDefault().answer;
                        //回覆
                        this.ReplyMessage(LineEvent.replyToken, responseText);
                    }
                    if (LineEvent.message.type == "sticker") //收到貼圖
                        this.ReplyMessage(LineEvent.replyToken, 1, 2);
                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }
}