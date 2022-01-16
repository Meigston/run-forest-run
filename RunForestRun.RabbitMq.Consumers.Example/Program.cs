namespace TestRabbitMq.Consumers
{
    using System;

    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Tests.Consumers";

            Host.CreateDefaultBuilder(args)
                .UseStartup()
                .Build()
                .Run();
        }
    }
}
