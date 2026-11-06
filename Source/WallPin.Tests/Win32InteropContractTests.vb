Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the Win32 and COM declarations without invoking native desktop operations.
''' </summary>
'''
''' <remarks>
''' A COM interface declared with <see cref="ComInterfaceType.InterfaceIsIUnknown"/> is called through
''' its vtable: the member order, the parameter types (their native size), and the
''' <see cref="PreserveSigAttribute"/> flags must all match the native header exactly,
''' or calls silently land in the wrong slot or corrupt the stack.
''' </remarks>
<TestClass>
Public NotInheritable Class Win32InteropContractTests

#Region " Constants "

    ''' <summary>
    ''' The full name of the production <c>IDesktopWallpaper</c> interface.
    ''' </summary>
    Private Const DesktopWallpaperInterfaceTypeName As String = "Win32.IDesktopWallpaper"

    ''' <summary>
    ''' The expected <c>IDesktopWallpaper</c> member signatures, in native vtable order
    ''' (after the three <c>IUnknown</c> slots), as declared in <c>shobjidl_core.h</c>.
    ''' </summary>
    Private Shared ReadOnly ExpectedVTable As String() = {
        "SetWallpaper(String, String) As Void",
        "GetWallpaper(String, ByRef String) As Int32",
        "GetMonitorDevicePathAt(UInt32, ByRef String) As Void",
        "GetMonitorDevicePathCount(ByRef UInt32) As Int32",
        "GetMonitorRECT(String, ByRef IntPtr) As Int32",
        "SetBackgroundColor(UInt32) As Void",
        "GetBackgroundColor(ByRef UInt32) As Int32",
        "SetPosition(Int32) As Void",
        "GetPosition(ByRef Int32) As Int32",
        "SetSlideshow(IntPtr) As Void",
        "GetSlideshow(ByRef IntPtr) As Int32",
        "SetSlideshowOptions(Int32, UInt32) As Void",
        "GetSlideshowOptions(ByRef Int32, ByRef UInt32) As Int32",
        "AdvanceSlideshow(String, Int32) As Void",
        "GetStatus(ByRef Int32) As Int32",
        "Enable(Boolean) As Int32"
    }

    ''' <summary>
    ''' The <c>IDesktopWallpaper</c> members declared with <see cref="PreserveSigAttribute"/>
    ''' (they return the raw HRESULT instead of throwing a <see cref="COMException"/>).
    ''' </summary>
    Private Shared ReadOnly ExpectedPreserveSigMembers As String() = {
        "GetWallpaper",
        "GetMonitorDevicePathCount",
        "GetMonitorRECT",
        "GetBackgroundColor",
        "GetPosition",
        "GetSlideshow",
        "GetSlideshowOptions",
        "GetStatus",
        "Enable"
    }

#End Region

#Region " Win32 Constants and P/Invoke "

    ''' <summary>
    ''' Verifies the Win32 constants used by the wallpaper-setting implementations.
    ''' </summary>
    <TestMethod>
    Public Sub NativeConstantsMatchTheWin32Contract()

        Assert.AreEqual(260, CInt(GetStaticFieldValue("Win32.Constants", "MAX_PATH")))
        Assert.AreEqual(&H14, CInt(GetStaticFieldValue("Win32.Constants", "SPI_SETDESKWALLPAPER")))
        Assert.AreEqual(&H1, CInt(GetStaticFieldValue("Win32.Constants", "SPIF_UPDATEINIFILE")))
        Assert.AreEqual(&H2, CInt(GetStaticFieldValue("Win32.Constants", "SPIF_SENDCHANGE")))
    End Sub

    ''' <summary>
    ''' Verifies that the SPIF flags are distinct single bits, so combining them with <c>Or</c> is lossless.
    ''' </summary>
    <TestMethod>
    Public Sub SpifFlagsAreDistinctSingleBits()

        Dim updateIniFile As Integer = CInt(GetStaticFieldValue("Win32.Constants", "SPIF_UPDATEINIFILE"))
        Dim sendChange As Integer = CInt(GetStaticFieldValue("Win32.Constants", "SPIF_SENDCHANGE"))

        Assert.AreEqual(0, updateIniFile And (updateIniFile - 1), "SPIF_UPDATEINIFILE must be a single bit.")
        Assert.AreEqual(0, sendChange And (sendChange - 1), "SPIF_SENDCHANGE must be a single bit.")
        Assert.AreEqual(0, updateIniFile And sendChange, "SPIF flags must not overlap.")
        Assert.AreEqual(3, updateIniFile Or sendChange)
    End Sub

    ''' <summary>
    ''' Verifies the SystemParametersInfo P/Invoke metadata, its parameter types, and string marshaling.
    ''' </summary>
    <TestMethod>
    Public Sub SystemParametersInfoUsesTheExpectedPInvokeSignature()

        Dim method As MethodInfo = GetProductionMethod("Win32.NativeMethods", "SystemParametersInfo")

        Assert.IsTrue(method.IsStatic, "P/Invoke declarations must be static (module members).")
        Assert.AreEqual("SystemParametersInfo(Int32, Int32, String, Int32) As Int32", FormatSignature(method))

        Dim importAttribute As DllImportAttribute = DirectCast(
            Attribute.GetCustomAttribute(method, GetType(DllImportAttribute)), DllImportAttribute)

        Assert.IsNotNull(importAttribute, "DllImportAttribute was not found.")
        Assert.IsTrue(String.Equals("user32.dll", importAttribute.Value, StringComparison.OrdinalIgnoreCase))
        Assert.AreEqual("SystemParametersInfoW", importAttribute.EntryPoint, "The Unicode entry point must be used explicitly.")
        Assert.AreEqual(CharSet.Unicode, importAttribute.CharSet)
        Assert.IsTrue(importAttribute.SetLastError, "SetLastError is required by Marshal.GetLastWin32Error.")

        Dim marshalAttribute As MarshalAsAttribute = GetMarshalAs(method.GetParameters()(2))
        Assert.IsNotNull(marshalAttribute, "The wallpaper path must have an explicit marshaling attribute.")
        Assert.AreEqual(UnmanagedType.LPWStr, marshalAttribute.Value)
    End Sub

