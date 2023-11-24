// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.*
import com.intellij.openapi.util.NlsSafe
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinRule
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinStepsHolder
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTokenTypes

class GherkinRuleImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinRule {
    override fun toString(): String {
        return "GherkinRule:" + ruleName
    }

    override val ruleName: String
        get() {
            val node = node
            val firstText: ASTNode? = node.findChildByType(GherkinTokenTypes.TEXT)
            return firstText?.text ?: elementText
        }
    override val scenarios: Array<GherkinStepsHolder?>
        get() {
            val children = PsiTreeUtil.getChildrenOfType(this, GherkinStepsHolder::class.java)
            return children ?: GherkinStepsHolder.Companion.EMPTY_ARRAY
        }
    protected override val presentableText: @NlsSafe String?
        protected get() = "Rule: " + ruleName
}
