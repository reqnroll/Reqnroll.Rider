// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.openapi.util.NlsSafe
import com.intellij.psi.tree.TokenSet
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinElementTypes
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinExamplesBlock
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinTable

class GherkinExamplesBlockImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinExamplesBlock {
    override fun toString(): String {
        return "GherkinExamplesBlock:$elementText"
    }

    protected override val presentableText: @NlsSafe String?
        protected get() = buildPresentableText("Examples")

    override val table: GherkinTable?
        get() {
            val node = node
            val tableNode = node.findChildByType(TABLE_FILTER)
            return if (tableNode == null) null else tableNode.psi as GherkinTable
        }

    companion object {
        private val TABLE_FILTER = TokenSet.create(GherkinElementTypes.Companion.TABLE)
    }
}
