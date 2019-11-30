using System;
using Microsoft.Extensions.Logging;
using TinyLogger.Tokenizers;

namespace TinyLogger
{
	[ProviderAlias("TinyLogger")]
	public sealed class TinyLoggerProvider : ILoggerProvider
	{
		private readonly IMessageTokenizer messageTokenizer;
		private readonly TinyLoggerOptions options;
		private readonly LogRendererProxy rendererProxy;

		private bool disposed;

		public TinyLoggerProvider(TinyLoggerOptions options)
			: this(new MessageTokenizer(options.Template, options.ObjectTokenizer), options)
		{
		}

		public TinyLoggerProvider(IMessageTokenizer messageTokenizer, TinyLoggerOptions options)
		{
			this.messageTokenizer = messageTokenizer;
			this.options = options;

			rendererProxy = new LogRendererProxy(options);
		}

		public void Dispose()
		{
			if (disposed)
				return;

			rendererProxy.Dispose();

			foreach (var renderer in options.Renderers)
			{
				if (renderer is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			disposed = true;
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(TinyLoggerProvider));

			return new TinyLogger(messageTokenizer, options.Extenders, rendererProxy, categoryName);
		}
	}
}
