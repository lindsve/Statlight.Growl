using System;
using Growl.Connector;
using Growl.CoreLibrary;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;

namespace Statlight.Growl
{
    public class StatlightGrowl : ITestingReportEvents, IListener<TestRunCompletedServerEvent>
    {
        private GrowlConnector _growl;
        private int _totalNoOfTests = 0;
        private int _noOfFailedTests = 0;
        private int _noOfIgnoredTests = 0;
        private int _noOfPassedTests = 0;

        private const string AppName = "Statlight";
        private const string MessageTestsFailed = "FAILED";
        private const string MessageTestsSucceeded = "SUCCEEDED";
        private const string GeneralMessage = "STATLIGHT";

        public StatlightGrowl()
        {
            _growl = new GrowlConnector();

            var application = new Application("Statlight");

            var testCompleteWithFailedTests = new NotificationType(MessageTestsFailed, "Statlight", Properties.Resources.circleFAIL, true);
            var testCompleteWithOnlySuccessfulTests = new NotificationType(MessageTestsSucceeded, "Statlight", Properties.Resources.circleWIN, true);
            var generalNotification = new NotificationType(GeneralMessage, "Statlight", Properties.Resources.StatlightIcon, true);

            _growl.Register(application, new[] { testCompleteWithFailedTests, testCompleteWithOnlySuccessfulTests, generalNotification});
        }

        public void Handle(TestCaseResult message)
        {
            if (_totalNoOfTests == 0)
                NotifyTestRunStarted();
            _totalNoOfTests++;
            
            switch (message.ResultType)
            {
                case ResultType.Failed:
                    _noOfFailedTests++;
                    break;
                case ResultType.Ignored:
                    _noOfIgnoredTests++;
                    break;
                case ResultType.Passed:
                    _noOfPassedTests++;
                    break;
            }
        }

        private void NotifyTestRunStarted()
        {
            var notification = new Notification(AppName, GeneralMessage, null, AppName, "Test run started", null, false, Priority.Moderate, null);
            _growl.Notify(notification);
        }

        public void Handle(TraceClientEvent message) { }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message) { }

        public void Handle(FatalSilverlightExceptionServerEvent message)
        {
            var notification = new Notification(AppName, GeneralMessage, null, AppName, message.Message, null, false, Priority.Moderate, null);
            _growl.Notify(notification);
        }

        public void Handle(UnhandledExceptionClientEvent message)
        {
            var notification = new Notification(AppName, GeneralMessage, null, AppName, message.ExceptionInfo.Message, null, false, Priority.Moderate, null);
            _growl.Notify(notification);
        }
        
        public void Handle(TestRunCompletedServerEvent message)
        {
            var notificationMessage = _noOfFailedTests > 0 ? MessageTestsFailed : MessageTestsSucceeded;

            var summary = string.Format("Test run finished. {0} passed, {1} failed, {2} ignored", _noOfPassedTests, _noOfFailedTests, _noOfIgnoredTests);

            var notification = new Notification(AppName, notificationMessage, null, AppName, summary, null, false, Priority.Moderate, null);
            _growl.Notify(notification);

            _totalNoOfTests = 0;
            _noOfPassedTests = 0;
            _noOfIgnoredTests = 0;
            _noOfFailedTests = 0;
        }
    }
}