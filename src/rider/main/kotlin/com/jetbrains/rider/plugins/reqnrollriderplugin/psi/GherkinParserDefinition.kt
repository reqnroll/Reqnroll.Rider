// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.lang.*
import com.intellij.lang.ParserDefinition.SpaceRequirements
import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.FileViewProvider
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IFileElementType
import com.intellij.psi.tree.TokenSet
import com.intellij.psi.util.PsiUtilCore
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.i18n.JsonGherkinKeywordProvider
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.*

class GherkinParserDefinition : ParserDefinition {
    override fun createLexer(project: Project): Lexer {
        return GherkinLexer(JsonGherkinKeywordProvider.getKeywordProvider(true))
    }

    override fun createParser(project: Project): PsiParser {
        return GherkinParser()
    }

    override fun getFileNodeType(): IFileElementType {
        return GHERKIN_FILE
    }

    override fun getCommentTokens(): TokenSet {
        return GherkinTokenTypes.Companion.COMMENTS
    }

    override fun getStringLiteralElements(): TokenSet {
        return TokenSet.EMPTY
    }

    override fun createElement(node: ASTNode): PsiElement {
        if (node.elementType === GherkinElementTypes.Companion.FEATURE) return GherkinFeatureImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.FEATURE_HEADER) return GherkinFeatureHeaderImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.SCENARIO) return GherkinScenarioImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.STEP) return GherkinStepImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.SCENARIO_OUTLINE) return GherkinScenarioOutlineImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.RULE) return GherkinRuleImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.EXAMPLES_BLOCK) return GherkinExamplesBlockImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.TABLE) return GherkinTableImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.TABLE_ROW) return GherkinTableRowImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.TABLE_CELL) return GherkinTableCellImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.TABLE_HEADER_ROW) return GherkinTableHeaderRowImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.TAG) return GherkinTagImpl(node)
        if (node.elementType === GherkinElementTypes.Companion.STEP_PARAMETER) return GherkinStepParameterImpl(node)
        return if (node.elementType === GherkinElementTypes.Companion.PYSTRING) GherkinPystringImpl(node) else PsiUtilCore.NULL_PSI_ELEMENT
    }

    override fun createFile(viewProvider: FileViewProvider): PsiFile {
        return GherkinFileImpl(viewProvider)
    }

    override fun spaceExistenceTypeBetweenTokens(left: ASTNode, right: ASTNode): SpaceRequirements {
        // Line break between line comment and other elements
        val leftElementType = left.elementType
        if (leftElementType === GherkinTokenTypes.Companion.COMMENT) {
            return SpaceRequirements.MUST_LINE_BREAK
        }
        return if (right.elementType === GherkinTokenTypes.Companion.EXAMPLES_KEYWORD) {
            SpaceRequirements.MUST_LINE_BREAK
        } else SpaceRequirements.MAY
    }

    companion object {
        val GHERKIN_FILE = IFileElementType(GherkinLanguage)
    }
}
