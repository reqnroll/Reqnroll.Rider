using JetBrains.Application.Parts;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Settings;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders
{
    [SolutionComponent(Instantiation.DemandAnyThreadUnsafe)]
    public class GherkinReferenceAutomaticCompletionStrategy : IAutomaticCodeCompletionStrategy
    {
        public AutopopupType IsEnabledInSettings(IContextBoundSettingsStore settingsStore, ITextControl textControl)
        {
            return AutopopupType.SoftAutopopup;
        }

        public bool AcceptTyping(char c, ITextControl textControl, IContextBoundSettingsStore boundSettingsStore)
        {
            return true;
        }

        public bool ProcessSubsequentTyping(char c, ITextControl textControl)
        {
            return true;
        }

        public bool AcceptsFile(IFile file, ITextControl textControl)
        {
            return file is GherkinFile;
        }

        public PsiLanguageType Language => GherkinLanguage.Instance.NotNull();
        public bool ForceHideCompletion => false;
    }
}