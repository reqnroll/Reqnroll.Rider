package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.psi.tree.IElementType

/**
 * @author Roman.Chernyatchik
 */
class GherkinKeywordTable {
    private val myType2KeywordsTable: MutableMap<IElementType, MutableCollection<String>> = HashMap()

    init {
        for (type in GherkinTokenTypes.Companion.KEYWORDS.getTypes()) {
            myType2KeywordsTable[type] = ArrayList()
        }
    }

    fun putAllKeywordsInto(target: MutableMap<String, IElementType>) {
        for (type in types) {
            val keywords: Collection<String>? = getKeywords(type)
            if (keywords != null) {
                for (keyword in keywords) {
                    target[keyword] = type
                }
            }
        }
    }

    fun put(type: IElementType, keyword: String) {
        if (GherkinTokenTypes.Companion.KEYWORDS.contains(type)) {
            var keywords = getKeywords(type)
            if (keywords == null) {
                keywords = ArrayList(1)
                myType2KeywordsTable[type] = keywords
            }
            keywords.add(keyword)
        }
    }

    val stepKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.STEP_KEYWORD)!!
    val scenarioKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.SCENARIO_KEYWORD)
    val scenarioLikeKeywords: Collection<String>
        get() {
            val keywords: MutableSet<String> = HashSet()
            val scenarios: Collection<String> = getKeywords(GherkinTokenTypes.Companion.SCENARIO_KEYWORD)!!
            keywords.addAll(scenarios)
            val scenarioOutline: Collection<String> =
                getKeywords(GherkinTokenTypes.Companion.SCENARIO_OUTLINE_KEYWORD)!!
            keywords.addAll(scenarioOutline)
            return keywords
        }
    val ruleKeywords: Collection<String>
        get() {
            val result: Collection<String>? = getKeywords(GherkinTokenTypes.Companion.RULE_KEYWORD)
            return result ?: emptyList()
        }
    val scenarioOutlineKeyword: String
        get() = scenarioOutlineKeywords!!.iterator().next()
    val scenarioOutlineKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.SCENARIO_OUTLINE_KEYWORD)!!
    val backgroundKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.BACKGROUND_KEYWORD)!!
    val exampleSectionKeyword: String
        get() = exampleSectionKeywords!!.iterator().next()
    val exampleSectionKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.EXAMPLES_KEYWORD)!!
    val featureSectionKeyword: String
        get() = featuresSectionKeywords!!.iterator().next()
    val featuresSectionKeywords: Collection<String>?
        get() = getKeywords(GherkinTokenTypes.Companion.FEATURE_KEYWORD)!!
    val types: Collection<IElementType>
        get() = myType2KeywordsTable.keys

    fun getKeywords(type: IElementType): MutableCollection<String>? {
        return myType2KeywordsTable[type]
    }

    fun tableContainsKeyword(type: GherkinElementType, keyword: String): Boolean {
        val alreadyKnownKeywords: Collection<String>? = getKeywords(type)
        return null != alreadyKnownKeywords && alreadyKnownKeywords.contains(keyword)
    }
}