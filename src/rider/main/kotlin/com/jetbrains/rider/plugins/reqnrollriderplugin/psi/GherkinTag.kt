package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

interface GherkinTag : GherkinPsiElement {
    val tagName: String?

    companion object {
        val EMPTY_ARRAY = arrayOfNulls<GherkinTag>(0)
    }
}
