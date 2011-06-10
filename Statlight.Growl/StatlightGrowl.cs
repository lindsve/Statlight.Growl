using System;
using Growl.Connector;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;

namespace Statlight.Growl
{
    public class StatlightGrowl : ITestingReportEvents
    {
        private GrowlConnector _growl;

        private const string appName = "Statlight";
        private const string messageTestFailed = "FAILED";

        public StatlightGrowl()
        {
            _growl = new GrowlConnector();

            var application = new Application("Statlight");
            var testComplete = new NotificationType("FAILED", "Test failed");
            _growl.Register(application, new[] { testComplete});
        }

        public void Handle(TestCaseResult message)
        {
            if (message.ResultType == ResultType.Failed)
            {
                var notification = new Notification(appName, messageTestFailed, null, message.ClassName, message.MethodName, null, true, Priority.Moderate, null);
                _growl.Notify(notification);
            }
        }

        public void Handle(TraceClientEvent message) { }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message) { }

        public void Handle(FatalSilverlightExceptionServerEvent message) { }

        public void Handle(UnhandledExceptionClientEvent message) { }
    }
}