using H1_ThirdPartyWalletAPI.Model.Config;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Utility
{
    public class SlackWebHookReq
    {
        public string Message { get; set; }
    }


    public class SlackWebHook
    {
        const string webhookUrl = "https://hooks.slack.com/services/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";

        /// <summary>
        /// 發送訊息
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="status"></param>
        /// <param name="suspendTime"></param>
        /// <param name="url"></param>
        /// <param name="DataJson"></param>
        /// <param name="message"></param>
        /// <param name="userName"></param>
        public async Task SendMessageAsync(string platform, string status, DateTime suspendTime, string url, string DataJson, string message = "", string userName = "W1 Message")
        {
            using var slackClient = new SlackClient(webhookUrl);
            try
            {
                var slackMessage = new SlackMessage
                {
                    Channel = Config.OneWalletAPI.Slack_Channel,
                    Text = message,
                    IconEmoji = Emoji.Bird,
                    Username = userName,
                    Attachments = new List<SlackAttachment>
                    {
                        new SlackAttachment
                        {
                            Text = "",
                            Color = "warning",
                            Fields = GetSlackFields(platform, status, suspendTime,url,DataJson)
                        }
                    }
                };

                await slackClient.PostAsync(slackMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private List<SlackField> GetSlackFields(string platform, string status, DateTime suspendTime, string url, string DataJson)
        {
            var fields = new List<SlackField>
            {
                new SlackField { Title = "遊戲館", Value = platform, Short = true },
                new SlackField { Title = "狀態", Value = status, Short = true },
                new SlackField { Title = "廠商URL", Value = url, Short = true },
                new SlackField { Title = "廠商requestData", Value = DataJson, Short = true },
            };

            if (!suspendTime.Equals(DateTime.MinValue))
            {
                fields.Add(new SlackField
                {
                    Title = "暫停時間",
                    Value = suspendTime.ToString(CultureInfo.InvariantCulture),
                    Short = true
                });
            }

            return fields;
        }

    }
}