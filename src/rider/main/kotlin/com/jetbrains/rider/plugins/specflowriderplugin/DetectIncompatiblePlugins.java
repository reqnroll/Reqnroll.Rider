package com.jetbrains.rider.plugins.specflowriderplugin;

import com.intellij.ide.plugins.IdeaPluginDescriptor;
import com.intellij.ide.plugins.PluginManagerCore;
import com.intellij.notification.Notification;
import com.intellij.notification.NotificationAction;
import com.intellij.notification.NotificationGroupManager;
import com.intellij.notification.NotificationType;
import com.intellij.openapi.actionSystem.AnActionEvent;
import com.intellij.openapi.application.ApplicationManager;
import com.intellij.openapi.project.Project;
import com.intellij.openapi.startup.StartupActivity;
import org.jetbrains.annotations.NotNull;

import java.util.Arrays;
import java.util.Optional;

public class DetectIncompatiblePlugins implements StartupActivity.DumbAware {

    @Override
    public void runActivity(@NotNull Project project) {
        Optional<@NotNull IdeaPluginDescriptor> cucumberPlugin = Arrays.stream(PluginManagerCore.getPlugins())
                .filter(p -> p.getPluginId().getIdString().equals("cucumber-javascript"))
                .findFirst();
        Optional<@NotNull IdeaPluginDescriptor> gherkinPlugin = Arrays.stream(PluginManagerCore.getPlugins())
                .filter(p -> p.getPluginId().getIdString().equals("gherkin"))
                .findFirst();

        if ((cucumberPlugin.isPresent() && cucumberPlugin.get().isEnabled()) || (gherkinPlugin.isPresent() && gherkinPlugin.get().isEnabled())) {
            StringBuilder content = new StringBuilder();
            content.append("SpecFlow: Incompatible plugin detected\n");
            content.append("<ul>");
            cucumberPlugin.ifPresent(ideaPluginDescriptor -> content.append("<li>").append(ideaPluginDescriptor.getName()).append("</li>\n"));
            gherkinPlugin.ifPresent(ideaPluginDescriptor -> content.append("<li>").append(ideaPluginDescriptor.getName()).append("</li>\n"));
            content.append("</ul>");
            NotificationGroupManager.getInstance().getNotificationGroup("SpecFlow")
                    .createNotification(content.toString(), NotificationType.ERROR)
                    .addAction(new NotificationAction("Disable plugins and restart") {
                        @Override
                        public void actionPerformed(@NotNull AnActionEvent anActionEvent, @NotNull Notification notification) {
                            {
                                cucumberPlugin.ifPresent(ideaPluginDescriptor -> PluginManagerCore.disablePlugin(ideaPluginDescriptor.getPluginId()));
                                gherkinPlugin.ifPresent(ideaPluginDescriptor -> PluginManagerCore.disablePlugin(ideaPluginDescriptor.getPluginId()));
                                notification.expire();
                                ApplicationManager.getApplication().restart();
                            }
                        }
                    })
                    .notify(project);
        }
    }
}
