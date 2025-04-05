namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinStepParameter() : GherkinElement(GherkinNodeTypes.STEP_PARAMETER)
{

    protected override string GetPresentableText()
    {
        var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.STEP_PARAMETER_TEXT);
        return textToken?.GetText();
    }

    public string GetParameterName()
    {
        var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.STEP_PARAMETER_TEXT);
        return textToken?.GetText();
    }
}