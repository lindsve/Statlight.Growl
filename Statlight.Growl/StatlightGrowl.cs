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
        private readonly GrowlConnector _growl;
        private int _totalNoOfTests;
        private int _noOfFailedTests;
        private int _noOfIgnoredTests;
        private int _noOfPassedTests;

        private const string AppName = "Statlight";
        private const string FailureTitle = "FAILED";
        private const string SuccessTitle = "SUCCEEDED";
        private const string GeneralMessage = "STATLIGHT";

        public StatlightGrowl()
        {
            _growl = new GrowlConnector();

            var application = new Application("Statlight");
            var testCompleteWithFailedTests = new NotificationType(FailureTitle, "Statlight", Properties.Resources.circleFAIL, true);
            var testCompleteWithOnlySuccessfulTests = new NotificationType(SuccessTitle, "Statlight", Properties.Resources.circleWIN, true);
            var generalNotification = new NotificationType(GeneralMessage, "Statlight", Properties.Resources.StatlightIcon, true);

            _growl.Register(application, new[] { testCompleteWithFailedTests, testCompleteWithOnlySuccessfulTests, generalNotification});
        }

        public void Handle(TestCaseResult testCase)
        {
            if (_totalNoOfTests == 0)
                NotifyTestRunStarted();
            _totalNoOfTests++;

            switch (testCase.ResultType)
            {
                case ResultType.Failed:
                    _noOfFailedTests++;
                    if(_noOfFailedTests == 1)
                        NotifyCurrentStatus(testCase);
                    break;
                case ResultType.Ignored:
                    _noOfIgnoredTests++;
                    break;
                case ResultType.Passed:
                    _noOfPassedTests++;
                    break;
            }
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
            NotifyFinalStatus();
            ResetTestsCounts();
        }

        private void NotifyTestRunStarted()
        {
            var notification = new Notification(AppName, GeneralMessage, null, AppName, "Test run started", null, false, Priority.Moderate, null);
            _growl.Notify(notification);
        }

        private void NotifyCurrentStatus(TestCaseResult testCase)
        {
            var summary = GetTestCaseSummary(testCase);
            NotifyStatus(summary);
        }

        private void NotifyFinalStatus()
        {
            var summary = GetFinalSummary();
            NotifyStatus(summary);
        }

        private void NotifyStatus(string summary)
        {
            var notification = GetNotification(summary);
            _growl.Notify(notification);
        }

        private Notification GetNotification(string summary)
        {
            var notificationMessage = GetNotificationTitle();
            var priority = GetNotificationPriorityLevel();
            var notification = new Notification(AppName, notificationMessage, null, AppName, summary, null, false, priority,null);
            return notification;
        }

        private string GetFinalSummary()
        {
            return string.Format("Test run finished. {0} passed, {1} failed, {2} ignored", _noOfPassedTests, _noOfFailedTests, _noOfIgnoredTests);
        }

        private string GetTestCaseSummary(TestCaseResult testCase)
        {
            return string.Format("At least 1 test failed!. {0} - {1}",testCase.ClassName, testCase.MethodName);
        }

        private string GetNotificationTitle()
        {
            return _noOfFailedTests > 0 ? FailureTitle : SuccessTitle;
        }

        public Priority GetNotificationPriorityLevel()
        {
            Priority priority = Priority.Moderate;
            if (_totalNoOfTests == 0 || (_noOfIgnoredTests == _totalNoOfTests)) return priority;
            if (_noOfPassedTests > 0) Enum.TryParse(Properties.Resources.failpriority, true, out priority);
            else Enum.TryParse(Properties.Resources.successpriority, true, out priority);
            Console.WriteLine("DEBUG:{0}",priority);
            return priority;
        }

        private void ResetTestsCounts()
        {
            _totalNoOfTests = 0;
            _noOfPassedTests = 0;
            _noOfIgnoredTests = 0;
            _noOfFailedTests = 0;
        }
    }
}