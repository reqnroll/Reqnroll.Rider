package com.jetbrains.rider.plugins.specflowriderplugin;

import com.intellij.lang.Language;
import com.intellij.openapi.project.Project;
import com.intellij.openapi.vfs.VfsUtilCore;
import com.intellij.openapi.vfs.VirtualFile;
import com.intellij.openapi.vfs.VirtualFileVisitor;
import com.intellij.psi.LanguageSubstitutor;
import com.jetbrains.rider.plugins.specflowriderplugin.ideaInterop.fileTypes.GherkinLanguage;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

import java.util.Arrays;

public class SpecflowLanguageSubstitutor extends LanguageSubstitutor {
    @Override
    public @Nullable Language getLanguage(@NotNull VirtualFile virtualFile, @NotNull Project project) {
        // FIXME: not sure if this is call ofter or not, it may need some optimization or cache
        if (virtualFile.getName().endsWith(".feature")) {
            var parent = virtualFile.getParent();
            while (parent != null) {
                for (VirtualFile child : parent.getChildren()) {
                    if (child.getName().endsWith("proj") || child.getName().endsWith(".cs")) {
                        if (findCsharpFile(parent)) {
                            return GherkinLanguage.INSTANCE;
                        }
                        return null;
                    }
                }
                if (Arrays.stream(parent.getChildren()).anyMatch(f -> f.getName().endsWith(".sln"))) {
                    return null;
                }
                parent = parent.getParent();
            }
        }
        return null;
    }

    private boolean findCsharpFile(VirtualFile directory) {
        final boolean[] found = {false};
        VfsUtilCore.visitChildrenRecursively(directory, new VirtualFileVisitor<Object>() {
            @Override
            public boolean visitFile(@NotNull VirtualFile file) {
                if (found[0])
                    return false;
                if (file.getName().endsWith(".cs")) {
                    found[0] = true;
                    return false;
                }
                return super.visitFile(file);
            }
        });
        return found[0];
    }
}
