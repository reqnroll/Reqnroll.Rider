package com.jetbrains.rider.plugins.reqnrollriderplugin.completion

import com.intellij.codeInsight.completion.CompletionType
import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiFile

class GherkinCompletionStrategy : com.jetbrains.rider.completion.CompletionSessionStrategy {
    override fun shouldForbidCompletion(editor: Editor, type: CompletionType) = editor.selectionModel.hasSelection()
    override fun shouldRescheduleCompletion(prefix: String, psiFile: PsiFile, char: Char?, offset: Int) =
        prefix.isEmpty()
}
