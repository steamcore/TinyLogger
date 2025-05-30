using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class LogRendererProxyTests
{
	[Test]
	[Arguments(1)]
	[Arguments(127)]
	[Arguments(7_919)]
	public void All_messages_should_be_rendered(int messageCount)
	{
		var renderer = new TestRenderer();
		var options = new TinyLoggerOptions { Renderers = [renderer] };

		RenderMessages(options, messageCount);

		renderer.Count.ShouldBe(messageCount);
	}

	[Test]
	[Arguments(1, 131)]
	[Arguments(2, 359)]
	public void All_messages_should_be_rendered_even_if_queue_depth_is_small(int queueDepth, int messageCount)
	{
		var renderer = new TestRenderer();
		var options = new TinyLoggerOptions { MaxQueueDepth = queueDepth, Renderers = [renderer] };

		RenderMessages(options, messageCount);

		renderer.Count.ShouldBe(messageCount);
	}

	[Test]
	[Arguments(3, 577)]
	[Arguments(5, 1_583)]
	[Arguments(7, 7_129)]
	public void All_messages_should_be_rendered_by_all_renderers(int rendererCount, int messageCount)
	{
		var renderers = new List<TestRenderer>();
		for (var i = 0; i < rendererCount; i++)
		{
			renderers.Add(new TestRenderer());
		}

		var options = new TinyLoggerOptions { Renderers = [.. renderers] };

		RenderMessages(options, messageCount);

		for (var i = 0; i < rendererCount; i++)
		{
			renderers[i].Count.ShouldBe(messageCount);
		}
	}

	private static void RenderMessages(TinyLoggerOptions options, int messageCount)
	{
		using var proxy = new LogRendererProxy(options);

		for (var i = 0; i < messageCount; i++)
		{
			proxy.Render(string.Empty, LogLevel.Debug, _ => { }, (_, _) => { });
		}
	}

	private sealed class TestRenderer : ILogRenderer
	{
		public int Count { get; private set; }

		public Task FlushAsync()
		{
			return Task.CompletedTask;
		}

		public Task RenderAsync(TokenizedMessage message)
		{
			Count++;
			return Task.CompletedTask;
		}
	}
}
