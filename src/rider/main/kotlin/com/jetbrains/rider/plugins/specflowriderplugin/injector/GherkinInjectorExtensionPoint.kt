package com.jetbrains.rider.plugins.specflowriderplugin.injector

import com.intellij.openapi.extensions.ExtensionPointName

interface GherkinInjectorExtensionPoint {
    /**
     * Returns the prefix to be injected for the specified language
     *
     * @param language injected language
     * @return injection prefix
     */
    fun getPrefix(language: String?): String? {
        return ""
    }

    /**
     * Returns the suffix to be injected for the specified language
     *
     * @param language injected language
     * @return injection suffix
     */
    fun getSuffix(language: String?): String? {
        return ""
    }

    companion object {
        val EP_NAME =
            ExtensionPointName.create<GherkinInjectorExtensionPoint>("com.jetbrains.rider.plugins.specflowriderplugin.injector.injectorExtensionPoint")
    }
}