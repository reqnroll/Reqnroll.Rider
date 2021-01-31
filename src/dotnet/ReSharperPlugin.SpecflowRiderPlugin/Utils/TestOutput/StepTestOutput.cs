using JetBrains.Annotations;

namespace ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput
{
    public class StepTestOutput
    {
        public string Status { get; set; }
        public string FirstLine { get; set; }
        [CanBeNull]
        public string MultiLineArgument { get; set; }
        [CanBeNull]
        public string Table { get; set; }

        public string ErrorOutput { get; set; }
        public string StatusLine { get; set; }
    }
}