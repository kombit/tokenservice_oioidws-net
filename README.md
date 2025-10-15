README
======

## Getting started with OIOIDWS.Net.Core

`OIOIDWS.Net.Core` is a `.Net Core`- based reference implementation of the `OIOIDWS 1.0.1a` profile.

The `OIOIDWS.Net.Core` components can be used by service providers to act as a Web
Service Consumer (`WSC`) using the `SOAP` standard. Note that this Core implementation does not have functionality needed by a Web Service Producer (`WSP`).

This is the codebase that the `OIOIDWS.Net.Core` components are built from. All WSP examples found in this repository uses .Net framework.

### Resource links

* [Project maintenance][project]
* [Nuget packages (prefixed Kombit.OioIdws)][nuget]
* [Code repository][github]

[project]: https://digitaliser.dk/group/705156
[nuget]:   https://www.nuget.org/profiles/Digitaliseringsstyrelsen
[github]: https://github.com/digst/OIOIDWS.Net.Core

### Repository content

* **Build**: Contains build files that are used to build and publish the OIOIDWS.Net.Core Nuget package.

* **Docs**: Contains guidelines about how to use OIOIDWS.Net.Core.

* **Examples**: Contains examples that illustrates how to use `OIOIDWS.Net.Core`. In the Solution Explorer view in Visual Studio, the folder name is `Examples.Core`.

  * **Digst.OioIdws.WscExampleCommon**: This is a common code library shared across .NET Core client examples.
  * **Digst.OioIdws.WscExampleCore**: This example demonstrates how to use `OIOIDWS.Net.Core` to implement a .NET Core WSC that implements the SOAP variant of the OIOIDWS profile. Specifically, it shows how to obtain a direct token from the KOMBIT STS and then call a WSP using that token.
  * **Digst.OioIdws.WscLocalTokenExampleCore**: This example demonstrates how to use `OIOIDWS.Net.Core` to implement a .NET Core WSC that implements the SOAP variant of the OIOIDWS profile. Specifically, it shows how to obtain an `OBO token` from the KOMBIT STS and then call a WSP using that token.
  * **Digst.OioIdws.WscProxyOBOExampleCore**: This example demonstrates how to use `OIOIDWS.Net.Core` to implement a .NET Core WSC that implements the SOAP variant of the OIOIDWS profile. Specifically, it shows how to obtain a `proxy OBO token` from the KOMBIT STS and then call a WSP using that token.
  * **Digst.OioIdws.WscExampleCoreNuget**: This example is the same as `Digst.OioIdws.WscExampleCore`, but based on the latest public available `Digst.OioIdws.WscCore` NuGet package.
  * **Digst.OioIdws.WscExampleCoreWithCode**: This example is the same as `Digst.OioIdws.WscExampleCore`, but based on the latest public available `Digst.OioIdws.WscCore` NuGet package. It also initializes the Configuration object by code and eliminates the use of the app.config file.
  * **Digst.OioIdws.DotnetWscJavaWspExampleCore**: This example demonstrates how to run the .NET Core `WSC/Client` in the SOAP variant of OIOIDWS in the direct token scenario against a Java `WSP/Server`. Note that a Java `WSP/Server` must be up and running for this example to work. See the WSP section for instructions on how to prepare and run the `Java WSP`.

* **Misc**: Contains various resources related to the project.

  * **Certificates**: Includes all necessary certificates for running the examples.
  * **SOAP examples**: Provides examples of requests and responses for both OioWsTrust communication between WSC <-> STS and WSC <-> WSP.
  * **Specifications**: Contains all PDF specifications related to OIOIDWS. These specifications are named [spec-name] and are referenced by that name. They are included solely to document the specifications as they existed during the development of the project.
  * **Token examples**: Features examples of tokens issued by the KOMBIT STS.

* **Setup**: This folder contains PowerShell scripts that automatically set up the development environment.

* **Source**: This folder contains the source code of the `OIOIDWS.Net.Core` library.

  * **Digst.OioIdws.CommonCore**: This is a common code library shared across .NET Core projects.
  * **Digst.OioIdws.OioWsTrustCore**: This project contains the .NET Core implementation of the [`OIO-WST`] specification.
  * **Digst.OioIdws.SoapCore**: This project contains the .NET Core implementation of the [`OIO IDWS SOAP 1.1`] specification.
  * **Digst.OioIdws.WscCore**: This project encapsulates the usage and configuration of `Digst.OioIdws.SoapCore` and `Digst.OioIdws.OioWsTrustCore`.

