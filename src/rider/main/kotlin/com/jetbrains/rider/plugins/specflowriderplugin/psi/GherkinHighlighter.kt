package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.HighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import org.jetbrains.annotations.NonNls

/**
 * @author Roman.Chernyatchik
 */
object GherkinHighlighter {
    val COMMENT_ID: @NonNls String? = "GHERKIN_COMMENT"
    val COMMENT = TextAttributesKey.createTextAttributesKey(
        COMMENT_ID!!,
        DefaultLanguageHighlighterColors.DOC_COMMENT
    )
    val KEYWORD_ID: @NonNls String? = "GHERKIN_KEYWORD"
    val KEYWORD = TextAttributesKey.createTextAttributesKey(
        KEYWORD_ID!!,
        DefaultLanguageHighlighterColors.KEYWORD
    )
    val GHERKIN_OUTLINE_PARAMETER_SUBSTITUTION_ID: @NonNls String? = "GHERKIN_OUTLINE_PARAMETER_SUBSTITUTION"
    val OUTLINE_PARAMETER_SUBSTITUTION = TextAttributesKey.createTextAttributesKey(
        GHERKIN_OUTLINE_PARAMETER_SUBSTITUTION_ID!!,
        DefaultLanguageHighlighterColors.INSTANCE_FIELD
    )
    val GHERKIN_TABLE_HEADER_CELL_ID: @NonNls String? = "GHERKIN_TABLE_HEADER_CELL"
    val TABLE_HEADER_CELL = TextAttributesKey.createTextAttributesKey(
        GHERKIN_TABLE_HEADER_CELL_ID!!,
        OUTLINE_PARAMETER_SUBSTITUTION
    )
    val GHERKIN_TAG_ID: @NonNls String? = "GHERKIN_TAG"
    val TAG = TextAttributesKey.createTextAttributesKey(
        GHERKIN_TAG_ID!!,
        DefaultLanguageHighlighterColors.METADATA
    )
    val GHERKIN_REGEXP_PARAMETER_ID: @NonNls String? = "GHERKIN_REGEXP_PARAMETER"
    val REGEXP_PARAMETER = TextAttributesKey.createTextAttributesKey(
        GHERKIN_REGEXP_PARAMETER_ID!!,
        DefaultLanguageHighlighterColors.PARAMETER
    )
    val GHERKIN_TABLE_CELL_ID: @NonNls String? = "GHERKIN_TABLE_CELL"
    val TABLE_CELL = TextAttributesKey.createTextAttributesKey(
        GHERKIN_TABLE_CELL_ID!!,
        REGEXP_PARAMETER
    )
    val GHERKIN_PYSTRING_ID: @NonNls String? = "GHERKIN_PYSTRING"
    val PYSTRING = TextAttributesKey.createTextAttributesKey(
        GHERKIN_PYSTRING_ID!!,
        DefaultLanguageHighlighterColors.STRING
    )
    val TEXT = TextAttributesKey.createTextAttributesKey("GHERKIN_TEXT", HighlighterColors.TEXT)
    val PIPE = TextAttributesKey.createTextAttributesKey("GHERKIN_TABLE_PIPE", KEYWORD)
}
