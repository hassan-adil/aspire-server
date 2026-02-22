using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Application.Consumers;

// Minimal test consumer
public record TestMessage(string Text);

public class TestMessageConsumer : IConsumer<TestMessage>
{
    public Task Consume(ConsumeContext<TestMessage> context)
    {
        Console.WriteLine($"Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}