package com.jetbrains.rider.plugins.specflowriderplugin.psi.completion

import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.PlainPrefixMatcher
import com.intellij.openapi.util.TextRange
import com.intellij.util.ProcessingContext
import com.jetbrains.rider.completion.ProtocolCompletionContributor
import com.jetbrains.rider.completion.ProtocolCompletionProvider

class GherkinCompletionProvider(owner: ProtocolCompletionContributor) : ProtocolCompletionProvider(owner) {
    private var currentCompletionParams: CompletionParameters? = null;

    override fun addCompletions(
        params: CompletionParameters,
        context: ProcessingContext,
        initialResultSet: CompletionResultSet
    ) {
        /**
         * Store `params` so we have access to it when `createCompletionConsumer` is called
         * This is very hacky and not threadsafe, for now I cannot see a scenario where this code is called in 2 thread
         * at the same time (until CodeWithMe ?)
         */
        currentCompletionParams = params;
        super.addCompletions(params, context, initialResultSet)
        currentCompletionParams = null;
    }

    override fun createCompletionConsumer(
        initialResultSet: CompletionResultSet,
        prefix: String,
        allSmallLetters: Boolean,
        onlyFirstLetterIsCapital: Boolean,
        casingIsDescendant: Boolean,
        params: CompletionParameters
    ): CompletionResultSet {
        /**
         * `prefix` is set to "" when completion is started after a space, in a step. so here we try to
         * find the full text of the step
         */
        val completionParams = currentCompletionParams
        if (completionParams != null) {
            val completionTextRange =
                completionParams.position.textRange.intersection(TextRange(0, completionParams.offset))
            val recomputedPrefix = completionParams.position.text.substring(0, completionTextRange.length);
            return super.createCompletionConsumer(
                initialResultSet,
                recomputedPrefix,
                allSmallLetters,
                onlyFirstLetterIsCapital,
                casingIsDescendant,
                params
            ).withPrefixMatcher(PlainPrefixMatcher(recomputedPrefix))
        }
        return super.createCompletionConsumer(
            initialResultSet,
            prefix,
            allSmallLetters,
            onlyFirstLetterIsCapital,
            casingIsDescendant,
            params
        ).withPrefixMatcher(GherkinPrefixMatcher(initialResultSet.prefixMatcher.prefix))
    }
}

class GherkinPrefixMatcher(prefix: String) : PlainPrefixMatcher(prefix) {

}