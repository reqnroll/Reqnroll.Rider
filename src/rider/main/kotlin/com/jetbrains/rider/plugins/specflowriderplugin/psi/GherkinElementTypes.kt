// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.TokenSet

interface GherkinElementTypes {
    companion object {
        val FEATURE: IElementType = GherkinElementType("feature")
        val FEATURE_HEADER: IElementType = GherkinElementType("feature header")
        val SCENARIO: IElementType = GherkinElementType("scenario")
        val STEP: IElementType = GherkinElementType("step")
        val STEP_PARAMETER: IElementType = GherkinElementType("step parameter")
        val SCENARIO_OUTLINE: IElementType = GherkinElementType("scenario outline")
        val RULE: IElementType = GherkinElementType("rule")
        val EXAMPLES_BLOCK: IElementType = GherkinElementType("examples block")
        val TABLE: IElementType = GherkinElementType("table")
        val TABLE_HEADER_ROW: IElementType = GherkinElementType("table header row")
        val TABLE_ROW: IElementType = GherkinElementType("table row")
        val TABLE_CELL: IElementType = GherkinElementType("table cell")
        val TAG: IElementType = GherkinElementType("tag")
        val PYSTRING: IElementType = GherkinElementType("pystring")
        val SCENARIOS = TokenSet.create(SCENARIO, SCENARIO_OUTLINE)
    }
}
