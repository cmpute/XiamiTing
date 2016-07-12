using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Template10.Services.SerializationService;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 辅助前后台消息传送的服务类
    /// </summary>
    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";
        static ISerializationService serializeService = SerializationService.Json;

        public static void SendMediaMessageToForeground<T>(MediaMessageTypes type, T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, type.ToString());
            payload.Add(MessageService.MessageBody, serializeService.Serialize(message));
            BackgroundMediaPlayer.SendMessageToForeground(payload);
        }
        public static void SendMediaMessageToForeground(MediaMessageTypes type)
        {
            SendMediaMessageToForeground<string>(type, null);
        }

        public static void SendMediaMessageToBackground<T>(MediaMessageTypes type, T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, type.ToString());
            payload.Add(MessageService.MessageBody, serializeService.Serialize(message));
            BackgroundMediaPlayer.SendMessageToBackground(payload);
        }
        public static void SendMediaMessageToBackground(MediaMessageTypes type)
        {
            SendMediaMessageToBackground<string>(type, null);
        }

        public static MediaMessageTypes GetTypeOfMessage(ValueSet data)
        {
            return (MediaMessageTypes)Enum.Parse(typeof(MediaMessageTypes), data[MessageType].ToString());
        }

        public static T GetMessage<T>(ValueSet data)
        {
            return serializeService.Deserialize<T>(data[MessageBody].ToString());
            
        }
    }
}
