package model.rider

import com.jetbrains.rider.model.nova.ide.SolutionModel
import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*

@Suppress("unused")
object ReqnrollRiderPluginModel : Ext(SolutionModel.Solution) {

    val MyEnum = enum {
        +"FirstValue"
        +"SecondValue"
    }

    val MyStructure = structdef {
        field("projectFile", string)
        field("target", string)
    }

    init {
        //setting(CSharp50Generator.Namespace, "ReSharperPlugin.ReqnrollRiderPlugin.Rider.Model")
        //setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.reqnrollriderplugin.model")

        property("myString", string)
        property("myBool", bool)
        property("myEnum", MyEnum.nullable)

        map("data", string, string)

        signal("myStructure", MyStructure)
    }
}