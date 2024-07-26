package com.jetbrains.rider.plugins.reqnrollriderplugin.ideaInterop.fileTypes

import com.intellij.psi.PsiElement
import com.intellij.refactoring.actions.ExtractMethodAction
import com.jetbrains.rider.actions.RiderActionSupportPolicy
import java.io.File
import java.io.FileOutputStream
import java.io.OutputStream


class GherkinActionSupportPolicy : RiderActionSupportPolicy() {
    override fun isAvailable(psiElement: PsiElement, backendActionId: String): Boolean? {
        if (backendActionId == "ImplementMethods") return false;
        if (backendActionId == "OverrideMethods") return false;
        if (backendActionId == "ShowUsages") return false;
        if (backendActionId == "FindUsages") return false;
        if (backendActionId == "SafeDelete") return false;
        if (backendActionId == "Rename") return false;
        if (backendActionId == "ChangeSignature") return false;

        if (backendActionId == "ExtractMethod") return false;
        if (backendActionId == "ExtractInterface") return false;
        if (backendActionId == "ExtractSuperclass") return false;

        if (backendActionId == "PullUp") return false;
        if (backendActionId == "PushDown") return false;

        if (backendActionId == "RefactorThis") return false;

        if (backendActionId == "InlineVariable") return false;
        if (backendActionId == "IntroduceVariable") return false;
        if (backendActionId == "IntroVariable") return false;
        if (backendActionId == "IntroduceField") return false;
        if (backendActionId == "IntroduceParameter") return false;

        if (backendActionId == "GotoInheritors") return false;
        if (backendActionId == "GotoImplementations") return false;
        if (backendActionId == "GotoTypeDeclaration") return false;
        if (backendActionId == "GotoBase") return false;


        val result = super.isAvailable(psiElement, backendActionId);
        return result;
    }
}