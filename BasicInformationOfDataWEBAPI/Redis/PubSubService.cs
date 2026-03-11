using StackExchange.Redis;
using System;

namespace BasicInformationOfDataWEBAPI.Redis
{
    /// <summary>
    /// Redis 发布/订阅封装
    /// </summary>
    public class PubSubService
    {
        private readonly ISubscriber _subscriber;

        public PubSubService(IConnectionMultiplexer redis)
        {
            _subscriber = redis.GetSubscriber();
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        public async Task PublishAsync(string channel, string message)
        {
            await _subscriber.PublishAsync(channel, message);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        public void Subscribe(string channel, Action<string> handler)
        {
            _subscriber.Subscribe(channel, (ch, msg) => handler(msg));
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(string channel)
        {
            _subscriber.Unsubscribe(channel);
        }
    }
}