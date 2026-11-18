using System.Threading.Tasks;
using Xunit;

namespace Kaizen.SourceGenerators.Tests;

public class ScriptSignalsGeneratorTests
{
    [Fact]
    public async Task EventSignals()
    {
        await CSharpSourceGeneratorVerifier<ScriptSignalsGenerator>.Verify(
            "EventSignals.cs",
            "EventSignals_ScriptSignals.generated.cs"
        );
    }
}
