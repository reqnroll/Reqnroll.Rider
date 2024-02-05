package com.jetbrains.rider.plugins.reqnrollriderplugin.settings

import com.intellij.lang.Language
import com.intellij.psi.codeStyle.CodeStyleConfigurable
import com.intellij.psi.codeStyle.CodeStyleSettings
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinLanguage
import com.jetbrains.rider.settings.IRiderViewModelConfigurable
import com.jetbrains.rider.settings.RiderLanguageCodeStyleSettingsProvider

class GherkinStyleSettingsProvider : RiderLanguageCodeStyleSettingsProvider() {
    override fun getConfigurableDisplayName(): String {
        return language.displayName
    }

    override fun createConfigurable(baseSettings: CodeStyleSettings, modelSettings: CodeStyleSettings): CodeStyleConfigurable {
        return createRiderConfigurable(baseSettings, modelSettings, language, configurableDisplayName)
    }

    override fun getLanguage(): Language = GherkinLanguage

    override fun getPagesId(): Map<String, String>{
        return mapOf(
            "GherkinDotnetFormattingStylePage" to "Formatting Style")
    }

    override fun getHelpTopic(): String = "Settings_Code_Style_GHERKIN"

    override fun filterPages(filterTag: String): Map<String, String> {
        if (filterTag == IRiderViewModelConfigurable.EditorConfigFilterTag)
            return mapOf(
                "GherkinDotnetFormattingStylePage" to "Formatting Style")

        return super.filterPages(filterTag)
    }
}