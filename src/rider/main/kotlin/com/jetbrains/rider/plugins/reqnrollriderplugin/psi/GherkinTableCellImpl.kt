package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.lang.*
import com.intellij.openapi.externalSystem.service.execution.NotSupportedException
import com.intellij.openapi.util.NlsSafe
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiReference
import com.intellij.psi.impl.source.resolve.reference.ReferenceProvidersRegistry
import com.intellij.psi.impl.source.tree.LeafPsiElement
import com.intellij.psi.search.LocalSearchScope
import com.intellij.psi.search.SearchScope
import com.intellij.psi.util.CachedValueProvider
import com.intellij.psi.util.CachedValuesManager
import com.intellij.psi.util.PsiTreeUtil
import com.intellij.util.IncorrectOperationException
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.GherkinPsiElementBase
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.GherkinSimpleReference
import org.jetbrains.annotations.NonNls

/**
 * @author Roman.Chernyatchik
 */
open class GherkinTableCellImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinTableCell {
    override val presentableText: @NlsSafe String?
        get() = String.format("Step parameter '%s'", name)

    override fun getReference(): PsiReference? {
        return GherkinSimpleReference(this)
    }

    override fun getName(): String? {
        return text
    }

    @Throws(IncorrectOperationException::class)
    override fun setName(name: @NonNls String): PsiElement {
        throw NotSupportedException("FIXME")
    }

    override fun getNameIdentifier(): PsiElement? {
        return PsiTreeUtil.getChildOfType(this, LeafPsiElement::class.java)
    }

    override fun getUseScope(): SearchScope {
        return LocalSearchScope(containingFile)
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
}
