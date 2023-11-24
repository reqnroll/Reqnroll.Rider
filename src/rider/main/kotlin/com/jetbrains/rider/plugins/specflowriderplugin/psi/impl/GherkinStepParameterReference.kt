package com.jetbrains.rider.plugins.specflowriderplugin.psi.impl

import com.intellij.psi.PsiElement
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.plugins.specflowriderplugin.psi.*

class GherkinStepParameterReference(stepParameter: GherkinStepParameter) : GherkinSimpleReference(stepParameter) {
    override fun getElement(): GherkinStepParameter {
        return super.getElement() as GherkinStepParameter
    }

    override fun resolve(): PsiElement? {
        val scenario = PsiTreeUtil.getParentOfType(element, GherkinScenarioOutline::class.java)
            ?: return null
        val exampleBlock = PsiTreeUtil.getChildOfType(scenario, GherkinExamplesBlock::class.java)
            ?: return null
        val table = PsiTreeUtil.getChildOfType(exampleBlock, GherkinTable::class.java) ?: return null
        val header = PsiTreeUtil.getChildOfType(table, GherkinTableHeaderRowImpl::class.java)
            ?: return null
        for (cell in header.children) {
            if (cell is GherkinTableCell) {
                val cellText = cell.getText()
                if (cellText == element.name) {
                    return cell
                }
            }
        }
        return null
    }
}
