using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveMq
{
    public class MqHelper
    {
        //protected static AutoResetEvent semaphore = new AutoResetEvent(false);
        //protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
        private static IConnectionFactory factory;
        private static IConnection connection;
        private static ISession session;
        private static readonly string username = "你的帐户名";
        private static readonly string passwd = "你的密码";
        private static readonly string brokerURL = "tcp://你的地址";

        static MqHelper()
        {
            try
            {
                //初始化工厂
                factory = new ConnectionFactory(brokerURL);
                connection = factory.CreateConnection(username, passwd);
                //连接服务器端的标识
                connection.ClientId = "NetQueueListener";
                //启动连接
                connection.Start();
                //通过连接创建对话
                session = connection.CreateSession();
            }
            catch
            {
                throw new ApplicationException("初始化失败");
            }
        }

        /// <summary>
        /// 生产消息到消息队列
        /// </summary>
        /// <param name="name">队列名</param>
        /// <param name="msg">消息内容</param>
        public static void ProduceMessageToQueue(string name, string msg, string fillterKey = null, string fillterValue = null)
        {
            Console.WriteLine(session.AcknowledgementMode.ToString());//消息确认机制,默认是
                                                                      //通过会话创建生产者，方法里new出来MQ的Queue
            IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(name));
            //创建一个发送消息的对象
            ITextMessage message = prod.CreateTextMessage();
            message.Text = msg; //给这个消息对象赋实际的消息
                                //设置消息对象的属性，是Queue的过滤条件也是P2P的唯一指定属性
            if (!string.IsNullOrEmpty(fillterKey) && !string.IsNullOrEmpty(fillterValue))
            {
                message.Properties.SetString(fillterKey, fillterValue);
            }
            ///配置消息的持久性，优先级和
            prod.Send(message, MsgDeliveryMode.NonPersistent, MsgPriority.Normal, TimeSpan.MinValue);
            Console.WriteLine("发送成功！");
        }
        /// <summary>
        /// 消费消息队列的消息
        /// </summary>
        /// <param name="name">队列名</param>
        /// <param name="waitTime">消费消息等待时间</param>
        /// <param name="selector">选择器（"filter = 'demo'"）</param>"
        public static void ConsumeMessageFromQueue(string name, int waitTime = 20, string selector = null)
        {
            //通过会话创建一个消费者
            IMessageConsumer consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(name), selector);

            //注册监听事件
            consumer.Listener += new MessageListener(consumer_Listener);
        }
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="name">Topic名称</param>
        /// <param name="msg">消息内容</param>
        public static void PublishMessageToTopic(string name, string msg)
        {
            IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(name));
            //创建一个发送消息的对象
            ITextMessage message = prod.CreateTextMessage();
            message.Text = msg; //给这个消息对象赋实际的消息
                                //设置消息对象的属性，是Queue的过滤条件也是P2P的唯一指定属性
            Console.WriteLine(message.NMSDeliveryMode.ToString());
            ///配置消息是持久化传输或者非持久化传输（持久话传输会先存到硬盘，宕机后还可恢复）优先级
            prod.Send(message, MsgDeliveryMode.NonPersistent, MsgPriority.Normal, TimeSpan.MinValue);
            Console.WriteLine("发送成功！");
        }
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="name">订阅的Topic名称</param>
        /// <param name="waitTime">消费消息等待时间</param>
        public static void SubscribeMessageFromTopic(string name, int waitTime = 20)
        {
            //通过会话创建一个持久化消费者
            IMessageConsumer consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(name), "NetTopicListener", null, false);

            /*同步接收，receive之后，消息就从队列中删掉
            IMessage message = consumer.Receive(TimeSpan.FromSeconds(waitTime));
            ITextMessage msg = message as ITextMessage;
            if (msg == null)
            {
                Console.WriteLine(string.Format("队列中无消息"));
            }
            else
            {
                Console.WriteLine(string.Format(@"接收到:{0}{1}", msg.Text, Environment.NewLine));
            }
            */

            //注册监听事件
            consumer.Listener += new MessageListener(consumer_Listener);
        }

        private static void consumer_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            Console.WriteLine(string.Format(@"接收到:{0}{1}", msg.Text, Environment.NewLine));
        }
    }
}
