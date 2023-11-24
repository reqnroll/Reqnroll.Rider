// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.lexer.Lexer
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.fileTypes.SyntaxHighlighterBase
import com.intellij.psi.tree.IElementType
import java.util.*

class GherkinSyntaxHighlighter(private val myKeywordProvider: GherkinKeywordProvider) : SyntaxHighlighterBase() {
    override fun getHighlightingLexer(): Lexer {
        return GherkinLexer(myKeywordProvider)
    }

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> {
        return pack(ATTRIBUTES[tokenType])
    }

    companion object {
        private val ATTRIBUTES: MutableMap<IElementType, TextAttributesKey?> = HashMap()

        init {
            Arrays.stream<IElementType>(GherkinTokenTypes.Companion.KEYWORDS.getTypes())
                .forEach { p: IElementType -> ATTRIBUTES[p] = GherkinHighlighter.KEYWORD }
            ATTRIBUTES[GherkinTokenTypes.Companion.COMMENT] = GherkinHighlighter.COMMENT
            ATTRIBUTES[GherkinTokenTypes.Companion.TEXT] = GherkinHighlighter.TEXT
            ATTRIBUTES[GherkinTokenTypes.Companion.TAG] = GherkinHighlighter.TAG
            ATTRIBUTES[GherkinTokenTypes.Companion.PYSTRING] = GherkinHighlighter.PYSTRING
            ATTRIBUTES[GherkinTokenTypes.Companion.PYSTRING_TEXT] = GherkinHighlighter.PYSTRING
            ATTRIBUTES[GherkinTokenTypes.Companion.TABLE_CELL] = GherkinHighlighter.TABLE_CELL
            ATTRIBUTES[GherkinTokenTypes.Companion.PIPE] = GherkinHighlighter.PIPE
        }
    }
}
