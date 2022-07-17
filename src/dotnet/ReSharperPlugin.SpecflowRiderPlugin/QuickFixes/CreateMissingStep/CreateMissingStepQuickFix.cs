using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Daemon.Errors;
using ReSharperPlugin.SpecflowRiderPlugin.Utils;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes.CreateMissingStep
{
    [QuickFix]
    public class CreateMissingStepQuickFix : IQuickFix
    {
        private readonly StepNotResolvedError _error;
        private readonly IgnoredStepNotResolvedInfo _info;

        public CreateMissingStepQuickFix(StepNotResolvedError error)
        {
            _error = error;
        }

        public CreateMissingStepQuickFix(IgnoredStepNotResolvedInfo info)
        {
            _info = info;
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            var gherkinStep = _error?.GherkinStep ?? _info?.GherkinStep;
            if (gherkinStep == null)
                return Enumerable.Empty<IntentionAction>();
            var psiServices = gherkinStep.GetPsiServices();

            return new List<IntentionAction>
            {
                new IntentionAction(new CreateSpecflowStepFromUsageAction(
                    gherkinStep.GetStepReference(),
                    psiServices.GetComponent<IMenuModalUtil>(),
                    psiServices.GetComponent<ICreateStepClassDialogUtil>(),
                    psiServices.GetComponent<ICreateStepPartialClassFile>(),
                    psiServices.GetComponent<SpecflowStepsDefinitionsCache>(),
                    psiServices.GetComponent<ICreateSpecFlowStepUtil>()
                ), BulbThemedIcons.YellowBulb.Id, IntentionsAnchors.QuickFixesAnchor)
            };
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return true;
        }
    }
}