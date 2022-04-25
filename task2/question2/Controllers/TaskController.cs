using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Question2A_ans.Models;
using System.Text;
using System.Net;

namespace Question2A_ans.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        /*
        public IActionResult Index()
        {
            return View();
        }
        */
        [HttpPost]
        public void Post([FromBody] Models.Task task)
        {
            try
            {
                // validate username and pw first
                WebRequest req = WebRequest.Create(@"https://reqres.in/api/login");
                req.Method = "POST";
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                resp.Close();
            } catch (WebException ex)
            {
                HttpContext.Response.StatusCode = 401;
            }

            var factory = new ConnectionFactory()
            {
                //HostName = "localhost" , 
                //Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "TaskQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = task.task;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "TaskQueue",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}