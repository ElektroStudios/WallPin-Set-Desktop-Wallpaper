Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection

''' <summary>
''' Provides reflection helpers for inspecting WallPin's non-public implementation
''' without changing the production assembly's visibility.
''' </summary>
Friend Module ReflectionTestSupport

#Region " Constants "

    ''' <summary>
    ''' The simple name of the production assembly.
    ''' </summary>
    Private Const ProductionAssemblyName As String = "WallPin"

    ''' <summary>
    ''' The full name of the production module that contains the entry point.
    ''' </summary>
    Friend Const ProgramTypeName As String = "Program"

    ''' <summary>
    ''' The full name of the production wallpaper configuration class.
    ''' </summary>
    Friend Const WallpaperConfigTypeName As String = "WallpaperConfig"

    ''' <summary>
    ''' Binding flags that match any instance member, regardless of its visibility.
    ''' </summary>
    Friend Const AnyInstance As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic

    ''' <summary>
    ''' Binding flags that match any static member, regardless of its visibility.
    ''' </summary>
    Friend Const AnyStatic As BindingFlags = BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic

#End Region

#Region " Assembly and Type Helpers "

    ''' <summary>
    ''' Gets the production WallPin assembly.
    ''' </summary>
    '''
    ''' <returns>
    ''' The loaded production assembly.
    ''' </returns>
    Friend Function GetProductionAssembly() As Assembly

        Return Assembly.Load(ProductionAssemblyName)
    End Function

    ''' <summary>
    ''' Gets a type from the production WallPin assembly.
    ''' </summary>
    '''
    ''' <param name="typeName">
    ''' The fully qualified type name.
    ''' </param>
    '''
    ''' <returns>
    ''' The requested production type.
    ''' </returns>
    Friend Function GetProductionType(typeName As String) As Type

        Return GetProductionAssembly().GetType(typeName, throwOnError:=True, ignoreCase:=False)
    End Function

    ''' <summary>
    ''' Gets a static field value from a production type.
    ''' </summary>
    '''
    ''' <param name="typeName">
    ''' The fully qualified declaring type name.
    ''' </param>
    '''
    ''' <param name="fieldName">
    ''' The field name.
    ''' </param>
    '''
    ''' <returns>
    ''' The field value.
    ''' </returns>
    '''
    ''' <exception cref="MissingFieldException">
    ''' Thrown when the field does not exist.
    ''' </exception>
    Friend Function GetStaticFieldValue(typeName As String, fieldName As String) As Object

        Dim field As FieldInfo = GetProductionType(typeName).GetField(fieldName, AnyStatic)
        If field Is Nothing Then
            Throw New MissingFieldException(typeName, fieldName)
        End If

        Return field.GetValue(Nothing)
    End Function

    ''' <summary>
    ''' Gets a method from a production type, regardless of its visibility.
    ''' </summary>
    '''
    ''' <param name="typeName">
    ''' The fully qualified declaring type name.
    ''' </param>
    '''
    ''' <param name="methodName">
    ''' The method name.
    ''' </param>
    '''
    ''' <returns>
    ''' The method metadata.
    ''' </returns>
    '''
    ''' <exception cref="MissingMethodException">
    ''' Thrown when the method does not exist.
    ''' </exception>
    Friend Function GetProductionMethod(typeName As String, methodName As String) As MethodInfo

        Dim method As MethodInfo = GetProductionType(typeName).GetMethod(methodName, AnyInstance Or AnyStatic)
        If method Is Nothing Then
            Throw New MissingMethodException(typeName, methodName)
        End If

        Return method
    End Function

#End Region

#Region " Program Helpers "

    ''' <summary>
    ''' Gets the production <c>Program.SupportedFileExtensions</c> collection, as is
    ''' (its own equality comparer is preserved).
    ''' </summary>
    '''
    ''' <returns>
    ''' The production collection of supported file extensions.
    ''' </returns>
    Friend Function GetSupportedFileExtensions() As HashSet(Of String)

        Return DirectCast(GetStaticFieldValue(ProgramTypeName, "SupportedFileExtensions"), HashSet(Of String))
    End Function

    ''' <summary>
    ''' Gets the production <c>Program.StyleMapping</c> dictionary, as is
    ''' (its own key comparer is preserved).
    ''' </summary>
    '''
    ''' <returns>
    ''' The production style mapping, as a non-generic dictionary,
    ''' because its value type (<c>WallpaperConfig</c>) is not public.
    ''' </returns>
    Friend Function GetStyleMapping() As IDictionary

        Return DirectCast(GetStaticFieldValue(ProgramTypeName, "StyleMapping"), IDictionary)
    End Function

    ''' <summary>
    ''' Looks up a wallpaper style identifier in the production style mapping,
    ''' using the dictionary's own key comparer (the same lookup performed by <c>Program.Main</c>).
    ''' </summary>
    '''
    ''' <param name="identifier">
    ''' The style identifier to look up.
    ''' </param>
    '''
    ''' <param name="refConfig">
    ''' When this method returns <see langword="True"/>, contains the mapped <c>WallpaperConfig</c>
    ''' instance (or <see langword="Nothing"/> for the "default" identifier).
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the identifier is mapped; otherwise, <see langword="False"/>.
    ''' </returns>
    Friend Function TryGetStyle(identifier As String, ByRef refConfig As Object) As Boolean

        Dim styleMapping As Object = GetStaticFieldValue(ProgramTypeName, "StyleMapping")
        Dim tryGetValue As MethodInfo = styleMapping.GetType().GetMethod("TryGetValue")
        Dim arguments As Object() = {identifier, Nothing}
        Dim found As Boolean = CBool(tryGetValue.Invoke(styleMapping, arguments))

        refConfig = arguments(1)
        Return found
    End Function

#End Region

#Region " WallpaperConfig Helpers "

    ''' <summary>
    ''' Creates a production <c>WallpaperConfig</c> instance through its non-public constructor.
    ''' </summary>
    '''
    ''' <param name="style">
    ''' The registry <c>WallpaperStyle</c> value.
    ''' </param>
    '''
    ''' <param name="tile">
    ''' The registry <c>TileWallpaper</c> value.
    ''' </param>
    '''
    ''' <returns>
    ''' The new <c>WallpaperConfig</c> instance.
    ''' </returns>
    '''
    ''' <exception cref="MissingMethodException">
    ''' Thrown when the (String, String) constructor does not exist.
    ''' </exception>
    Friend Function CreateWallpaperConfig(style As String, tile As String) As Object

        Dim configType As Type = GetProductionType(WallpaperConfigTypeName)
        Dim constructor As ConstructorInfo = configType.GetConstructor(
            AnyInstance, binder:=Nothing, types:=New Type() {GetType(String), GetType(String)}, modifiers:=Nothing)

        If constructor Is Nothing Then
            Throw New MissingMethodException(WallpaperConfigTypeName, ".ctor(String, String)")
        End If

        Return constructor.Invoke(New Object() {style, tile})
    End Function

    ''' <summary>
    ''' Gets the <c>WallpaperStyle</c> field value of a <c>WallpaperConfig</c> instance.
    ''' </summary>
    '''
    ''' <param name="config">
    ''' The <c>WallpaperConfig</c> instance.
    ''' </param>
    '''
    ''' <returns>
    ''' The registry <c>WallpaperStyle</c> value.
    ''' </returns>
    Friend Function GetWallpaperStyle(config As Object) As String

        Return CStr(GetConfigField("WallpaperStyle").GetValue(config))
    End Function

    ''' <summary>
    ''' Gets the <c>TileWallpaper</c> field value of a <c>WallpaperConfig</c> instance.
    ''' </summary>
    '''
    ''' <param name="config">
    ''' The <c>WallpaperConfig</c> instance.
    ''' </param>
    '''
    ''' <returns>
    ''' The registry <c>TileWallpaper</c> value.
    ''' </returns>
    Friend Function GetTileWallpaper(config As Object) As String

        Return CStr(GetConfigField("TileWallpaper").GetValue(config))
    End Function

    ''' <summary>
    ''' Invokes <c>WallpaperConfig.ToIDesktopWallpaperPosition</c> on an instance.
    ''' </summary>
    '''
    ''' <param name="config">
    ''' The <c>WallpaperConfig</c> instance.
    ''' </param>
    '''
    ''' <returns>
    ''' The native <c>DESKTOP_WALLPAPER_POSITION</c> value.
    ''' </returns>
    Friend Function GetDesktopWallpaperPosition(config As Object) As Integer

        Dim method As MethodInfo = GetProductionMethod(WallpaperConfigTypeName, "ToIDesktopWallpaperPosition")
        Return CInt(method.Invoke(config, Nothing))
    End Function

    ''' <summary>
    ''' Gets a <c>WallpaperConfig</c> instance field, regardless of its visibility.
    ''' </summary>
    '''
    ''' <param name="fieldName">
    ''' The field name.
    ''' </param>
    '''
    ''' <returns>
    ''' The field metadata.
    ''' </returns>
    '''
    ''' <exception cref="MissingFieldException">
    ''' Thrown when the field does not exist.
    ''' </exception>
    Friend Function GetConfigField(fieldName As String) As FieldInfo

        Dim field As FieldInfo = GetProductionType(WallpaperConfigTypeName).GetField(fieldName, AnyInstance)
        If field Is Nothing Then
            Throw New MissingFieldException(WallpaperConfigTypeName, fieldName)
        End If

        Return field
    End Function

#End Region

End Module
