using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TinyLogger.Tokenizers;

namespace TinyLogger
{
	[ProviderAlias("TinyLogger")]
	public sealed class TinyLoggerProvider : ILoggerProvider
	{
		private readonly IMessageTokenizer messageTokenizer;
		private readonly IOptions<TinyLoggerOptions> options;
		private readonly LogRendererProxy rendererProxy;

		private bool disposed;

		public TinyLoggerProvider(IOptions<TinyLoggerOptions> options)
			: this(new MessageTokenizer(options), options)
		{
		}

		public TinyLoggerProvider(IMessageTokenizer messageTokenizer, IOptions<TinyLoggerOptions> options)
		{
			this.messageTokenizer = messageTokenizer ?? throw new ArgumentNullException(nameof(messageTokenizer));
			this.options = options ?? throw new ArgumentNullException(nameof(options));

			rendererProxy = new LogRendererProxy(options.Value);
		}

		public void Dispose()
		{
			if (disposed)
				return;

			rendererProxy.Dispose();

			foreach (var renderer in options.Value.Renderers)
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

			return new TinyLogger(messageTokenizer, options.Value.Extenders, rendererProxy, categoryName);
		}
	}
}