* **Tests**: This folder contains various unit and integration test projects:
  
  * **Digst.OioIdws.Common.TestUtils**: This project contains common code used by the other test projects.
  * **Digst.OioIdws.CommonCore.Test**: This project contains tests for the Digst.OioIdws.CommonCore project.
  * **Digst.OioIdws.IntegrationTests**: This project contains integration tests for all normal STS, OBO token and proxy OBO token cases.
  * **Digst.OioIdws.OioWsTrustCore.Test**: This project contains tests for the Digst.OioIdws.CommonCore Digst.OioIdws.OioWsTrustCore projects.
  * **Digst.OioIdws.SoapCore.Test**: This project contains tests for the Digst.OioIdws.SoapCore project.

* **WspExamples**: Contains examples that illustrates a WSP application.

  * **Digst.OioIdws.WspExample**: This is an WSP example hosted in a managed application (e.g. a .NET console application) that the WSC examples can call.
  * **Digst.OioIdws.WspSvcExample**:This is an WSP example hosted in an an Azure App Service that the WSC examples can call.
  * **Digst.OioIdws.Wsp**: This project encapsulates the usage and configuration of Digst.OioIdws.Soap.
  * **Digst.OioIdws.Wsp.Wsdl**: This project is part of the `Digst.OioIdws.Wsp` project and provides cross-platform capabilities for exposing `ServiceMetadata` (`WSDL`) by the `.NET WSP`. Usage is _optional_, but highly recommened as it will ease and minimize the amout of manual task for non-`.NET WSC` consuming the `.NET WSP`.
  * **Digst.OioIdws.Common**: This is a common code library shared across WSP example projects.
  * **Digst.OioIdws.Soap**: This project contains the implementation of the [`OIO IDWS SOAP 1.1`] specification.

* **DEVELOPER-NOTES.md**: This file contains information relevant to maintainers of `OIOIDWS.Net.Core`.
* **Digst.OioIdws.Core.sln**: This Visual Studio solution file contains all .NET Core projects, including example projects, but does not contain all WSP-related projects.
* **Digst.OioIdws.Core with WSP.sln**: This Visual Studio solution file contains all projects, including all WSP-related projects.
* **README.md**: This file.

### Getting started

The source code contains everything you need to set up a demonstration environment that federates with the `KOMBIT STS`.

_The full documentation of `OIOIDWS.Net.Core` is a combination of the various readme files, `API` documentation, and provided examples._

For a quick setup, follow these steps:

* Run the script `Setup\setup_prerequisites.ps1` from an elevated `PowerShell`. This installs all required certificates and performs `sslcert` bindings to host local websites using `HTTPS`.
* Open the solution `Digst.OioIdws.Core.sln` in `Visual Studio 2022` and build it (if you get errors on external dependencies, ensure `NuGet` packages are being restored).
* The external `IP` address must be whitelisted at `NETS` to make revocation check of the test `FOCES` certificates.
* Run one of example projects (`Digst.OioIdws.WscExampleCore`, `Digst.OioIdws.WscLocalTokenExampleCore`, `Digst.OioIdws.WscProxyOBOExampleCore`) for different scenarios. Due to restrictions on what service can use OBO use cases, you will likely only be able to try the `Digst.OioIdws.WscExampleCore` example out successfully.

To try the normal STS case against a `Java WSP`:

* Refer to the `Configuring WSP` section for how to set up and start the `Java WSP`
* Run the `Digst.OioIdws.DotnetWscJavaWspExampleCore` project to start a signature case scenario against a `Java WSP`. 

This should start a console application that will request an token from KOMBIT STS then use it to call the target WSP.

### Configuring WSP

#### .NET Framework

