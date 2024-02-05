@file:Suppress("EXPERIMENTAL_API_USAGE","EXPERIMENTAL_UNSIGNED_LITERALS","PackageDirectoryMismatch","UnusedImport","unused","LocalVariableName","CanBeVal","PropertyName","EnumEntryName","ClassName","ObjectPropertyName","UnnecessaryVariable","SpellCheckingInspection")
package com.jetbrains.rd.ide.model

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.time.Duration
import kotlin.reflect.KClass
import kotlin.jvm.JvmStatic



/**
 * #### Generated from [ReqnrollRiderPluginModel.kt:8]
 */
class ReqnrollRiderPluginModel private constructor(
    private val _myString: RdOptionalProperty<String>,
    private val _myBool: RdOptionalProperty<Boolean>,
    private val _myEnum: RdProperty<MyEnum?>,
    private val _data: RdMap<String, String>,
    private val _myStructure: RdSignal<MyStructure>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers)  {
            serializers.register(MyEnum.marshaller)
            serializers.register(MyStructure)
        }
        
        
        
        
        private val __MyEnumNullableSerializer = MyEnum.marshaller.nullable()
        
        const val serializationHash = 652449028105363203L
        
    }
    override val serializersOwner: ISerializersOwner get() = ReqnrollRiderPluginModel
    override val serializationHash: Long get() = ReqnrollRiderPluginModel.serializationHash
    
    //fields
    val myString: IOptProperty<String> get() = _myString
    val myBool: IOptProperty<Boolean> get() = _myBool
    val myEnum: IProperty<MyEnum?> get() = _myEnum
    val `data`: IMutableViewableMap<String, String> get() = _data
    val myStructure: ISignal<MyStructure> get() = _myStructure
    //methods
    //initializer
    init {
        _myString.optimizeNested = true
        _myBool.optimizeNested = true
        _myEnum.optimizeNested = true
        _data.optimizeNested = true
    }
    
    init {
        bindableChildren.add("myString" to _myString)
        bindableChildren.add("myBool" to _myBool)
        bindableChildren.add("myEnum" to _myEnum)
        bindableChildren.add("data" to _data)
        bindableChildren.add("myStructure" to _myStructure)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdOptionalProperty<String>(FrameworkMarshallers.String),
        RdOptionalProperty<Boolean>(FrameworkMarshallers.Bool),
        RdProperty<MyEnum?>(null, __MyEnumNullableSerializer),
        RdMap<String, String>(FrameworkMarshallers.String, FrameworkMarshallers.String),
        RdSignal<MyStructure>(MyStructure)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("ReqnrollRiderPluginModel (")
        printer.indent {
            print("myString = "); _myString.print(printer); println()
            print("myBool = "); _myBool.print(printer); println()
            print("myEnum = "); _myEnum.print(printer); println()
            print("data = "); _data.print(printer); println()
            print("myStructure = "); _myStructure.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): ReqnrollRiderPluginModel   {
        return ReqnrollRiderPluginModel(
            _myString.deepClonePolymorphic(),
            _myBool.deepClonePolymorphic(),
            _myEnum.deepClonePolymorphic(),
            _data.deepClonePolymorphic(),
            _myStructure.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val Solution.reqnrollRiderPluginModel get() = getOrCreateExtension("reqnrollRiderPluginModel", ::ReqnrollRiderPluginModel)



/**
 * #### Generated from [ReqnrollRiderPluginModel.kt:10]
 */
enum class MyEnum {
    FirstValue, 
    SecondValue;
    
    companion object {
        val marshaller = FrameworkMarshallers.enum<MyEnum>()
        
    }
}


/**
 * #### Generated from [ReqnrollRiderPluginModel.kt:15]
 */
data class MyStructure (
    val projectFile: String,
    val target: String
) : IPrintable {
    //companion
    
    companion object : IMarshaller<MyStructure> {
        override val _type: KClass<MyStructure> = MyStructure::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MyStructure  {
            val projectFile = buffer.readString()
            val target = buffer.readString()
            return MyStructure(projectFile, target)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MyStructure)  {
            buffer.writeString(value.projectFile)
            buffer.writeString(value.target)
        }
        
        
    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as MyStructure
        
        if (projectFile != other.projectFile) return false
        if (target != other.target) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + projectFile.hashCode()
        __r = __r*31 + target.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MyStructure (")
        printer.indent {
            print("projectFile = "); projectFile.print(printer); println()
            print("target = "); target.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
