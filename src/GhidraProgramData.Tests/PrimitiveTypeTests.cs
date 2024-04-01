using FluentAssertions;
using GhidraProgramData.Types;

namespace GhidraProgramData.Tests;

public class PrimitiveTypeTests
{
    static void Test(string name, uint size)
    {
        var pd = new TestXmlBuilder().Load();
        var t = pd.Types[new TypeKey("", name)];
        t.Should().BeOfType<GPrimitive>();
        var st = (GPrimitive)t;
        st.FixedSize.Should().Be(size);
    }

    [Fact] public void BoolTest() => Test("bool", 1);
    [Fact] public void SByteTest() => Test("sbyte", 1);
    [Fact] public void WordTest() => Test("word", 2);
    [Fact] public void ShortTest() => Test("short", 2);
    [Fact] public void IntTest() => Test("int", 4);
    [Fact] public void LongTest() => Test("long", 4);
    [Fact] public void DwordTest() => Test("dword", 4);
    [Fact] public void LongLongTest() => Test("longlong", 8);
    [Fact] public void QwordTest() => Test("qword", 8);
    [Fact] public void ByteTest() => Test("byte", 1);
    [Fact] public void UCharTest() => Test("uchar", 1);
    [Fact] public void UShortTest() => Test("ushort", 2);
    [Fact] public void UIntTest() => Test("uint", 4);
    [Fact] public void ULongTest() => Test("ulong", 4);
    [Fact] public void ULongLongTest() => Test("ulonglong", 8);
    [Fact] public void UndefinedTest() => Test("undefined", 1);
    [Fact] public void Undefined1Test() => Test("undefined1", 1);
    [Fact] public void Undefined2Test() => Test("undefined2", 2);
    [Fact] public void Undefined4Test() => Test("undefined4", 4);
    [Fact] public void Undefined6Test() => Test("undefined6", 6);
    [Fact] public void Undefined8Test() => Test("undefined8", 8);
    [Fact] public void CharTest() => Test("char", 1);
    [Fact] public void FloatTest() => Test("float", 4);
    [Fact] public void DoubleTest() => Test("double", 8);
    [Fact] public void Float10Test() => Test("float10", 10);
    [Fact] public void VoidTest() => Test("void", 0);
    [Fact] public void VaListTest() => Test("va_list", 0);
    [Fact] public void ImageBaseOffset32Test() => Test("ImageBaseOffset32", 4);
    [Fact] public void Pointer32Test() => Test("pointer32", 4);
    [Fact] public void PointerTest() => Test("pointer", Constants.PointerSize);
    [Fact] public void SizeTTest() => Test("size_t", Constants.PointerSize);
}