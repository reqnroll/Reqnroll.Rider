package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.lang.ASTNode
import com.intellij.openapi.util.NlsSafe
import com.intellij.psi.tree.TokenSet
import com.intellij.psi.util.CachedValueProvider
import com.intellij.psi.util.CachedValuesManager
import com.intellij.psi.util.PsiModificationTracker
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinElementTypes
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinExamplesBlock
import com.jetbrains.rider.plugins.specflowriderplugin.psi.GherkinScenarioOutline

open class GherkinScenarioOutlineImpl(node: ASTNode) : GherkinStepsHolderBase(node), GherkinScenarioOutline {
    override fun toString(): String {
        return "GherkinScenarioOutline:$elementText"
    }

    override val presentableText: @NlsSafe String?
        protected get() = buildPresentableText("Scenario Outline")

    override val examplesBlocks: List<GherkinExamplesBlock>
        get() {
            val result: MutableList<GherkinExamplesBlock> = ArrayList()
            val nodes = node.getChildren(EXAMPLES_BLOCK_FILTER)
            for (node in nodes) {
                result.add(node.psi as GherkinExamplesBlock)
            }
            return result
        }
    override val outlineTableMap: Map<String, String>?
        get() = CachedValuesManager
            .getCachedValue(this) {
                CachedValueProvider.Result.create(
                    buildOutlineTableMap(this),
                    PsiModificationTracker.MODIFICATION_COUNT
                )
            }

    companion object {
        private val EXAMPLES_BLOCK_FILTER = TokenSet.create(GherkinElementTypes.EXAMPLES_BLOCK)
        private fun buildOutlineTableMap(scenarioOutline: GherkinScenarioOutline?): Map<String, String>? {
            if (scenarioOutline == null) {
                return null
            }
            val examplesBlocks = scenarioOutline.examplesBlocks
            for (examplesBlock in examplesBlocks) {
                val table = examplesBlock.table
                if (table?.headerRow == null || table.dataRows.isEmpty()) {
                    continue
                }
                val headerCells = table.headerRow!!.psiCells
                val dataCells = table.dataRows[0]?.psiCells
                val result: MutableMap<String, String> = HashMap()
                for (i in headerCells.indices) {
                    if (i >= dataCells!!.size) {
                        break
                    }
                    result[headerCells[i].text.trim { it <= ' ' }] = dataCells!![i]!!.text.trim { it <= ' ' }
                }
                return result
            }
            return null
        }
    }
}
