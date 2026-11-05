Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the Win32 and COM declarations without invoking native desktop operations.
''' </summary>
<TestClass>
Public NotInheritable Class Win32InteropContractTests

    Private Const InstanceAndStaticFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic

    ''' <summary>
    ''' Verifies the Win32 constants used by the wallpaper-setting implementations.
    ''' </summary>
    <TestMethod>
    Public Sub NativeConstantsMatchTheWin32Contract()
        Assert.AreEqual(260, CInt(GetStaticFieldValue("Win32.Constants", "MAX_PATH")))
        Assert.AreEqual(20, CInt(GetStaticFieldValue("Win32.Constants", "SPI_SETDESKWALLPAPER")))
        Assert.AreEqual(1, CInt(GetStaticFieldValue("Win32.Constants", "SPIF_UPDATEINIFILE")))
        Assert.AreEqual(2, CInt(GetStaticFieldValue("Win32.Constants", "SPIF_SENDCHANGE")))
    End Sub

    ''' <summary>
    ''' Verifies the SystemParametersInfo P/Invoke metadata and string marshaling.
    ''' </summary>
    <TestMethod>
    Public Sub SystemParametersInfoUsesTheExpectedPInvokeSignature()
        Dim nativeMethodsType As Type = GetProductionType("Win32.NativeMethods")
        Dim method As MethodInfo = nativeMethodsType.GetMethod("SystemParametersInfo", InstanceAndStaticFlags)

        Assert.IsNotNull(method, "SystemParametersInfo declaration was not found.")

        Dim importAttribute As DllImportAttribute = DirectCast(
            Attribute.GetCustomAttribute(method, GetType(DllImportAttribute)),
            DllImportAttribute)

        Assert.IsNotNull(importAttribute, "DllImportAttribute was not found.")
        Assert.IsTrue(String.Equals("user32.dll", importAttribute.Value, StringComparison.OrdinalIgnoreCase))
        Assert.AreEqual("SystemParametersInfoW", importAttribute.EntryPoint)
        Assert.AreEqual(CharSet.Unicode, importAttribute.CharSet)
        Assert.IsTrue(importAttribute.SetLastError)
        Assert.HasCount(4, method.GetParameters())

        Dim wallpaperPathParameter As ParameterInfo = method.GetParameters()(2)
        Dim marshalAttribute As MarshalAsAttribute = DirectCast(
            Attribute.GetCustomAttribute(wallpaperPathParameter, GetType(MarshalAsAttribute)),
            MarshalAsAttribute)

        Assert.IsNotNull(marshalAttribute, "The wallpaper path must have an explicit marshaling attribute.")
        Assert.AreEqual(UnmanagedType.LPWStr, marshalAttribute.Value)
    End Sub

    ''' <summary>
    ''' Verifies the COM interface identity and the native vtable method order.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperInterfacePreservesItsComIdentityAndVTableOrder()
        Dim interfaceType As Type = GetProductionType("Win32.IDesktopWallpaper")

        Assert.IsTrue(interfaceType.IsInterface)
        Assert.IsTrue(interfaceType.IsImport, "The interface must be marked as a COM import.")

        Dim guidAttribute As GuidAttribute = DirectCast(
            Attribute.GetCustomAttribute(interfaceType, GetType(GuidAttribute)),
            GuidAttribute)
        Dim interfaceTypeAttribute As InterfaceTypeAttribute = DirectCast(
            Attribute.GetCustomAttribute(interfaceType, GetType(InterfaceTypeAttribute)),
            InterfaceTypeAttribute)

        Assert.IsNotNull(guidAttribute)
        Assert.IsTrue(String.Equals("B92B56A9-8B55-4E14-9A89-0199BBB6F93B", guidAttribute.Value, StringComparison.OrdinalIgnoreCase))
        Assert.IsNotNull(interfaceTypeAttribute)
        Assert.AreEqual(ComInterfaceType.InterfaceIsIUnknown, interfaceTypeAttribute.Value)

        Dim methods As MethodInfo() = interfaceType.GetMethods()
        Array.Sort(Of MethodInfo)(
            methods,
            Function(left As MethodInfo, right As MethodInfo) left.MetadataToken.CompareTo(right.MetadataToken))

        Dim actualMethodNames As New List(Of String)()
        For Each method As MethodInfo In methods
            actualMethodNames.Add(method.Name)
        Next

        Dim expectedMethodNames As String() = {
            "SetWallpaper",
            "GetWallpaper",
            "GetMonitorDevicePathAt",
            "GetMonitorDevicePathCount",
            "GetMonitorRECT",
            "SetBackgroundColor",
            "GetBackgroundColor",
            "SetPosition",
            "GetPosition",
            "SetSlideshow",
            "GetSlideshow",
            "SetSlideshowOptions",
            "GetSlideshowOptions",
            "AdvanceSlideshow",
            "GetStatus",
            "Enable"
        }

        Assert.AreSequenceEqual(expectedMethodNames, actualMethodNames.ToArray())

        Dim setWallpaperMethod As MethodInfo = interfaceType.GetMethod("SetWallpaper")
        Assert.IsNotNull(setWallpaperMethod)
        Assert.HasCount(2, setWallpaperMethod.GetParameters())

        For Each parameter As ParameterInfo In setWallpaperMethod.GetParameters()
            Dim marshalAttribute As MarshalAsAttribute = DirectCast(
                Attribute.GetCustomAttribute(parameter, GetType(MarshalAsAttribute)),
                MarshalAsAttribute)
            Assert.IsNotNull(marshalAttribute, $"Missing string marshaling on parameter '{parameter.Name}'.")
            Assert.AreEqual(UnmanagedType.LPWStr, marshalAttribute.Value)
        Next

        Dim monitorRectMethod As MethodInfo = interfaceType.GetMethod("GetMonitorRECT")
        Assert.IsNotNull(monitorRectMethod)
        Dim rectangleParameterType As Type = monitorRectMethod.GetParameters()(1).ParameterType
        Assert.IsTrue(rectangleParameterType.IsByRef)
        Assert.AreEqual(GetType(IntPtr), rectangleParameterType.GetElementType())
    End Sub

    ''' <summary>
    ''' Verifies the COM activation class identifier.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperCoclassHasTheExpectedClassIdentifier()
        Dim coclassType As Type = GetProductionType("Win32.CDesktopWallpaper")

        Assert.IsTrue(coclassType.IsClass)
        Assert.IsTrue(coclassType.IsImport, "The coclass must be marked as a COM import.")

        Dim guidAttribute As GuidAttribute = DirectCast(
            Attribute.GetCustomAttribute(coclassType, GetType(GuidAttribute)),
            GuidAttribute)

        Assert.IsNotNull(guidAttribute)
        Assert.IsTrue(String.Equals("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD", guidAttribute.Value, StringComparison.OrdinalIgnoreCase))
    End Sub

End Class