#End Region

#Region " IDesktopWallpaper "

    ''' <summary>
    ''' Verifies the COM interface identity.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperInterfaceHasTheExpectedComIdentity()

        Dim interfaceType As Type = GetProductionType(DesktopWallpaperInterfaceTypeName)

        Assert.IsTrue(interfaceType.IsInterface)
        Assert.IsTrue(interfaceType.IsImport, "The interface must be marked as a COM import.")
        Assert.AreEqual(New Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), interfaceType.GUID, "IID_IDesktopWallpaper")

        Dim interfaceTypeAttribute As InterfaceTypeAttribute = DirectCast(
            Attribute.GetCustomAttribute(interfaceType, GetType(InterfaceTypeAttribute)), InterfaceTypeAttribute)

        Assert.IsNotNull(interfaceTypeAttribute)
        Assert.AreEqual(ComInterfaceType.InterfaceIsIUnknown, interfaceTypeAttribute.Value)
        Assert.IsEmpty(interfaceType.GetInterfaces(), "The interface must not inherit other managed interfaces (vtable layout).")
        Assert.IsEmpty(interfaceType.GetProperties(), "Properties would add accessor slots to the vtable.")
        Assert.IsEmpty(interfaceType.GetEvents(), "Events would add accessor slots to the vtable.")
    End Sub

    ''' <summary>
    ''' Verifies the complete vtable: member order, parameter types, by-reference parameters, and return types.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperInterfaceMatchesTheNativeVTable()

        Dim actualVTable As String() = GetMembersInVTableOrder().Select(AddressOf FormatSignature).ToArray()

        Assert.AreSequenceEqual(ExpectedVTable, actualVTable)
    End Sub

    ''' <summary>
    ''' Verifies which members preserve the native HRESULT signature.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperInterfaceDeclaresTheExpectedPreserveSigMembers()

        Dim actualPreserveSigMembers As String() =
            GetMembersInVTableOrder().Where(AddressOf IsPreserveSig).Select(Function(method As MethodInfo) method.Name).ToArray()

        Assert.AreSequenceEqual(ExpectedPreserveSigMembers, actualPreserveSigMembers)
    End Sub

    ''' <summary>
    ''' Verifies the HRESULT convention for every member: a <see cref="PreserveSigAttribute"/> member must return
    ''' the HRESULT (Int32), and any other member must be a Sub (the runtime turns the HRESULT into an exception).
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperMembersFollowTheHResultConvention()

        For Each method As MethodInfo In GetMembersInVTableOrder()
            If IsPreserveSig(method) Then
                Assert.AreEqual(GetType(Integer), method.ReturnType, $"PreserveSig member '{method.Name}' must return the HRESULT.")
            Else
                Assert.AreEqual(GetType(Void), method.ReturnType, $"Non-PreserveSig member '{method.Name}' must be a Sub.")
            End If
        Next
    End Sub

    ''' <summary>
    ''' Verifies that every string parameter is explicitly marshaled as a null-terminated wide string
    ''' (<c>LPCWSTR</c> / <c>LPWSTR*</c>), never as the COM default (<c>BSTR</c>).
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperStringParametersAreMarshaledAsLPWStr()

        Dim checkedParameters As Integer = 0

        For Each method As MethodInfo In GetMembersInVTableOrder()
            For Each parameter As ParameterInfo In method.GetParameters()
                Dim parameterType As Type = If(parameter.ParameterType.IsByRef, parameter.ParameterType.GetElementType(), parameter.ParameterType)
                If parameterType Is GetType(String) Then
                    Dim marshalAttribute As MarshalAsAttribute = GetMarshalAs(parameter)
                    Assert.IsNotNull(marshalAttribute, $"Missing MarshalAs on {method.Name}.{parameter.Name}")
                    Assert.AreEqual(UnmanagedType.LPWStr, marshalAttribute.Value, $"{method.Name}.{parameter.Name}")
                    checkedParameters += 1
                End If
            Next
        Next

        Assert.AreEqual(7, checkedParameters, "Unexpected number of string parameters.")
    End Sub

    ''' <summary>
    ''' Verifies that the string output parameters are declared <see cref="OutAttribute"/>, so the marshaler
    ''' does not pass an input string and frees the callee-allocated (CoTaskMem) result.
    ''' </summary>
    '''
    ''' <param name="methodName">
    ''' The member name.
    ''' </param>
    '''
    ''' <param name="parameterIndex">
    ''' The zero-based index of the output parameter.
    ''' </param>
    <TestMethod>
    <DataRow("GetWallpaper", 1)>
    <DataRow("GetMonitorDevicePathAt", 1)>
    Public Sub DesktopWallpaperStringOutputParametersAreOutOnly(methodName As String, parameterIndex As Integer)

        Dim method As MethodInfo = GetProductionType(DesktopWallpaperInterfaceTypeName).GetMethod(methodName)
        Dim parameter As ParameterInfo = method.GetParameters()(parameterIndex)

        Assert.IsTrue(parameter.ParameterType.IsByRef, $"{methodName}.{parameter.Name} must be ByRef.")
        Assert.IsTrue(parameter.IsOut, $"{methodName}.{parameter.Name} must be marked <Out>.")
        Assert.IsFalse(parameter.IsIn, $"{methodName}.{parameter.Name} must not be marked <In>.")
    End Sub

    ''' <summary>
    ''' Verifies the COM activation class identifier.
    ''' </summary>
    <TestMethod>
    Public Sub DesktopWallpaperCoclassHasTheExpectedClassIdentifier()

        Dim coclassType As Type = GetProductionType("Win32.CDesktopWallpaper")

        Assert.IsTrue(coclassType.IsClass)
        Assert.IsTrue(coclassType.IsImport, "The coclass must be marked as a COM import.")
        Assert.AreEqual(New Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD"), coclassType.GUID, "CLSID_DesktopWallpaper")
        Assert.AreNotEqual(GetProductionType(DesktopWallpaperInterfaceTypeName).GUID, coclassType.GUID,
                           "The CLSID and the IID must not be swapped.")
    End Sub

#End Region

#Region " Private Helpers "

    ''' <summary>
    ''' Gets the <c>IDesktopWallpaper</c> members sorted by metadata token, which is their declaration
    ''' (and therefore vtable) order.
    ''' </summary>
    '''
    ''' <returns>
    ''' The interface members in vtable order.
    ''' </returns>
    Private Shared Function GetMembersInVTableOrder() As MethodInfo()

        Return GetProductionType(DesktopWallpaperInterfaceTypeName).
            GetMethods().
            OrderBy(Function(method As MethodInfo) method.MetadataToken).
            ToArray()
    End Function

    ''' <summary>
    ''' Determines whether a method is declared with <see cref="PreserveSigAttribute"/>
    ''' (a pseudo-attribute, stored as a method implementation flag).
    ''' </summary>
    '''
    ''' <param name="method">
    ''' The method to inspect.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the method preserves its native signature; otherwise, <see langword="False"/>.
    ''' </returns>
    Private Shared Function IsPreserveSig(method As MethodInfo) As Boolean

        Return (method.GetMethodImplementationFlags() And MethodImplAttributes.PreserveSig) = MethodImplAttributes.PreserveSig
    End Function

    ''' <summary>
    ''' Gets the <see cref="MarshalAsAttribute"/> of a parameter.
    ''' </summary>
    '''
    ''' <param name="parameter">
    ''' The parameter to inspect.
    ''' </param>
    '''
    ''' <returns>
    ''' The attribute, or <see langword="Nothing"/> if the parameter uses default marshaling.
    ''' </returns>
    Private Shared Function GetMarshalAs(parameter As ParameterInfo) As MarshalAsAttribute

        Return DirectCast(Attribute.GetCustomAttribute(parameter, GetType(MarshalAsAttribute)), MarshalAsAttribute)
    End Function

    ''' <summary>
    ''' Formats a method signature as <c>Name(Type, ByRef Type) As ReturnType</c>.
    ''' </summary>
    '''
    ''' <param name="method">
    ''' The method to format.
    ''' </param>
    '''
    ''' <returns>
    ''' The formatted signature.
    ''' </returns>
    Private Shared Function FormatSignature(method As MethodInfo) As String

        Dim parameterTypes As IEnumerable(Of String) =
            method.GetParameters().Select(Function(parameter As ParameterInfo) FormatType(parameter.ParameterType))

        Return $"{method.Name}({String.Join(", ", parameterTypes)}) As {FormatType(method.ReturnType)}"
    End Function

    ''' <summary>
    ''' Formats a type name, prefixing by-reference types with <c>ByRef</c>.
    ''' </summary>
    '''
    ''' <param name="type">
    ''' The type to format.
    ''' </param>
    '''
    ''' <returns>
    ''' The formatted type name.
    ''' </returns>
    Private Shared Function FormatType(type As Type) As String

        Return If(type.IsByRef, $"ByRef {type.GetElementType().Name}", type.Name)
    End Function

#End Region

End Class
