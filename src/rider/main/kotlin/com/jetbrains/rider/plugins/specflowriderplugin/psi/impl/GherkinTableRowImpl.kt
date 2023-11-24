// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiWhiteSpace
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTableCell
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTableRow
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinTokenTypes
import java.util.*

open class GherkinTableRowImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinTableRow {
    override fun toString(): String {
        return "GherkinTableRow"
    }

    override val psiCells: List<GherkinTableCell>
        get() = getChildrenByFilter(this, GherkinTableCell::class.java)

    override fun getColumnWidth(columnIndex: Int): Int {
        val cells = psiCells
        if (cells.size <= columnIndex) {
            return 0
        }
        val cell: PsiElement = cells[columnIndex]
        return if (cell != null && cell.text != null) {
            cell.text.trim { it <= ' ' }.length
        } else 0
    }

    override fun deleteCell(columnIndex: Int) {
        val cells = psiCells
        if (columnIndex < cells.size) {
            val cell: PsiElement = cells[columnIndex]
            var nextPipe = cell.nextSibling
            if (nextPipe is PsiWhiteSpace) {
                nextPipe = nextPipe.getNextSibling()
            }
            if (nextPipe != null && nextPipe.node.elementType === GherkinTokenTypes.Companion.PIPE) {
                nextPipe.delete()
            }
            cell.delete()
        }
    }

    companion object {
        // ToDo: Andrey Vokin, remove code duplication
        fun <T : PsiElement?> getChildrenByFilter(psiElement: PsiElement, c: Class<T>): List<T> {
            val list = LinkedList<T>()
            for (element in psiElement.children) {
                if (c.isInstance(element)) {
                    list.add(element as T)
                }
            }
            return if (list.isEmpty()) emptyList() else list
        }
    }
}
