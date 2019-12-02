# TinyLogger

[![NuGet](https://img.shields.io/nuget/v/TinyLogger.svg?maxAge=259200)](https://www.nuget.org/packages/TinyLogger/)

Tiny extendable logger that plugins into Microsoft.Extensions.Logging

Because sometimes you don't want to use a full logging framework but still want readable console output
and log files.

## Simple example

If builder is an instance of `ILoggingBuilder` you can simply add the console logger like this:

```
builder.AddTinyConsoleLogger();
```

## More complex example

For more configuration options and file logging, use `AddTinyLogger` method instead. Checkout the sample
`ConsoleApp` for a complete example.

```
builder.AddTinyLogger(options =>
{
	// Optionally extend log fields with new or modified data
	options.Extenders.Add(new SampleExceptionExtender());

	// Select a custom message template
	options.Template = MessageTemplates.DefaultTimestamped;

	// Render to console
	options.AddConsole();

	// Render to file with rolling name, when the timestamp changes the file changes
	options.AddFile(() => $"example-{DateTime.Now.ToString("yyyyMMdd-HHmm")}.log");
});
```

## Screenshot of ConsoleApp sample

Notice the different colors for numbers, strings, dates, uris, easily readable lists and dictionaries,
and so on. Colors are selected based on the underlying data type, even in dictionaries.

![Screenshot of sample output](https://blob.steamcore.se/tinylogger/tinylogger-screenshot-20191202-2.png "Screenshot of sample output")

## Benefits and drawbacks

Because it is a logger provider like any other you are free to use other loggers as well, TinyLogger won't
get in your way. And since it is built on top of the standard logging abstractions from Microsoft there is
no magic other than adding the logging provider and the simple configuration.

For production scenarios you should probably use a more full featured logging framework since they tend to
be battle tested. TinyLogger is more intended for development scenarios and as such does not have as many
features, but that's why it's tiny.

## How it works

When a log message is received it is passed to an internal message tokenizer which parses the original
log format, extracts data from the log message state and creates a list of message tokens which easily
can be rendered by any class implementing the `ILogRenderer` interface.

The console renderer will render tokens containing object values with different colors depending on their
type so logs easier to read, while the file logger will render in plain text.

If you want one log format for the console and one log format for files, you can simply add two instances
of TinyLogger with different configuration options.

## Background workers

Everything is rendered on background threads to be as non blocking as possible.

This however means that messages are temporarily stored on a queue until they are ready to be processed
by a log renderer, which should normally be nearly instantly. But if log messages are produced faster than
can be rendered then eventually you may hit the configurable queue depth limit.

If this happens a decision has to be made whether to keep all messages in which case logging threads will
be blocked until the log renderers can catch up, or start discarding messages to give renderers more
breathing room.

The default behavior is to keep all messages.
