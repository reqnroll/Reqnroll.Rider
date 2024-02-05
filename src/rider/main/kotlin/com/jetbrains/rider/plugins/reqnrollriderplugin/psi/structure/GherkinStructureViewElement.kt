// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.structure

import com.intellij.icons.AllIcons
import com.intellij.ide.structureView.StructureViewTreeElement
import com.intellij.ide.structureView.impl.common.PsiTreeElementBase
import com.intellij.navigation.NavigationItem
import com.intellij.psi.PsiElement
import com.jetbrains.rider.plugins.reqnrollriderplugin.ReqnrollIcons.Icons.Companion.ReqnrollLogo
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.*
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.GherkinFeatureHeaderImpl
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.GherkinTableImpl
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl.GherkinTagImpl
import javax.swing.Icon

class GherkinStructureViewElement(psiElement: PsiElement?) : PsiTreeElementBase<PsiElement?>(psiElement) {
    override fun getChildrenBase(): Collection<StructureViewTreeElement> {
        val result: MutableList<StructureViewTreeElement> = ArrayList()
        for (element in element!!.children) {
            if (element is GherkinPsiElement &&
                element !is GherkinFeatureHeaderImpl &&
                element !is GherkinTableImpl &&
                element !is GherkinTagImpl &&
                element !is GherkinPystring
            ) {
                result.add(GherkinStructureViewElement(element))
            }
        }
        return result
    }

    override fun getIcon(open: Boolean): Icon? {
        val element = element
        if (element is GherkinFeature
            || element is GherkinStepsHolder
        ) {
            return AllIcons.Nodes.LogFolder
        }
        return if (element is GherkinStep) {
            ReqnrollLogo
        } else null
    }

    override fun getPresentableText(): String? {
        return (element as NavigationItem?)!!.presentation!!.presentableText
    }
}
