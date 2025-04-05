namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinLanguageComment(string lang) : GherkinElement(GherkinNodeTypes.LANGUAGE_COMMENT)
{
    public string Lang { get; } = lang;

}