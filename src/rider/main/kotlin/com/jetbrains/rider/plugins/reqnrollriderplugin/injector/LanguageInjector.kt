package com.jetbrains.rider.plugins.reqnrollriderplugin.injector

import com.intellij.lang.Language
import com.intellij.lang.injection.MultiHostInjector
import com.intellij.lang.injection.MultiHostRegistrar
import com.intellij.openapi.util.TextRange
import com.intellij.openapi.util.text.StringUtil
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiLanguageInjectionHost
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinLexer
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinPystring
import org.intellij.plugins.intelliLang.inject.InjectorUtils

class LanguageInjector : MultiHostInjector {
    override fun elementsToInjectIn(): List<Class<out PsiElement?>?> {
        return listOf(GherkinPystring::class.java)
    }

    override fun getLanguagesToInject(registrar: MultiHostRegistrar, context: PsiElement) {
        if (context !is GherkinPystring) {
            return
        }
        val host = context as PsiLanguageInjectionHost
        val hostText = host.text
        var newLineCharacterOffset = 0
        val newLineOffsets = ArrayList<Int>()
        while (newLineCharacterOffset < hostText.length) {
            if (hostText[newLineCharacterOffset] == '\n')
                newLineOffsets.add(newLineCharacterOffset)
            newLineCharacterOffset++
        }
        if (newLineOffsets.count() < 2) {
            return
        }
        val firstNewLineOffset = newLineOffsets[0];
        val languageMarker =
            StringUtil.trimTrailing(hostText.substring(GherkinLexer.PYSTRING_MARKER.length, firstNewLineOffset))
        val language: Language? = InjectorUtils.getLanguageByString(languageMarker)
        val indent = context.prevSibling.text.trim('\n', '\r').length;
        if (language != null) {
            val range = TextRange.create(firstNewLineOffset, host.textLength - GherkinLexer.PYSTRING_MARKER.length)
            if (!range.isEmpty) {
                registrar.startInjecting(language)
                for (i in 0 until newLineOffsets.size - 1) {
                    val startLineOffset = newLineOffsets[i] + indent + 1;
                    val endLineOffset = newLineOffsets[i + 1];
                    if (startLineOffset < endLineOffset) {
                        val lineRange = TextRange.create(startLineOffset, endLineOffset)
                        registrar.addPlace(null, null, host, lineRange)
                    }
                }
                registrar.doneInjecting()
            }
        }
    }
}
