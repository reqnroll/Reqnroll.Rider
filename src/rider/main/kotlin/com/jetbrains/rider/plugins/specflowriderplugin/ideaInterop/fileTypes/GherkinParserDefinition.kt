package com.jetbrains.rider.plugins.specflowriderplugin.ideaInterop.fileTypes

import com.intellij.lexer.DummyLexer
import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.IFileElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderFileElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderParserDefinitionBase

class GherkinParserDefinition : RiderParserDefinitionBase(GherkinFileElementType, GherkinFileType) {
    companion object {
        val GherkinElementType = IElementType("RIDER_GHERKIN", GherkinLanguage)
        val GherkinFileElementType = RiderFileElementType("RIDER_GHERKIN_FILE", GherkinLanguage, GherkinElementType)
    }

    override fun createLexer(project: Project?): Lexer = DummyLexer(GherkinFileElementType)
    override fun getFileNodeType(): IFileElementType = GherkinFileElementType
}