// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl

import com.intellij.extapi.psi.PsiFileBase
import com.intellij.lang.*
import com.intellij.openapi.fileTypes.*
import com.intellij.psi.*
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.*
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.i18n.JsonGherkinKeywordProvider

class GherkinFileImpl(viewProvider: FileViewProvider?) : PsiFileBase(viewProvider!!, GherkinLanguage), GherkinFile {
    override fun getFileType(): FileType {
        return GherkinFileType.Companion.INSTANCE
    }

    override fun toString(): String {
        return "GherkinFile:$name"
    }

    override val stepKeywords: List<String?>
        get() {
            val provider: GherkinKeywordProvider = JsonGherkinKeywordProvider.getKeywordProvider(this)
            val result: MutableList<String?> = ArrayList()

            // find language comment
            val language = localeLanguage

            // step keywords
            val table = provider.getKeywordsTable(language)
            result.addAll(table.stepKeywords ?: ArrayList())
            return result
        }
    override val localeLanguage: String
        get() {
            val node: ASTNode = node
            var child = node.firstChildNode
            while (child != null) {
                if (child.elementType === GherkinTokenTypes.Companion.COMMENT) {
                    val text = child.text.substring(1).trim { it <= ' ' }
                    val lang: String? = GherkinLexer.fetchLocationLanguage(text)
                    if (lang != null) {
                        return lang
                    }
                } else {
                    if (child.elementType !== TokenType.WHITE_SPACE) {
                        break
                    }
                }
                child = child.treeNext
            }
            return defaultLocale
        }
    override val features: Array<GherkinFeature>
        get() = findChildrenByClass(GherkinFeature::class.java)

    override fun findElementAt(offset: Int): PsiElement? {
        var result = super.findElementAt(offset)
        if (result == null && offset == textLength) {
            val last = lastChild
            result = if (last != null) last.lastChild else last
        }
        return result
    }

    companion object {
        val defaultLocale: String
            get() = "en"
    }
}
