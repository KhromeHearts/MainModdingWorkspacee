using System.Runtime.CompilerServices;

#if DEBUG
// Unit tests (tests/NeoModLoader.Tests) exercise internal Mod Browser logic. Debug only: Release builds are
// strong-name signed, which would require the test assembly to be signed too.
[assembly: InternalsVisibleTo("NeoModLoader.Tests")]
#endif
