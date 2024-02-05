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
        while (newLineCharacterOffset < hostText.length && hostText[newLineCharacterOffset] != '\n') {
            newLineCharacterOffset++
        }
        if (newLineCharacterOffset >= hostText.length) {
            return
        }
        val languageMarker =
            StringUtil.trimTrailing(hostText.substring(GherkinLexer.PYSTRING_MARKER.length, newLineCharacterOffset))
        val language: Language? = InjectorUtils.getLanguageByString(languageMarker)
        if (language != null) {
            val range = TextRange.create(newLineCharacterOffset, host.textLength - GherkinLexer.PYSTRING_MARKER.length)
            if (!range.isEmpty) {
                val extensionPoint = GherkinInjectorExtensionPoint.EP_NAME.extensionList.stream().findFirst()
                val prefix = extensionPoint.map { ep: GherkinInjectorExtensionPoint -> ep.getPrefix(languageMarker) }
                    .orElse(null)
                val suffix = extensionPoint.map { ep: GherkinInjectorExtensionPoint -> ep.getSuffix(languageMarker) }
                    .orElse(null)
                registrar.startInjecting(language)
                registrar.addPlace(prefix, suffix, host, range)
                registrar.doneInjecting()
            }
        }
    }
}
