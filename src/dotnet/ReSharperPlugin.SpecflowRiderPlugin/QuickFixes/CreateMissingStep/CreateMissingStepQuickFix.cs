using System.Collections.Generic;
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

        public CreateMissingStepQuickFix(StepNotResolvedError error)
        {
            _error = error;
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            var psiServices = _error.GherkinStep.GetPsiServices();

            return new List<IntentionAction>
            {
                new IntentionAction(new CreateSpecflowStepFromUsageAction(
                    _error.GherkinStep.GetStepReference(),
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