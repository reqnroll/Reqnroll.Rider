package com.jetbrains.rider.plugins.specflowriderplugin.ideaInterop.fileTypes

import com.intellij.openapi.fileTypes.LanguageFileType
import com.jetbrains.rider.plugins.specflowriderplugin.GherkinIcons

object GherkinFileType : LanguageFileType(GherkinLanguage) {
    override fun getName() = "SpecFlow"
    override fun getDefaultExtension() = "feature"
    override fun getDescription() = "SpecFlow file"
    override fun getIcon() = GherkinIcons.Gherkin
}