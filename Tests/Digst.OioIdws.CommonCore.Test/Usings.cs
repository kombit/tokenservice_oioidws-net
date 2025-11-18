global using System.IdentityModel.Tokens;
global using System.Security.Cryptography.X509Certificates;

// Explicitly disable parallelization for integration tests
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]