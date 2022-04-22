package com.jetbrains.rider.plugins.specflowriderplugin

import com.intellij.lang.Language
import com.intellij.openapi.project.Project
import com.intellij.psi.LanguageSubstitutor
import com.intellij.openapi.vfs.VirtualFile
import com.jetbrains.rider.plugins.specflowriderplugin.ideaInterop.fileTypes.GherkinLanguage
import com.intellij.openapi.vfs.VfsUtilCore
import com.intellij.openapi.vfs.VirtualFileVisitor
import java.util.*

class SpecflowLanguageSubstitutor : LanguageSubstitutor() {
    override fun getLanguage(virtualFile: VirtualFile, project: Project): Language? {
        // FIXME: not sure if this is call ofter or not, it may need some optimization or cache
        if (virtualFile.name.endsWith(".feature")) {
            var parent = virtualFile.parent
            while (parent != null) {
                for (child in parent.children) {
                    if (child.name.endsWith("proj") || child.name.endsWith(".cs")) {
                        return if (findCsharpFile(parent)) {
                            GherkinLanguage
                        } else null
                    }
                }
                if (Arrays.stream(parent.children).anyMatch { f: VirtualFile -> f.name.endsWith(".sln") }) {
                    return null
                }
                parent = parent.parent
            }
        }
        return null
    }

    private fun findCsharpFile(directory: VirtualFile): Boolean {
        val found = booleanArrayOf(false)
        VfsUtilCore.visitChildrenRecursively(directory, object : VirtualFileVisitor<Any?>() {
            override fun visitFile(file: VirtualFile): Boolean {
                if (found[0]) return false
                if (file.name.endsWith(".cs")) {
                    found[0] = true
                    return false
                }
                return super.visitFile(file)
            }
        })
        return found[0]
    }
}