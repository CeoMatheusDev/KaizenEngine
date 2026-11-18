namespace Kaizen.SourceGenerators.Sample;

public partial class EventSignals : KaizenObject
{
    [Signal]
    public delegate void MySignalEventHandler(string str, int num);
}
