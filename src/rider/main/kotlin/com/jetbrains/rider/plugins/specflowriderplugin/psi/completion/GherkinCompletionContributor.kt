package com.jetbrains.rider.plugins.specflowriderplugin.psi.completion

import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.jetbrains.rider.completion.ProtocolCompletionContributor

class GherkinCompletionContributor : ProtocolCompletionContributor() {

    override fun createCompletionProvider(): CompletionProvider<CompletionParameters> {
        return GherkinCompletionProvider(this);
    }


}