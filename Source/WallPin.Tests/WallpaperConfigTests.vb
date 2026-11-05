Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the wallpaper configuration conversion behavior.
''' </summary>
<TestClass>
Public NotInheritable Class WallpaperConfigTests

    ''' <summary>
    ''' Verifies the documented fallback for an unrecognized registry style value.
    ''' </summary>
    <TestMethod>
    Public Sub UnknownRegistryStyleFallsBackToCenterPosition()
        Dim configType As Type = GetProductionType("WallpaperConfig")
        Dim constructorFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic
        Dim constructor As ConstructorInfo = configType.GetConstructor(
            constructorFlags,
            binder:=Nothing,
            types:=New Type() {GetType(String), GetType(String)},
            modifiers:=Nothing)

        Assert.IsNotNull(constructor, "WallpaperConfig constructor was not found.")

        Dim config As Object = constructor.Invoke(New Object() {"999", "0"})
        Dim method As MethodInfo = configType.GetMethod("ToIDesktopWallpaperPosition", constructorFlags)

        Assert.IsNotNull(method, "ToIDesktopWallpaperPosition method was not found.")
        Assert.AreEqual(0, CInt(method.Invoke(config, Nothing)))
    End Sub

End Class
