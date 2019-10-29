//using System;
//using System.Collections.Generic;
//using System.Linq;
//using JetBrains.Application.Settings;
//using JetBrains.ReSharper.Daemon.CSharp.Stages;
//using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
//using JetBrains.ReSharper.Feature.Services.Daemon;
//using JetBrains.ReSharper.Psi;
//using JetBrains.ReSharper.Psi.Files;
//using JetBrains.ReSharper.Psi.Tree;
//
//namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
//{
//    [DaemonStage(StagesBefore = new Type[] {typeof (SmartResolverStage)})]
//    public class GherkinSyntaxHighlightingStage : IDaemonStage
//    {
//        public IEnumerable<IDaemonStageProcess> CreateProcess(
//            IDaemonProcess process,
//            IContextBoundSettingsStore settings,
//            DaemonProcessKind processKind)
//        {
//            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
//                return Enumerable.Empty<IDaemonStageProcess>();
//            
//            IPsiServices psiServices = process.SourceFile.GetPsiServices();
//            psiServices.Files.AssertAllDocumentAreCommitted((string) null);
//            ILanguageManager languageManager = psiServices.LanguageManager;
//            IReadOnlyCollection<IFile> psiFiles = psiServices.Files.GetPsiFiles<KnownLanguage>(process.SourceFile, PsiLanguageCategories.All);
//            List<IDaemonStageProcess> daemonStageProcessList = new List<IDaemonStageProcess>();
//            foreach (IFile getPrimaryPsiFile in (IEnumerable<IFile>) psiFiles)
//            {
//                SyntaxHighlightingStageProcess process1 = languageManager.TryGetService<SyntaxHighlightingManager>(getPrimaryPsiFile.Language)?.CreateProcess(process, settings, getPrimaryPsiFile);
//                if (process1 != null)
//                    daemonStageProcessList.Add((IDaemonStageProcess) process1);
//            }
//            return (IEnumerable<IDaemonStageProcess>) daemonStageProcessList;
//        }
//    }
//}