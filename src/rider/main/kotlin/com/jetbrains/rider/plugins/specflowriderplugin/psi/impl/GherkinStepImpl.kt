// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.*
import com.intellij.openapi.util.NlsSafe
import com.intellij.psi.*
import com.intellij.psi.impl.source.resolve.reference.ReferenceProvidersRegistry
import com.intellij.psi.util.CachedValueProvider
import com.intellij.psi.util.CachedValuesManager
import com.intellij.util.IncorrectOperationException
import com.jetbrains.rider.plugins.specflowriderplugin.psi.*

class GherkinStepImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinStep, PsiCheckedRenameElement {
    override fun toString(): String {
        return "GherkinStep:$name"
    }

    override val keyword: ASTNode?
        get() = node.findChildByType(GherkinTokenTypes.STEP_KEYWORD)
    override val table: GherkinTable?
        get() {
            val tableNode: ASTNode? = node.findChildByType(GherkinElementTypes.TABLE)
            return if (tableNode == null) null else tableNode.psi as GherkinTable
        }

    override fun getReferences(): Array<PsiReference> {
        return CachedValuesManager.getCachedValue(this) {
            CachedValueProvider.Result.create(
                referencesInner, this
            )
        }
    }

    private val referencesInner: Array<PsiReference>
        get() = ReferenceProvidersRegistry.getReferencesFromProviders(this)

    override fun getName(): String {
        return elementText
    }

    @Throws(IncorrectOperationException::class)
    override fun setName(s: @NlsSafe String): PsiElement? {
        return null
    }

    @Throws(IncorrectOperationException::class)
    override fun checkSetName(s: String) {
    }
}
