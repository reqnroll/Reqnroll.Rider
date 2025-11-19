package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.completion

import com.intellij.openapi.editor.Document
import com.intellij.openapi.util.TextRange
import com.intellij.patterns.ElementPattern
import com.intellij.patterns.PatternCondition
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiWhiteSpace
import com.intellij.psi.util.lastLeaf
import com.intellij.refactoring.suggested.extend
import com.intellij.util.ProcessingContext
import com.jetbrains.rider.completion.CustomCharPattern
import com.jetbrains.rider.completion.ICompletionHelper
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinStep

class GherkinCompletionHelper : ICompletionHelper {
    override fun getIdentifierPart(
        documentOffset: Int,
        document: Document,
        element: PsiElement,
        completionOffset: Int
    ): ElementPattern<Char>? {
        var currentOffset = completionOffset
        var psiElement = element.findElementAt(completionOffset)
        var addSpace = false;
        if (psiElement is PsiWhiteSpace) {
            if (psiElement.text.startsWith(' '))
                addSpace = true;
            if (psiElement.prevSibling.lastLeaf().parent is GherkinStep)
                psiElement = psiElement.prevSibling.lastLeaf()
            else if (psiElement.parent !is GherkinStep)
                psiElement = psiElement.prevSibling?.lastChild
            else
                psiElement = psiElement.prevSibling
        }
        if (psiElement?.parent !is GherkinStep) {
            return null
        }
        var completionTextRange = psiElement.textRange?.intersection(TextRange(0, completionOffset))
        if (addSpace && (psiElement.parent as GherkinStep).name != "")
            completionTextRange = completionTextRange?.grown(1)

        return CustomCharPattern.customCharacter().with(object : PatternCondition<Char>("gherkinIdentifierPart") {
            override fun accepts(character: Char, context: ProcessingContext): Boolean =
                completionTextRange?.containsOffset(currentOffset--) ?: false
        })
    }

    override fun getIdentifierStart(
        documentOffset: Int,
        document: Document,
        element: PsiElement,
        completionOffset: Int
    ): ElementPattern<Char>? {
        var currentOffset = completionOffset
        var psiElement = element.findElementAt(completionOffset)
        var addSpace = false;
        if (psiElement is PsiWhiteSpace) {
            if (psiElement.text.startsWith(' '))
                addSpace = true;
            if (psiElement.prevSibling.lastLeaf().parent is GherkinStep)
                psiElement = psiElement.prevSibling.lastLeaf()
            else if (psiElement.parent !is GherkinStep)
                psiElement = psiElement.prevSibling?.lastChild
            else
                psiElement = psiElement.prevSibling
        }
        if (psiElement?.parent !is GherkinStep) {
            return null
        }
        var completionTextRange = psiElement.textRange?.intersection(TextRange(0, completionOffset))
        if (addSpace && (psiElement.parent as GherkinStep).name != "")
            completionTextRange = completionTextRange?.grown(1)

        return CustomCharPattern.customCharacter().with(object : PatternCondition<Char>("gherkinIdentifierStart") {
            override fun accepts(character: Char, context: ProcessingContext): Boolean =
                completionTextRange?.containsOffset(currentOffset--) ?: false
        })
    }
}