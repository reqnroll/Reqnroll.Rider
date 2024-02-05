// Copyright 2000-2021 JetBrains s.r.o. Use of this source code is governed by the Apache 2.0 license that can be found in the LICENSE file.
package com.jetbrains.rider.plugins.reqnrollriderplugin.psi

import com.intellij.openapi.fileTypes.LanguageFileType
import com.jetbrains.rider.plugins.reqnrollriderplugin.ReqnrollIcons.Icons.Companion.ReqnrollLogo
import javax.swing.Icon

class GherkinFileType private constructor() : LanguageFileType(GherkinLanguage) {
    override fun getName(): String {
        return "Reqnroll"
    }

    override fun getDescription(): String {
        return "Reqnroll file"
    }

    override fun getDefaultExtension(): String {
        return "feature"
    }

    override fun getIcon(): Icon {
        return ReqnrollLogo
    }

    companion object {
        val INSTANCE = GherkinFileType()
    }
}
