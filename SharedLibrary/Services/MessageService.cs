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
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";
        static ISerializationService serializeService = SerializationService.Json;

        /// <summary>
        /// 向前台发送媒体带参消息
        /// </summary>
        /// <typeparam name="T">消息参数的类型</typeparam>
        /// <param name="type">媒体消息的类型</param>
        /// <param name="message">消息参数</param>
        public static void SendMediaMessageToForeground<T>(MediaMessageTypes type, T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, type.ToString());
            payload.Add(MessageService.MessageBody, serializeService.Serialize(message));
            BackgroundMediaPlayer.SendMessageToForeground(payload);
        }
        /// <summary>
        /// 向前台发送媒体消息
        /// </summary>
        /// <param name="type">媒体消息的类型</param>
        public static void SendMediaMessageToForeground(MediaMessageTypes type) => SendMediaMessageToForeground<string>(type, null);
        /// <summary>
        /// 向后台发送媒体带参消息
        /// </summary>
        /// <typeparam name="T">消息参数的类型</typeparam>
        /// <param name="type">媒体消息的类型</param>
        /// <param name="message">消息参数</param>
        public static void SendMediaMessageToBackground<T>(MediaMessageTypes type, T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, type.ToString());
            payload.Add(MessageService.MessageBody, serializeService.Serialize(message));
            BackgroundMediaPlayer.SendMessageToBackground(payload);
        }
        /// <summary>
        /// 向后台发送媒体消息
        /// </summary>
        /// <param name="type">媒体消息的类型</param>
        public static void SendMediaMessageToBackground(MediaMessageTypes type) => SendMediaMessageToBackground<string>(type, null);

        /// <summary>
        /// 获取媒体消息的类型
        /// </summary>
        /// <param name="data">存储在<see cref="MediaPlayerDataReceivedEventArgs"/>中的<see cref="ValueSet"/>数据</param>
        public static MediaMessageTypes GetTypeOfMediaMessage(ValueSet data) =>
            (MediaMessageTypes)Enum.Parse(typeof(MediaMessageTypes), data[MessageType].ToString());
        /// <summary>
        /// 获取媒体消息的参数（即消息主体）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="data">存储在<see cref="MediaPlayerDataReceivedEventArgs"/>中的<see cref="ValueSet"/>数据</param>
        public static T GetMediaMessage<T>(ValueSet data) =>
            serializeService.Deserialize<T>(data[MessageBody].ToString());
    }
}
