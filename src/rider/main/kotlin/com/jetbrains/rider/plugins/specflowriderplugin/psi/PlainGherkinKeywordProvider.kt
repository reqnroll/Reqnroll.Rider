// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.psi.tree.IElementType

class PlainGherkinKeywordProvider : GherkinKeywordProvider {
    override fun getAllKeywords(language: String?): Collection<String> {
        return DEFAULT_KEYWORDS.keys
    }

    override fun getTokenType(language: String?, keyword: String): IElementType {
        return DEFAULT_KEYWORDS[keyword]!!
    }

    override fun isSpaceRequiredAfterKeyword(language: String?, keyword: String): Boolean {
        return !ourKeywordsWithNoSpaceAfter.contains(keyword)
    }

    override fun isStepKeyword(keyword: String): Boolean {
        return DEFAULT_KEYWORDS[keyword] === GherkinTokenTypes.Companion.STEP_KEYWORD
    }

    override fun getKeywordsTable(language: String?): GherkinKeywordTable {
        return DEFAULT_KEYWORD_TABLE
    }

    companion object {
        var DEFAULT_KEYWORD_TABLE = GherkinKeywordTable()
        var DEFAULT_KEYWORDS: MutableMap<String, IElementType> = HashMap()
        private val ourKeywordsWithNoSpaceAfter: MutableSet<String?> = HashSet()

        init {
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.FEATURE_KEYWORD, "Feature")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.BACKGROUND_KEYWORD, "Background")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.SCENARIO_KEYWORD, "Scenario")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.RULE_KEYWORD, "Rule")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.SCENARIO_KEYWORD, "Example")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.SCENARIO_OUTLINE_KEYWORD, "Scenario Outline")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.EXAMPLES_KEYWORD, "Examples")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.EXAMPLES_KEYWORD, "Scenarios")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "Given")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "When")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "Then")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "And")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "But")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "*")
            DEFAULT_KEYWORD_TABLE.put(GherkinTokenTypes.Companion.STEP_KEYWORD, "Lorsqu'")
            ourKeywordsWithNoSpaceAfter.add("Lorsqu'")
            DEFAULT_KEYWORD_TABLE.putAllKeywordsInto(DEFAULT_KEYWORDS)
        }
    }
}