To run a local WSP, you need to use the `Digst.OioIdws.Core with WSP.sln' solution file. Right-click on the `Digst.OioIdws.WspExample` project and select `Debug` => `Start Without Debugging`. The WSP service will be ready to listen at https://Digst.OioIdws.Wsp:9899/HelloWorld. Refer to the `Configuring WSP` section for how to configure them.

The `Digst.OioIdws.WspExample` is a .NET WSP that supports both SOAP 1.1 and 1.2. To specify the SOAP version, you need to configure the `useSoap12` attribute in the `SoapBindingConfiguration` in the App.config file:

```XML
<binding name="SoapBindingConfiguration" useHttps="true" sendTimeout="00:15:00" useSoap12="false"/>
```

**Configure Trusted issuers**

The `trustedIssuers` list must contain the thumbprint of the signing certificate that a KOMBIT STS instance is using so that the WSP can accept security tokens from that STS:

```XML
<issuerNameRegistry>
  <trustedIssuers>
    <add name="sts.oioidws-net.dk" thumbprint="8081b09446a396ba0ff9b7159d07d8c90f7db9ae"/>
    <add name="test-ekstern-adgangsstyring" thumbprint="0aa7a193f18d095f7e2ce09d892178c9682b7924"/>
  </trustedIssuers>
</issuerNameRegistry>      
```

**Configure Audience Uris**

```
<audienceUris>
  <add value="https://wsp.oioidws-net.dk"/>
  <add value="http://wsp11.oioidws-net.dk/service/service/1"/>
  <add value="http://wsp12.oioidws-net.dk/service/service/1"/>
</audienceUris>
```

The Digst.OioIdws.WspSvcExample example is the same as the Digst.OioIdws.WspExample example, except the fact that it is an ASP.NET-based WCF that we can host on Azure App Service or IIS instead of running as a managed application (e.g. a .NET console application).

All settings are usually pre-configured correctly, so you do not need to make any changes though.

#### Java

We will use the Java WSP example at https://github.com/digst/OIOIDWS.Java as the WSP to test the **Digst.OioIdws.DotnetWscJavaWspExampleCore** example

Required steps to run the Holder-Of-Key Java WSP:

* Download Java 8 from https://www.oracle.com/java/technologies/downloads/#java8-windows
* Create a ``JAVA_HOME`` environment variable with value "C:\Program Files\Java\jdk1.8.0_351"
* Add "%JAVA_HOME%\bin" to the ``Path`` environment variable
* Add the STS certificate into service's ``trust.jks``
  * Access https://n2adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/oauth2/certs.idp to get the certificate, or ask KOMBIT if you want to use another instance of KOMBIT STS, and save it as ``kombitsts.cer``. Note that, the kombitsts.cer must include “-----BEGIN CERTIFICATE-----” and “-----END CERTIFICATE-----” - this is a requirement from Java’s keytool.
  * Follow the instructions in the ``Update STS certificate`` at https://github.com/digst/OIOIDWS.Java#troubleshooting. Note that we are going to use the Holder-Of-Key WSP at "OIOIDWS.Java\examples\oio-idws-soap\service-hok"
  * You can copy the ``/Setup/create_trust.cmd`` file to ``OIOIDWS.Java\examples\oio-idws-soap\service-hok\src\main\resources`` along with the ``kombitsts.cer`` to import the STS certificate to ``trust.jks``
* Add restricted audience
	* Locate the``OIOIDWS.Java\examples\oio-idws-soap\service-hok\src\main\java\service\saml\DigstSamlAssertionValidator.java`` class and add the audience URI (http://wsp12.oioidws-java.dk/service/service/1)
	* See https://github.com/digst/OIOIDWS.Java/blob/master/examples/oio-idws-soap/service-hok/src/main/java/service/saml/DigstSamlAssertionValidator.java for how audiences are validated.
* Run the Java WSP: Follow this guide https://github.com/digst/OIOIDWS.Java#start-the-service-hok-java-wsp
	* Download Maven package https://maven.apache.org/download.cgi and install it https://maven.apache.org/install.html.
	* Start the service-hok Java WSP: cd examples/oio-idws-soap/service-hok mvn tomcat7:run-war
* The Java WSP now is up, running and listening at https://localhost:8443/HelloWorld/services/helloworld. You can check the wsdl at https://localhost:8443/HelloWorld/services/helloworld?wsdl

### Update dotnet-svcutil

To add service reference to an OIOIDWS WSP from Visual Studio, your machine needs to have `dotnet-svcutil` version 2.1.0 or newer. You can update the tool to the latest version using the following command:

```CMD
dotnet tool install --global dotnet-svcutil
```

Reference: https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-svcutil-guide?tabs=dotnetsvcutil2x

