// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.psi.tree.TokenSet
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinElementTypes
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinTable
import com.jetbrains.rider.plugins.reqnrollriderplugin.psi.GherkinTableRow
import kotlin.math.max

class GherkinTableImpl(node: ASTNode) : GherkinPsiElementBase(node), GherkinTable {
    override val headerRow: GherkinTableRow?
        get() {
            val node = node
            val tableNode = node.findChildByType(HEADER_ROW_TOKEN_SET)
            return if (tableNode == null) null else tableNode.psi as GherkinTableRow
        }
    override val dataRows: List<GherkinTableRow?>
        get() {
            val result: MutableList<GherkinTableRow?> = ArrayList()
            val rows = PsiTreeUtil.getChildrenOfType(this, GherkinTableRow::class.java)
            if (rows != null) {
                for (row in rows) {
                    if (row !is GherkinTableHeaderRowImpl) {
                        result.add(row)
                    }
                }
            }
            return result
        }

    override fun getColumnWidth(columnIndex: Int): Int {
        var result = 0
        val headerRow = headerRow
        if (headerRow != null) {
            result = headerRow.getColumnWidth(columnIndex)
        }
        for (row in dataRows) {
            result = max(result.toDouble(), row!!.getColumnWidth(columnIndex).toDouble()).toInt()
        }
        return result
    }

    override fun toString(): String {
        return "GherkinTable"
    }

    companion object {
        private val HEADER_ROW_TOKEN_SET = TokenSet.create(GherkinElementTypes.Companion.TABLE_HEADER_ROW)
    }
}
