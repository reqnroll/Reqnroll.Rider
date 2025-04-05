using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Daemon.Errors;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes.CreateMissingStep;

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
            new IntentionAction(new CreateReqnrollStepFromUsageAction(
                gherkinStep.GetStepReference(),
                psiServices.GetComponent<IMenuModalUtil>(),
                psiServices.GetComponent<ICreateStepClassDialogUtil>(),
                psiServices.GetComponent<ICreateStepPartialClassFile>(),
                psiServices.GetComponent<ReqnrollStepsDefinitionsCache>(),
                psiServices.GetComponent<ICreateReqnrollStepUtil>()
            ), BulbThemedIcons.YellowBulb.Id, IntentionsAnchors.QuickFixesAnchor)
        };
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
        return true;
    }
}