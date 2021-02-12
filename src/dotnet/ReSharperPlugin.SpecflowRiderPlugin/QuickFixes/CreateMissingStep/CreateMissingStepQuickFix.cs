using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Daemon.UnresolvedReferenceHighlight;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    [QuickFix]
    public class CreateMissingStepQuickFix : IQuickFix
    {
        private readonly NotResolvedError _error;

        public CreateMissingStepQuickFix(NotResolvedError error)
        {
            _error = error;
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            return new List<IntentionAction>
            {
                new IntentionAction(new CreateSpecflowStepFromUsageAction(_error.GherkinStep.GetStepReference(), _error.GherkinStep.GetPsiServices().GetComponent<IStepDefinitionBuilder>()), BulbThemedIcons.RedBulb.Id, IntentionsAnchors.QuickFixesAnchor)
            };
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return true;
        }
    }
}