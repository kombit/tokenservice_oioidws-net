global using System;
global using System.Collections.Generic;
global using System.Configuration;
global using System.IdentityModel.Tokens;
global using System.IO;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Security;
global using System.Security.Cryptography.X509Certificates;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Xml;
global using System.Xml.Serialization;
global using Digst.OioIdws.OioWsTrustCore;
global using Digst.OioIdws.WscCore.OioWsTrust;
global using Digst.OioIdws.IntegrationTests.TokenAcquisition;

global using Microsoft.VisualStudio.TestTools.UnitTesting;

// Explicitly disable parallelization for integration tests
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]