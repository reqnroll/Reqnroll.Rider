// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.lexer.LexerBase
import com.intellij.openapi.util.text.Strings
import com.intellij.psi.*
import com.intellij.psi.tree.IElementType

class GherkinLexer(private val myKeywordProvider: GherkinKeywordProvider) : LexerBase() {
    private var myBuffer: CharSequence = Strings.EMPTY_CHAR_SEQUENCE
    private var myStartOffset = 0
    private var myEndOffset = 0
    private var myPosition = 0
    private var myCurrentToken: IElementType? = null
    private var myCurrentTokenStart = 0
    private var myKeywords: ArrayList<String>? = null
    private var myState = 0
    private var myCurLanguage: String? = null

    init {
        updateLanguage("en")
    }

    private fun updateLanguage(language: String) {
        myCurLanguage = language
        myKeywords = ArrayList(myKeywordProvider.getAllKeywords(language))
        myKeywords?.sortWith { o1: String, o2: String -> o2.length - o1.length }
    }

    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        myBuffer = buffer
        myStartOffset = startOffset
        myEndOffset = endOffset
        myPosition = startOffset
        myState = initialState
        advance()
    }

    override fun getState(): Int {
        return myState
    }

    override fun getTokenType(): IElementType? {
        return myCurrentToken
    }

    override fun getTokenStart(): Int {
        return myCurrentTokenStart
    }

    override fun getTokenEnd(): Int {
        return myPosition
    }

    private fun isStepParameter(currentElementTerminator: String): Boolean {
        var pos = myPosition
        if (myBuffer[pos] == '<') {
            while (pos < myEndOffset && myBuffer[pos] != '\n' && myBuffer[pos] != '>' && !isStringAtPosition(
                    currentElementTerminator,
                    pos
                )
            ) {
                pos++
            }
            return pos < myEndOffset && myBuffer[pos] == '>'
        }
        return false
    }

    override fun advance() {
        if (myPosition >= myEndOffset) {
            myCurrentToken = null
            return
        }
        myCurrentTokenStart = myPosition
        val c = myBuffer[myPosition]
        if (myState != STATE_INSIDE_PYSTRING && Character.isWhitespace(c)) {
            advanceOverWhitespace()
            myCurrentToken = TokenType.WHITE_SPACE
            while (myPosition < myEndOffset && Character.isWhitespace(myBuffer[myPosition])) {
                advanceOverWhitespace()
            }
        } else if (c == '|' && myState != STATE_INSIDE_PYSTRING) {
            myCurrentToken = GherkinTokenTypes.PIPE
            myPosition++
            myState = STATE_TABLE
        } else if (myState == STATE_PARAMETER_INSIDE_PYSTRING) {
            if (c == '>') {
                myState = STATE_INSIDE_PYSTRING
                myPosition++
                myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_BRACE
            } else {
                advanceToParameterEnd(PYSTRING_MARKER)
                myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_TEXT
            }
        } else if (myState == STATE_INSIDE_PYSTRING) {
            if (isStringAtPosition(PYSTRING_MARKER)) {
                myPosition += 3 /* marker length */
                myCurrentToken = GherkinTokenTypes.PYSTRING
                myState = STATE_DEFAULT
            } else {
                if (myBuffer[myPosition] == '<') {
                    if (isStepParameter(PYSTRING_MARKER)) {
                        myPosition++
                        myState = STATE_PARAMETER_INSIDE_PYSTRING
                        myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_BRACE
                    } else {
                        myPosition++
                        advanceToParameterOrSymbol(PYSTRING_MARKER, STATE_INSIDE_PYSTRING, false)
                        myCurrentToken = GherkinTokenTypes.PYSTRING_TEXT
                    }
                } else {
                    advanceToParameterOrSymbol(PYSTRING_MARKER, STATE_INSIDE_PYSTRING, false)
                    myCurrentToken = GherkinTokenTypes.PYSTRING_TEXT
                }
            }
        } else if (myState == STATE_TABLE) {
            myCurrentToken = GherkinTokenTypes.TABLE_CELL
            while (myPosition < myEndOffset) {
                // Cucumber: 0.7.3 Table cells can now contain escaped bars - \| and escaped backslashes - \\
                if (myBuffer[myPosition] == '\\') {
                    val nextPos = myPosition + 1
                    if (nextPos < myEndOffset) {
                        val nextChar = myBuffer[nextPos]
                        if (nextChar == '|' || nextChar == '\\') {
                            myPosition += 2
                            continue
                        }
                        // else - common case
                    }
                } else if (myBuffer[myPosition] == '|' || myBuffer[myPosition] == '\n') {
                    break
                }
                myPosition++
            }
            while (myPosition > 0 && Character.isWhitespace(myBuffer[myPosition - 1])) {
                myPosition--
            }
        } else if (c == '#') {
            myCurrentToken = GherkinTokenTypes.COMMENT
            advanceToEOL()
            val commentText = myBuffer.subSequence(myCurrentTokenStart + 1, myPosition).toString().trim { it <= ' ' }
            val language = fetchLocationLanguage(commentText)
            language?.let { updateLanguage(it) }
        } else if (c == ':' && myState != STATE_AFTER_STEP_KEYWORD) {
            myCurrentToken = GherkinTokenTypes.COLON
            myPosition++
        } else if (c == '@') {
            myCurrentToken = GherkinTokenTypes.TAG
            myPosition++
            while (myPosition < myEndOffset && isValidTagChar(myBuffer[myPosition])) {
                myPosition++
            }
        } else if (isStringAtPosition(PYSTRING_MARKER)) {
            myCurrentToken = GherkinTokenTypes.PYSTRING
            myState = STATE_INSIDE_PYSTRING
            myPosition += 3
        } else {
            if (myState == STATE_DEFAULT) {
                for (keyword in myKeywords!!) {
                    val length = keyword.length
                    if (isStringAtPosition(keyword)) {
                        if (myKeywordProvider.isSpaceRequiredAfterKeyword(
                                myCurLanguage,
                                keyword
                            ) && myEndOffset - myPosition > length &&
                            Character.isLetterOrDigit(myBuffer[myPosition + length])
                        ) {
                            continue
                        }
                        val followedByChar: Char =
                            if (myPosition + length < myEndOffset) myBuffer[myPosition + length] else 0.toChar()
                        myCurrentToken = myKeywordProvider.getTokenType(myCurLanguage, keyword)
                        if (myCurrentToken === GherkinTokenTypes.STEP_KEYWORD) {
                            val followedByWhitespace = Character.isWhitespace(followedByChar) && followedByChar != '\n'
                            if (followedByWhitespace != myKeywordProvider.isSpaceRequiredAfterKeyword(
                                    myCurLanguage,
                                    keyword
                                )
                            ) {
                                myCurrentToken = GherkinTokenTypes.TEXT
                            }
                        }
                        myPosition += length
                        myState = if (myCurrentToken === GherkinTokenTypes.STEP_KEYWORD) {
                            STATE_AFTER_STEP_KEYWORD
                        } else if (myCurrentToken === GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD) {
                            STATE_AFTER_SCENARIO_KEYWORD
                        } else {
                            STATE_AFTER_KEYWORD
                        }
                        return
                    }
                }
            }
            if (myState == STATE_PARAMETER_INSIDE_STEP) {
                if (c == '>') {
                    myState = STATE_AFTER_STEP_KEYWORD
                    myPosition++
                    myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_BRACE
                } else {
                    advanceToParameterEnd("\n")
                    myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_TEXT
                }
                return
            } else if (isParameterAllowed) {
                if (myPosition < myEndOffset && myBuffer[myPosition] == '<' && isStepParameter("\n")) {
                    myState = STATE_PARAMETER_INSIDE_STEP
                    myPosition++
                    myCurrentToken = GherkinTokenTypes.STEP_PARAMETER_BRACE
                } else {
                    myCurrentToken = GherkinTokenTypes.TEXT
                    advanceToParameterOrSymbol("\n", STATE_AFTER_STEP_KEYWORD, true)
                }
                return
            }
            myCurrentToken = GherkinTokenTypes.TEXT
            advanceToEOL()
        }
    }

    private val isParameterAllowed: Boolean
        get() = myState == STATE_AFTER_STEP_KEYWORD || myState == STATE_AFTER_SCENARIO_KEYWORD

    private fun advanceOverWhitespace() {
        if (myBuffer[myPosition] == '\n') {
            myState = STATE_DEFAULT
        }
        myPosition++
    }

    private fun isStringAtPosition(keyword: String?): Boolean {
        val length = keyword!!.length
        return myEndOffset - myPosition >= length && myBuffer.subSequence(myPosition, myPosition + length)
            .toString() == keyword
    }

    private fun isStringAtPosition(keyword: String, position: Int): Boolean {
        val length = keyword.length
        return myEndOffset - position >= length && myBuffer.subSequence(position, position + length)
            .toString() == keyword
    }

    private fun advanceToEOL() {
        myPosition++
        val mark = myPosition
        while (myPosition < myEndOffset && myBuffer[myPosition] != '\n') {
            myPosition++
        }
        returnWhitespace(mark)
        myState = STATE_DEFAULT
    }

    private fun returnWhitespace(mark: Int) {
        while (myPosition > mark && Character.isWhitespace(myBuffer[myPosition - 1])) {
            myPosition--
        }
    }

    private fun advanceToParameterOrSymbol(s: String, parameterState: Int, shouldReturnWhitespace: Boolean) {
        val mark = myPosition
        while (myPosition < myEndOffset && !isStringAtPosition(s) && !isStepParameter(s)) {
            myPosition++
        }
        if (shouldReturnWhitespace) {
            myState = STATE_DEFAULT
            if (myPosition < myEndOffset) {
                if (!isStringAtPosition(s)) {
                    myState = parameterState
                }
            }
            returnWhitespace(mark)
        }
    }

    private fun advanceToParameterEnd(endSymbol: String) {
        myPosition++
        val mark = myPosition
        while (myPosition < myEndOffset && !isStringAtPosition(endSymbol) && myBuffer[myPosition] != '>') {
            myPosition++
        }
        if (myPosition < myEndOffset) {
            if (isStringAtPosition(endSymbol)) {
                myState = STATE_DEFAULT
            }
        }
        returnWhitespace(mark)
    }

    override fun getBufferSequence(): CharSequence {
        return myBuffer
    }

    override fun getBufferEnd(): Int {
        return myEndOffset
    }

    companion object {
        private const val STATE_DEFAULT = 0
        private const val STATE_AFTER_KEYWORD = 1
        private const val STATE_TABLE = 2
        private const val STATE_AFTER_STEP_KEYWORD = 3
        private const val STATE_AFTER_SCENARIO_KEYWORD = 4
        private const val STATE_INSIDE_PYSTRING = 5
        private const val STATE_PARAMETER_INSIDE_PYSTRING = 6
        private const val STATE_PARAMETER_INSIDE_STEP = 7
        const val PYSTRING_MARKER = "\"\"\""
        fun fetchLocationLanguage(commentText: String): String? {
            return if (commentText.startsWith("language:")) {
                commentText.substring(9).trim { it <= ' ' }
            } else null
        }

        private fun isValidTagChar(c: Char): Boolean {
            return !Character.isWhitespace(c) && c != '@'
        }
    }
}
