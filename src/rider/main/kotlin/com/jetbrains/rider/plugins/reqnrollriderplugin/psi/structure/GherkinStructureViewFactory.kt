// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.structure

import com.intellij.ide.structureView.StructureViewBuilder
import com.intellij.ide.structureView.StructureViewModel
import com.intellij.ide.structureView.StructureViewModelBase
import com.intellij.ide.structureView.TreeBasedStructureViewBuilder
import com.intellij.lang.PsiStructureViewFactory
import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinFeature
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinFile
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinStep
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinStepsHolder

class GherkinStructureViewFactory : PsiStructureViewFactory {
    override fun getStructureViewBuilder(psiFile: PsiFile): StructureViewBuilder? {
        return object : TreeBasedStructureViewBuilder() {
            override fun createStructureViewModel(editor: Editor?): StructureViewModel {
                var root: PsiElement? = PsiTreeUtil.getChildOfType(psiFile, GherkinFeature::class.java)
                if (root == null) {
                    root = psiFile
                }
                return StructureViewModelBase(psiFile, editor, GherkinStructureViewElement(root))
                    .withSuitableClasses(
                        GherkinFile::class.java,
                        GherkinFeature::class.java,
                        GherkinStepsHolder::class.java,
                        GherkinStep::class.java
                    )
            }
        }
    }
}