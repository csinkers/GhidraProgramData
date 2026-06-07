namespace GhidraProgramData;

public record DecompiledFunction(
    uint Address,
    int FirstLineNumber,
    string Name,
    string[] Lines,
    uint[] LineAddresses,
    uint StackOffset,
    uint[] ExitPoints
);