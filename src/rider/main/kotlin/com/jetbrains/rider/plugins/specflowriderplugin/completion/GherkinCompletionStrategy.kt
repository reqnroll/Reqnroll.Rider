package com.jetbrains.rider.plugins.specflowriderplugin.completion

import com.intellij.codeInsight.completion.CompletionType
import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiFile
import com.jetbrains.rdclient.completion.ICompletionSessionStrategy

class GherkinCompletionStrategy : ICompletionSessionStrategy {
    override fun shouldForbidCompletion(editor: Editor, type: CompletionType) = editor.selectionModel.hasSelection()
    override fun shouldRescheduleCompletion(prefix: String, psiFile: PsiFile, char: Char?, offset: Int) =
        prefix.isEmpty()
}
