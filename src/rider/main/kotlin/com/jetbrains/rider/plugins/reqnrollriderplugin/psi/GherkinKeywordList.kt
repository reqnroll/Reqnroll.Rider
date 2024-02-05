// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.psi.tree.IElementType
import com.intellij.util.ArrayUtil

class GherkinKeywordList {
    // maps custom language keyword to base (English) keyword
    private val myKeyword2BaseNameTable: MutableMap<String, String> = HashMap()
    private val mySpaceAfterKeywords: MutableSet<String> = HashSet()
    val keywordsTable = GherkinKeywordTable()

    constructor()
    constructor(map: Map<String, Any?>) {
        for ((key, value) in map) {
            if (!GHERKIN_LANGUAGE_META_ATTRIBUTES.contains(key)) {
                val values = value as List<String>
                val translatedKeywords = ArrayUtil.toStringArray(values)
                val keyword = capitalizeAndFixSpace(key)
                val type = getTokenTypeByBaseKeyword(keyword)
                for (translatedKeyword in translatedKeywords) {
                    if (translatedKeyword.endsWith(" ")) {
                        val translatedKeyword2 = translatedKeyword.substring(0, translatedKeyword.length - 1)
                        mySpaceAfterKeywords.add(translatedKeyword2)
                        myKeyword2BaseNameTable[translatedKeyword2] = keyword
                        keywordsTable.put(type, translatedKeyword2)
                    } else {
                        myKeyword2BaseNameTable[translatedKeyword] = keyword
                        keywordsTable.put(type, translatedKeyword)
                    }
                }
            }
        }
    }

    val allKeywords: Collection<String>
        get() = myKeyword2BaseNameTable.keys

    fun isSpaceAfterKeyword(keyword: String): Boolean {
        return mySpaceAfterKeywords.contains(keyword)
    }

    fun getTokenType(keyword: String): IElementType {
        return getTokenTypeByBaseKeyword(getBaseKeyword(keyword))
    }

    private fun getBaseKeyword(keyword: String): String? {
        return myKeyword2BaseNameTable[keyword]
    }

    companion object {
        // i18n.json file contains list of keywords and some meta-information about the language. At the moment it's three attributes below.
        private val GHERKIN_LANGUAGE_META_ATTRIBUTES: Collection<String> = mutableListOf("name", "native", "encoding")
        private fun capitalizeAndFixSpace(s: String): String {
            val result = StringBuilder()
            for (i in 0 until s.length) {
                var c = s[i]
                if (i == 0) {
                    c = c.uppercaseChar()
                }
                if (Character.isUpperCase(c) && i > 0) {
                    result.append(' ')
                }
                result.append(c)
            }
            return result.toString()
        }

        private fun getTokenTypeByBaseKeyword(baseKeyword: String?): IElementType {
            return PlainGherkinKeywordProvider.DEFAULT_KEYWORDS.get(baseKeyword)!!
        }
    }
}
