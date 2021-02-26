using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Settings;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    [SolutionComponent]
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

        public PsiLanguageType Language => GherkinLanguage.Instance;
        public bool ForceHideCompletion => false;
    }
}