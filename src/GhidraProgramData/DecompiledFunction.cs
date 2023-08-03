namespace GhidraProgramData;

public record DecompiledFunction(
    uint Address,
    string[] Lines,
    uint[] LineAddresses);