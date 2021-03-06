﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Models
{
    public class MessageBroker
    {
        private readonly IConnection _connection;
        public MessageBroker()
        {

        }
        public MessageBroker(IConnection connection)
        {
            _connection = connection;
        }

        public async Task SendMessage(string message,string queueSend)
        {          
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueSend,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                             routingKey: queueSend,
                                             basicProperties: null,
                                             body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        public async Task<string> ReceiveMessage(string queueReceive)
        {
            string receivedCoeficient = null;
            bool received = false;
            var channel = _connection.CreateModel();
            while (!received)
            {
                channel.QueueDeclare(queue: queueReceive,
                                      durable: false,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received from worker {0}", message);
                    receivedCoeficient = message;
                    received = true;
                };
                channel.BasicConsume(queue: queueReceive,
                                        autoAck: true,
                                        consumer: consumer);
            }
            channel.Close();
            return await Task.FromResult<string>(receivedCoeficient);
        }
    }
}
