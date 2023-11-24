// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.openapi.util.NlsSafe
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinScenario
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTokenTypes

class GherkinScenarioImpl(node: ASTNode) : GherkinStepsHolderBase(node), GherkinScenario {
    override fun toString(): String {
        return if (isBackground) {
            "GherkinScenario(Background):"
        } else "GherkinScenario:$scenarioName"
    }

    override val isBackground: Boolean
        get() {
            val node = node.firstChildNode
            return node != null && node.elementType === GherkinTokenTypes.Companion.BACKGROUND_KEYWORD
        }
    protected override val presentableText: @NlsSafe String?
        protected get() = buildPresentableText(if (isBackground) "Background" else scenarioKeyword)
}
