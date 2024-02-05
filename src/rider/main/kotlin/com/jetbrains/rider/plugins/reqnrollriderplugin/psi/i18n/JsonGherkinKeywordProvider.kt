// Copyright 2000-2020 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.i18n

import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.google.gson.stream.MalformedJsonException
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.module.Module
import com.intellij.openapi.module.ModuleUtilCore
import com.intellij.psi.PsiElement
import com.intellij.psi.tree.IElementType
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.*
import java.io.IOException
import java.io.InputStream
import java.io.InputStreamReader
import java.nio.charset.StandardCharsets
import java.util.*

class JsonGherkinKeywordProvider(inputStream: InputStream) : GherkinKeywordProvider {
    private object Lazy {
        // leads to init of gher
        val myEmptyKeywordList: GherkinKeywordList = GherkinKeywordList()
    }

    private val myLanguageKeywords: MutableMap<String?, GherkinKeywordList> = HashMap<String?, GherkinKeywordList>()
    private val myAllStepKeywords: MutableSet<String> = HashSet()

    init {
        var fromJson: Map<String, Map<String, Any>>
        try {
            InputStreamReader(inputStream, StandardCharsets.UTF_8).use { `in` ->
                fromJson = Gson().fromJson(`in`, object : TypeToken<Map<String, HashMap<String, Any>>>() {}.type)
                for ((key, translation) in fromJson) {
                    val keywordList = GherkinKeywordList(translation)
                    myLanguageKeywords[key] = keywordList
                    for (keyword in keywordList.allKeywords) {
                        if (keywordList.getTokenType(keyword) === GherkinTokenTypes.STEP_KEYWORD) {
                            myAllStepKeywords.add(keyword)
                        }
                    }
                }
            }
        } catch (e: MalformedJsonException) {
            // ignore
        } catch (e: IOException) {
            Logger.getInstance(JsonGherkinKeywordProvider::class.java.getName()).error(e)
        }
    }

    override fun getAllKeywords(language: String?): Collection<String> {
        return getKeywordList(language).allKeywords
    }

    override fun getTokenType(language: String?, keyword: String): IElementType {
        return getKeywordList(language).getTokenType(keyword)
    }

    override fun isSpaceRequiredAfterKeyword(language: String?, keyword: String): Boolean {
        return getKeywordList(language).isSpaceAfterKeyword(keyword)
    }

    override fun isStepKeyword(keyword: String): Boolean {
        return myAllStepKeywords.contains(keyword)
    }

    override fun getKeywordsTable(language: String?): GherkinKeywordTable {
        return getKeywordList(language).keywordsTable
    }

    private fun getKeywordList(language: String?): GherkinKeywordList {
        var keywordList: GherkinKeywordList? = myLanguageKeywords[language]
        if (keywordList == null) {
            keywordList = Lazy.myEmptyKeywordList
        }
        return keywordList
    }

    companion object {
        private var myKeywordProvider: GherkinKeywordProvider? = null
        private var myGherkin6KeywordProvider: GherkinKeywordProvider? = null
        val keywordProvider: GherkinKeywordProvider
            get() {
                if (myKeywordProvider == null) {
                    myKeywordProvider = createKeywordProviderFromJson("i18n_old.json")
                }
                return myKeywordProvider!!
            }

        fun getKeywordProvider(gherkin6: Boolean): GherkinKeywordProvider {
            if (!gherkin6) {
                return keywordProvider
            }
            if (myGherkin6KeywordProvider == null) {
                myGherkin6KeywordProvider = createKeywordProviderFromJson("i18n.json")
            }
            return myGherkin6KeywordProvider!!
        }

        fun getKeywordProvider(context: PsiElement): GherkinKeywordProvider {
            val module: Module? = ModuleUtilCore.findModuleForPsiElement(context)
            val gherkin6Enabled = module != null
            return getKeywordProvider(gherkin6Enabled)
        }

        private fun createKeywordProviderFromJson(jsonFileName: String): GherkinKeywordProvider {
            var result: GherkinKeywordProvider? = null
            val classLoader = JsonGherkinKeywordProvider::class.java.getClassLoader()
            if (classLoader != null) {
                val gherkinKeywordStream = Objects.requireNonNull(classLoader.getResourceAsStream(jsonFileName))
                result = JsonGherkinKeywordProvider(gherkinKeywordStream)
            }
            return if (result != null) result else PlainGherkinKeywordProvider()
        }
    }
}
