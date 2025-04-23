using System.Text;
using Alba.Internal;


namespace Alba;

public sealed class ScenarioAssertionException : Exception
{
    private readonly IList<string> _messages = new List<string>();

    /// <summary>
    /// Add an assertion failure message
    /// </summary>
    /// <param name="message"></param>
    public void Add(string message)
    {
        _messages.Add(message);
    }

    private string? _body;
    
    public void AddBody(string body) => _body = body;

    internal void AssertAll()
    {
        if (_messages.Any())
        {
            throw this;
        }
    }

    /// <summary>
    /// All the assertion failure messages
    /// </summary>
    public IEnumerable<string> Messages => _messages;

    public override string Message
    {
        get
        {
            var writer = new StringBuilder();

            foreach (var message in _messages)
            {
                writer.AppendLine(message);
            }

            if (_body.IsNotEmpty())
            {
                writer.AppendLine();
                writer.AppendLine();
                writer.AppendLine("Actual body text was:");
                writer.AppendLine();
                writer.AppendLine(_body);
            }

            return writer.ToString();
        }
    }
}