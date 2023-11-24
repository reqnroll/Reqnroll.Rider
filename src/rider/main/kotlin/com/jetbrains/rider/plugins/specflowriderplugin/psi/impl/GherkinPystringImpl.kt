package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.psi.LiteralTextEscaper
import com.intellij.psi.PsiLanguageInjectionHost
import com.intellij.psi.impl.source.tree.LeafPsiElement
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinPystring

class GherkinPystringImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinPystring {
    override fun toString(): String {
        return "GherkinPystring"
    }

    override fun isValidHost(): Boolean {
        return true
    }

    override fun updateText(text: String): PsiLanguageInjectionHost {
        val docStringSep = firstChild.text
        val startOffset = if (text.startsWith(docStringSep)) docStringSep.length else 0
        val endOffset = if (text.endsWith(docStringSep)) docStringSep.length else 0
        (firstChild.nextSibling as LeafPsiElement).replaceWithText(text.substring(startOffset, text.length - endOffset))
        firstChild.nextSibling.nextSibling.replace(lastChild)
        return this
    }

    override fun createLiteralTextEscaper(): LiteralTextEscaper<out PsiLanguageInjectionHost> {
        return LiteralTextEscaper.createSimple(this)
    }
}
