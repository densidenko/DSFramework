using DSFramework.Extensions;
using DSFramework.Logging.Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System.IO;

namespace DSFramework.Serilog.Sink.MongoDB.Sinks
{
    public class CustomJsonFormatter : ITextFormatter
    {
        private readonly JsonValueFormatter _formatter;

        public CustomJsonFormatter()
        {
            _formatter = new JsonValueFormatter();
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            output.Write("{");
            output.Write($"\"{RequestProperties.TIMESTAMP_UTC}\":\"{logEvent.Timestamp.ToUniversalTime().DateTime:u}\"");
            output.Write($",\"{RequestProperties.DATE}\":\"{logEvent.Timestamp.ToUniversalTime().DateTime:yyyy-MM-dd}\"");
            output.Write($",\"{RequestProperties.LEVEL}\":\"{logEvent.Level.ToString()}\"");

            if (logEvent.Properties.TryGetValue(RequestProperties.EVENT_ID, out var eventId))
            {
                output.Write(",");
                JsonValueFormatter.WriteQuotedJsonString(RequestProperties.EVENT_ID, output);
                output.Write(":");
                _formatter.Format(eventId, output);
            }

            output.Write($",\"{RequestProperties.MESSAGE_TEMPLATE}\":\"{logEvent.MessageTemplate}\"");

            output.Write(",\"RenderedMessage\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            if (logEvent.Exception != null)
            {
                output.Write(",");
                JsonValueFormatter.WriteQuotedJsonString("Exception", output);
                output.Write(":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            output.Write($"\"{RequestProperties.PROPERTIES}\":{{");

            foreach (var property in logEvent.Properties)
            {
                output.Write(",");
                JsonValueFormatter.WriteQuotedJsonString(property.Key.Pascalize(), output);
                output.Write(":");
                _formatter.Format(property.Value, output);
            }
            output.Write("}}");
            output.WriteLine();
        }
    }
}