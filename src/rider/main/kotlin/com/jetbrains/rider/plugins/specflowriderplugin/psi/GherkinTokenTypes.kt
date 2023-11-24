// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.specflowriderplugin.psi

import com.intellij.psi.tree.TokenSet

interface GherkinTokenTypes {
    companion object {
        val COMMENT = GherkinElementType("COMMENT")
        val TEXT = GherkinElementType("TEXT")
        val EXAMPLES_KEYWORD = GherkinElementType("EXAMPLES_KEYWORD")
        val FEATURE_KEYWORD = GherkinElementType("FEATURE_KEYWORD")
        val RULE_KEYWORD = GherkinElementType("RULE_KEYWORD")
        val BACKGROUND_KEYWORD = GherkinElementType("BACKGROUND_KEYWORD")
        val SCENARIO_KEYWORD = GherkinElementType("SCENARIO_KEYWORD")
        val EXAMPLE_KEYWORD = GherkinElementType("EXAMPLE_KEYWORD")
        val SCENARIO_OUTLINE_KEYWORD = GherkinElementType("SCENARIO_OUTLINE_KEYWORD")
        val STEP_KEYWORD = GherkinElementType("STEP_KEYWORD")
        val STEP_PARAMETER_BRACE = GherkinElementType("STEP_PARAMETER_BRACE")
        val STEP_PARAMETER_TEXT = GherkinElementType("STEP_PARAMETER_TEXT")
        val COLON = GherkinElementType("COLON")
        val TAG = GherkinElementType("TAG")
        val PYSTRING = GherkinElementType("PYSTRING_QUOTES")
        val PYSTRING_TEXT = GherkinElementType("PYSTRING_TEXT")
        val PIPE = GherkinElementType("PIPE")
        val TABLE_CELL = GherkinElementType("TABLE_CELL")
        val KEYWORDS = TokenSet.create(
            FEATURE_KEYWORD, RULE_KEYWORD, EXAMPLE_KEYWORD,
            BACKGROUND_KEYWORD, SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD,
            EXAMPLES_KEYWORD, EXAMPLES_KEYWORD,
            STEP_KEYWORD
        )
        val SCENARIOS_KEYWORDS = TokenSet.create(SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD, EXAMPLE_KEYWORD)
        val COMMENTS = TokenSet.create(COMMENT)
    }
}
