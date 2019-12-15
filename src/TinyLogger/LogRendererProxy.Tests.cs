using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TinyLogger
{
	public class LogRendererProxyTests
	{
		[Theory]
		[InlineData(1)]
		[InlineData(127)]
		[InlineData(7_919)]
		public async Task All_messages_should_be_rendered(int messageCount)
		{
			var renderer = new TestRenderer();
			var options = new TinyLoggerOptions { Renderers = new List<ILogRenderer> { renderer } };

			await RenderMessages(options, messageCount);

			Assert.Equal(messageCount, renderer.Count);
		}

		[Theory]
		[InlineData(1, 131)]
		[InlineData(2, 359)]
		public async Task All_messages_should_be_rendered_even_if_queue_depth_is_small(int queueDepth, int messageCount)
		{
			var renderer = new TestRenderer();
			var options = new TinyLoggerOptions { MaxQueueDepth = queueDepth, Renderers = new List<ILogRenderer> { renderer } };

			await RenderMessages(options, messageCount);

			Assert.Equal(messageCount, renderer.Count);
		}

		[Theory]
		[InlineData(3, 577)]
		[InlineData(5, 1_583)]
		[InlineData(7, 7_129)]
		public async Task All_messages_should_be_rendered_by_all_renderers(int rendererCount, int messageCount)
		{
			var renderers = new List<TestRenderer>();
			for (var i = 0; i < rendererCount; i++)
			{
				renderers.Add(new TestRenderer());
			}

			var options = new TinyLoggerOptions { Renderers = renderers.ToList<ILogRenderer>() };

			await RenderMessages(options, messageCount);

			for (var i = 0; i < rendererCount; i++)
			{
				Assert.Equal(messageCount, renderers[i].Count);
			}
		}

		private static async Task RenderMessages(TinyLoggerOptions options, int messageCount)
		{
			var message = new TokenizedMessage(string.Empty, LogLevel.Debug, () => Array.Empty<MessageToken>());

			using (var proxy = new LogRendererProxy(options))
			{
				for (var i = 0; i < messageCount; i++)
				{
					await proxy.Render(message);
				}
			}
		}

		private class TestRenderer : ILogRenderer
		{
			public int Count { get; private set; }

			public Task Render(TokenizedMessage message)
			{
				Count++;
				return Task.CompletedTask;
			}
		}
	}
}
