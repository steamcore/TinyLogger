using Microsoft.Extensions.Options;

namespace TinyLogger;

[OptionsValidator]
public partial class TinyLoggerOptionsValidator : IValidateOptions<TinyLoggerOptions>
{
}
