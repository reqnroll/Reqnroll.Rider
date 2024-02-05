namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinLanguageComment : GherkinElement
    {
        public string Lang { get; }

        public GherkinLanguageComment(string lang) : base(GherkinNodeTypes.LANGUAGE_COMMENT)
        {
            Lang = lang;
        }
    }
}