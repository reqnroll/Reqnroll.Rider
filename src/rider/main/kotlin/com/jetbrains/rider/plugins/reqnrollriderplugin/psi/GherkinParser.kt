package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.lang.*
import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.TokenSet

class GherkinParser : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val marker = builder.mark()
        parseFileTopLevel(builder)
        marker.done(GherkinParserDefinition.GHERKIN_FILE)
        return builder.treeBuilt
    }

    companion object {
        private val SCENARIO_END_TOKENS = TokenSet.create(
            GherkinTokenTypes.BACKGROUND_KEYWORD,
            GherkinTokenTypes.SCENARIO_KEYWORD,
            GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD,
            GherkinTokenTypes.RULE_KEYWORD,
            GherkinTokenTypes.FEATURE_KEYWORD
        )

        private fun parseFileTopLevel(builder: PsiBuilder) {
            while (!builder.eof()) {
                val tokenType = builder.tokenType
                if (tokenType === GherkinTokenTypes.FEATURE_KEYWORD) {
                    parseFeature(builder)
                } else if (tokenType === GherkinTokenTypes.TAG) {
                    parseTags(builder)
                } else {
                    builder.advanceLexer()
                }
            }
        }

        private fun parseFeature(builder: PsiBuilder) {
            val marker = builder.mark()
            assert(builder.tokenType === GherkinTokenTypes.FEATURE_KEYWORD)
            val featureEnd = builder.currentOffset + getTokenLength(builder.tokenText)
            var descMarker: PsiBuilder.Marker? = null
            while (true) {
                val tokenType = builder.tokenType
                if (tokenType === GherkinTokenTypes.TEXT && descMarker == null) {
                    if (hadLineBreakBefore(builder, featureEnd)) {
                        descMarker = builder.mark()
                    }
                }
                if (GherkinTokenTypes.SCENARIOS_KEYWORDS.contains(tokenType) || tokenType === GherkinTokenTypes.RULE_KEYWORD || tokenType === GherkinTokenTypes.BACKGROUND_KEYWORD || tokenType === GherkinTokenTypes.TAG) {
                    if (descMarker != null) {
                        descMarker.done(GherkinElementTypes.FEATURE_HEADER)
                        descMarker = null
                    }
                    parseFeatureElements(builder)
                    if (builder.tokenType === GherkinTokenTypes.FEATURE_KEYWORD) {
                        break
                    }
                }
                builder.advanceLexer()
                if (builder.eof()) break
            }
            descMarker?.done(GherkinElementTypes.FEATURE_HEADER)
            marker.done(GherkinElementTypes.FEATURE)
        }

        private fun hadLineBreakBefore(builder: PsiBuilder, prevTokenEnd: Int): Boolean {
            if (prevTokenEnd < 0) return false
            val precedingText = builder.originalText.subSequence(prevTokenEnd, builder.currentOffset).toString()
            return precedingText.contains("\n")
        }

        private fun parseTags(builder: PsiBuilder) {
            while (builder.tokenType === GherkinTokenTypes.TAG) {
                val tagMarker = builder.mark()
                builder.advanceLexer()
                tagMarker.done(GherkinElementTypes.TAG)
            }
        }

        private fun parseFeatureElements(builder: PsiBuilder) {
            var ruleMarker: PsiBuilder.Marker? = null
            while (builder.tokenType !== GherkinTokenTypes.FEATURE_KEYWORD && !builder.eof()) {
                if (builder.tokenType === GherkinTokenTypes.RULE_KEYWORD) {
                    ruleMarker?.done(GherkinElementTypes.RULE)
                    ruleMarker = builder.mark()
                    builder.advanceLexer()
                    if (builder.tokenType === GherkinTokenTypes.COLON) {
                        builder.advanceLexer()
                    } else {
                        break
                    }
                    while (builder.tokenType === GherkinTokenTypes.TEXT) {
                        builder.advanceLexer()
                    }
                }
                val marker = builder.mark()
                // tags
                parseTags(builder)

                // scenarios
                val startTokenType = builder.tokenType
                val outline = startTokenType === GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD
                builder.advanceLexer()
                parseScenario(builder)
                marker.done(if (outline) GherkinElementTypes.SCENARIO_OUTLINE else GherkinElementTypes.SCENARIO)
            }
            ruleMarker?.done(GherkinElementTypes.RULE)
        }

        private fun parseScenario(builder: PsiBuilder) {
            while (!atScenarioEnd(builder)) {
                if (builder.tokenType === GherkinTokenTypes.TAG) {
                    val marker = builder.mark()
                    parseTags(builder)
                    if (atScenarioEnd(builder)) {
                        marker.rollbackTo()
                        break
                    } else {
                        marker.drop()
                    }
                }
                if (parseStepParameter(builder)) {
                    continue
                }
                if (builder.tokenType === GherkinTokenTypes.STEP_KEYWORD) {
                    parseStep(builder)
                } else if (builder.tokenType === GherkinTokenTypes.EXAMPLES_KEYWORD) {
                    parseExamplesBlock(builder)
                } else {
                    builder.advanceLexer()
                }
            }
        }

        private fun atScenarioEnd(builder: PsiBuilder): Boolean {
            var i = 0
            while (builder.lookAhead(i) === GherkinTokenTypes.TAG) {
                i++
            }
            val tokenType = builder.lookAhead(i)
            return tokenType == null || SCENARIO_END_TOKENS.contains(tokenType)
        }

        private fun parseStepParameter(builder: PsiBuilder): Boolean {
            if (builder.tokenType === GherkinTokenTypes.STEP_PARAMETER_TEXT) {
                val stepParameterMarker = builder.mark()
                builder.advanceLexer()
                stepParameterMarker.done(GherkinElementTypes.STEP_PARAMETER)
                return true
            }
            return false
        }

        private fun parseStep(builder: PsiBuilder) {
            val marker = builder.mark()
            builder.advanceLexer()
            var prevTokenEnd = -1
            while (builder.tokenType === GherkinTokenTypes.TEXT || builder.tokenType === GherkinTokenTypes.STEP_PARAMETER_BRACE || builder.tokenType === GherkinTokenTypes.STEP_PARAMETER_TEXT) {
                val tokenText = builder.tokenText
                if (hadLineBreakBefore(builder, prevTokenEnd)) {
                    break
                }
                prevTokenEnd = builder.currentOffset + getTokenLength(tokenText)
                if (!parseStepParameter(builder)) {
                    builder.advanceLexer()
                }
            }
            val tokenTypeAfterName = builder.tokenType
            if (tokenTypeAfterName === GherkinTokenTypes.PIPE) {
                parseTable(builder)
            } else if (tokenTypeAfterName === GherkinTokenTypes.PYSTRING) {
                parsePystring(builder)
            }
            marker.done(GherkinElementTypes.STEP)
        }

        private fun parsePystring(builder: PsiBuilder) {
            if (!builder.eof()) {
                val marker = builder.mark()
                builder.advanceLexer()
                while (!builder.eof() && builder.tokenType !== GherkinTokenTypes.PYSTRING) {
                    if (!parseStepParameter(builder)) {
                        builder.advanceLexer()
                    }
                }
                if (!builder.eof()) {
                    builder.advanceLexer()
                }
                marker.done(GherkinElementTypes.PYSTRING)
            }
        }

        private fun parseExamplesBlock(builder: PsiBuilder) {
            val marker = builder.mark()
            builder.advanceLexer()
            if (builder.tokenType === GherkinTokenTypes.COLON) builder.advanceLexer()
            while (builder.tokenType === GherkinTokenTypes.TEXT) {
                builder.advanceLexer()
            }
            if (builder.tokenType === GherkinTokenTypes.PIPE) {
                parseTable(builder)
            }
            marker.done(GherkinElementTypes.EXAMPLES_BLOCK)
        }

        private fun parseTable(builder: PsiBuilder) {
            val marker = builder.mark()
            var rowMarker = builder.mark()
            var prevCellEnd = -1
            var isHeaderRow = true
            var cellMarker: PsiBuilder.Marker? = null
            var prevToken: IElementType? = null
            while (builder.tokenType === GherkinTokenTypes.PIPE || builder.tokenType === GherkinTokenTypes.TABLE_CELL) {
                val tokenType = builder.tokenType
                val hasLineBreakBefore = hadLineBreakBefore(builder, prevCellEnd)

                // cell - is all between pipes
                if (prevToken === GherkinTokenTypes.PIPE) {
                    // Don't start new cell if prev was last in the row
                    // it's not a cell, we just need to close a row
                    if (!hasLineBreakBefore) {
                        cellMarker = builder.mark()
                    }
                }
                if (tokenType === GherkinTokenTypes.PIPE) {
                    if (cellMarker != null) {
                        closeCell(cellMarker)
                        cellMarker = null
                    }
                }
                if (hasLineBreakBefore) {
                    closeRowMarker(rowMarker, isHeaderRow)
                    isHeaderRow = false
                    rowMarker = builder.mark()
                }
                prevCellEnd = builder.currentOffset + getTokenLength(builder.tokenText)
                prevToken = tokenType
                builder.advanceLexer()
            }
            if (cellMarker != null) {
                closeCell(cellMarker)
            }
            closeRowMarker(rowMarker, isHeaderRow)
            marker.done(GherkinElementTypes.TABLE)
        }

        private fun closeCell(cellMarker: PsiBuilder.Marker) {
            cellMarker.done(GherkinElementTypes.TABLE_CELL)
        }

        private fun closeRowMarker(rowMarker: PsiBuilder.Marker, headerRow: Boolean) {
            rowMarker.done(if (headerRow) GherkinElementTypes.TABLE_HEADER_ROW else GherkinElementTypes.TABLE_ROW)
        }

        private fun getTokenLength(tokenText: String?): Int {
            return tokenText?.length ?: 0
        }
    }
}
