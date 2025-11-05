using System.Diagnostics.Tracing;

namespace WebAPIImport
{
    [EventSource(Name = "WebAPIImportApp", Guid = "B2345678-1234-1234-1234-123456789ABC")]
    public class WebAPIImportApp : EventSource
    {
        public static readonly WebAPIImportApp Log = new WebAPIImportApp();

        [Event(1000, Level = EventLevel.Informational, Message = "Web server start")]
        public void ProcessingStarted() => WriteEvent(1000);

        [Event(1200, Level = EventLevel.Informational, Message = "Request message send {0}")]
        public void ExecuteMethod(string metod) => WriteEvent(1200, metod);

        [Event(1300, Level = EventLevel.Error, Message = "Request message send {0}")]
        public void ErrorMethod(string metod) => WriteEvent(1300, metod);

    }
}
