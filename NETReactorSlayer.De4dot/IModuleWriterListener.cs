using dnlib.DotNet.Writer;

namespace NETReactorSlayer.De4dot;

public interface IModuleWriterListener
{
    void OnWriterEvent(ModuleWriterBase writer, ModuleWriterEvent evt);
}