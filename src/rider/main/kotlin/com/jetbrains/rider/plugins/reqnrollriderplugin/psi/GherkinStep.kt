// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.lang.ASTNode
import com.intellij.pom.PomTarget
import com.intellij.psi.PsiNamedElement

interface GherkinStep : GherkinPsiElement, GherkinSuppressionHolder, PomTarget, PsiNamedElement {
    val keyword: ASTNode?
    override fun getName(): String
    val table: GherkinTable?

    companion object {
        val EMPTY_ARRAY = arrayOfNulls<GherkinStep>(0)
    }
}
