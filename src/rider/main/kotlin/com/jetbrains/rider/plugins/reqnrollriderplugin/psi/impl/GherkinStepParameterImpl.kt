package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl

import com.intellij.lang.*
import com.intellij.openapi.externalSystem.service.execution.NotSupportedException
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiReference
import com.intellij.psi.search.LocalSearchScope
import com.intellij.psi.search.SearchScope
import com.intellij.util.IncorrectOperationException
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinStepParameter
import org.jetbrains.annotations.NonNls

class GherkinStepParameterImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinStepParameter {
    override fun toString(): String {
        return "GherkinStepParameter:$text"
    }

    @Throws(IncorrectOperationException::class)
    override fun setName(name: @NonNls String): PsiElement {
        throw NotSupportedException("FIXME")
    }

    override fun getReference(): PsiReference? {
        return GherkinStepParameterReference(this)
    }

    override fun getName(): String? {
        return text
    }

    override fun getNameIdentifier(): PsiElement? {
        return this
    }

    override fun getUseScope(): SearchScope {
        return LocalSearchScope(containingFile)
    }
}
