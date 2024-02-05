package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

/**
 * @author Roman.Chernyatchik
 */
interface GherkinStepsHolder : GherkinPsiElement, GherkinSuppressionHolder {
    val scenarioName: String
    val steps: Array<GherkinStep?>
    val tags: Array<GherkinTag?>
    val scenarioKeyword: String

    companion object {
        val EMPTY_ARRAY = arrayOfNulls<GherkinStepsHolder>(0)
    }
}
