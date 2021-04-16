using JetBrains.Annotations;

namespace ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput
{
    public class StepTestOutput
    {
        public enum StepStatus
        {
            Done = 0,
            Failed = 1,
            NotImplemented = 2,
            Skipped = 3
        }

        public StepStatus Status { get; set; }
        public string FirstLine { get; set; }
        [CanBeNull]
        public string MultiLineArgument { get; set; }
        [CanBeNull]
        public string Table { get; set; }

        public string ErrorOutput { get; set; }
        public string StatusLine { get; set; }
    }
}