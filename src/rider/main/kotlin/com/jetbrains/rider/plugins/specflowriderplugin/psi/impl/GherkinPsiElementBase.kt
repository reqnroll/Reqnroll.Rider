// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.extapi.psi.ASTWrapperPsiElement
import com.intellij.lang.*
import com.intellij.navigation.ItemPresentation
import com.intellij.openapi.util.NlsSafe
import com.intellij.openapi.util.text.StringUtil
import com.intellij.psi.tree.TokenSet
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinPsiElement
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTokenTypes
import javax.swing.Icon

abstract class GherkinPsiElementBase(node: ASTNode) : ASTWrapperPsiElement(node), GherkinPsiElement {
    open val elementText: String
        get() {
            val node = node
            val children = node.getChildren(TEXT_FILTER)
            return StringUtil.join(children, { astNode: ASTNode -> astNode.text }, " ").trim { it <= ' ' }
        }

    override fun getPresentation(): ItemPresentation? {
        val base = this;
        return object : ItemPresentation {
            override fun getPresentableText(): String? {
                return base.presentableText
            }

            override fun getIcon(open: Boolean): Icon? {
                return this@GherkinPsiElementBase.getIcon(ICON_FLAG_VISIBILITY)
            }
        }
    }

    protected open val presentableText: @NlsSafe String?
        protected get() = toString()

    protected fun buildPresentableText(prefix: String?): String {
        val result = StringBuilder(prefix)
        val name = elementText
        if (!StringUtil.isEmpty(name)) {
            result.append(": ").append(name)
        }
        return result.toString()
    }


    companion object {
        private val TEXT_FILTER = TokenSet.create(GherkinTokenTypes.Companion.TEXT)
    }
}
