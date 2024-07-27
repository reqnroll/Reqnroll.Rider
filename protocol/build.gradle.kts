import com.jetbrains.rd.generator.gradle.RdGenTask

plugins {
    alias(libs.plugins.kotlinJvm)
    id("com.jetbrains.rdgen") version libs.versions.rdGen
}

dependencies {
    implementation(libs.rdGen)
    implementation(libs.kotlinStdLib)
    implementation(
        project(
            mapOf(
                "path" to ":",
                "configuration" to "riderModel"
            )
        )
    )
}

val dotNetPluginId: String by project
val riderPluginId: String by project

rdgen {
    val csOutput = file("../src/dotnet/${dotNetPluginId}/Model").absolutePath
    val ktOutput = file("../src/rider/generated/kotlin/${riderPluginId.replace('.','/').lowercase()}").absolutePath

    verbose = true
    packages = "model"

    generator {
        language = "kotlin"
        transform = "asis"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "$riderPluginId.model"
        directory = ktOutput.toString()
        generatedFileSuffix = ".Generated"
    }

    generator {
        language = "csharp"
        transform = "reversed"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "$dotNetPluginId.Model"
        directory = csOutput.toString()
        generatedFileSuffix = ".Generated"
    }
}

tasks.withType<RdGenTask> {
    val classPath = sourceSets["main"].runtimeClasspath
    dependsOn(classPath)
    classpath(classPath)
}
