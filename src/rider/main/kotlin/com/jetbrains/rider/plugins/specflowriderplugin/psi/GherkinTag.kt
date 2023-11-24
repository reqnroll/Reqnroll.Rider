package com.jetbrains.rider.plugins.specflowriderplugin.psi

interface GherkinTag : GherkinPsiElement {
    val tagName: String?

    companion object {
        val EMPTY_ARRAY = arrayOfNulls<GherkinTag>(0)
    }
}
