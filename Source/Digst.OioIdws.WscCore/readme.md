Welcome to Digst.OioIdws.WscCore
========

## Introduction

Digst.OioIdws.WscCore is a .Net Core-based reference implementation of the OIOIDWS 1.1 profile which is described at http://digitaliser.dk/resource/526486.
This package can be used by service providers to act as a Web Service Consumer (WSC).
The goal of this component is to make it easy for Web Service Consumers (WSC) to support the OIO Identity-based Web Services (OIOIDWS) profile and the KOMBIT STS profile. The supported use cases are the signature token (aka normal or direct STS), OBO token, and proxy OBO token.

KOMBIT STS is currently issuing unencrypted holder-of-key tokens and therefore this component is currently configured to statically work with holder-of-key tokens when SAML assertions are not encrypted.

The implementation is based on [Snitfladebeskrivelse-STS-OIO WS Trust] and [Snitfladebeskrivelse-STS-OnBehalfOf Proxy] for communication with KOMBIT STS and [OIO-IDWS-SOAP] for communication with a web service provider (WSP).
[Snitfladebeskrivelse-STS-OIO WS Trust] and [Snitfladebeskrivelse-STS-OnBehalfOf Proxy] are used for testing an implementation. The remaining proprietary standards that are directly or indirectly referenced through [Snitfladebeskrivelse-STS-OIO WS Trust], [Snitfladebeskrivelse-STS-OnBehalfOf Proxy] and [OIO-IDWS-SOAP] are also shortly described to get an overview of their internal dependencies.

[Snitfladebeskrivelse-STS-OIO WS Trust] and [Snitfladebeskrivelse-STS-OnBehalfOf Proxy] - KOMBIT STS 1.2: The purpose of these documents is to describe how web service providers and web service consumers can test the integration to the Security Token Service from here on named STS in the KOMBIT external test environment.

[SF1512 - Sikkerhed - Security Token Service, indgående_2.1] - This document describes the tasks that must be carried out in relation to KOMBIT integration, so that a municipality can use the KOMBIT STS through a user system.

[OIO-IDWS] - OIO Identity-Based Web Services 1.1: This document describes the overall business goals and requirements and shows how the different OIO profiles are combined to achieve these. Scenario 1 specifies that either WS-Security or a Liberty WSF-Profile can be used. Scenario 4 mandates OIO-IDWS-SOAP between WSC and WSP.

[OIO-WST-DEP] - OIO WS-Trust Deployment Profile Version 1.0: Mandates the use of OIO-IDWS-SOAP without the <sbf:Framework> element. Specifies that tokens SHOULD follow the OIO-IDT profile.

[OIO-IDWS-SOAP] - OIO IDWS SOAP Binding Profile Version 1.1: OIO-IDWS-SOAP is a scaled-down version of the "Liberty ID-WSF 2.0 profile" from 2006 and can be used without reading the "Liberty ID-WSF 2.0 profile". Liberty ID-WSF 2.0 profiles WS-Security, WS-Addressing and SAML. OIO-IDWS-SOAP specifies the use of SOAP 1.1, WS-Addressing 1.0 and WS-Security. WS-Security 1.0 namespaces are used in the examples but the reference list points to WS-Security 1.1. This implementation uses WS-Security 1.0 in order to be compliant with the examples. Furthermore, it mandates the use of SAML 2.0 assertions.

[OIO-WST] - OIO WS-Trust Profile 1.0.1: This profile is a true subset of WS-Trust 1.3 with the addition of the element `<wst14:ActAs>` from WS-Trust 1.4. KOMBIT STS uses `OnBeHalfOf` instead of `ActAs`.

[OIO-IDT] - OIO SAML Profile for Identity Tokens 1.0: Specifies that only "Holder of key" confirmation method should be allowed. The implementation has not been tested with bearer tokens.

They are also located in the "Misc\Specifications" folder. It is the copies on Softwarebørsen that this implementation follows.

Requirements:

- .Net 6.0 or newer.
- Transport Layer Security (TLS):
  - The "OIO IDWS SOAP 1.1" specification states that in order to maintain "Message Confidentiality", "a secure transport protocol with strong encryption such as 'TLS 1.2' MUST be used.".
  - As '.NET' doesn't have support to enforce this setting for 'WCF Services', this must be done on an 'Operating System' level by using a tool like IIS Crypto (https://www.nartac.com/Products/IISCrypto) (freeware) where a template, for example, 'PCI 3.1', can be chosen and afterward by unmarking all protocols, except 'TLS 1.2', will ensure to enforce this requirement.
  - Nowadays, WSPs and STS are usually configured to only allow 'TLS 1.2' and up, and client applications written .NET can pick up the right TLS algorithm to use automatically.

## How to use

Download `Digst.OioIdws.WscCore` package through NuGet. Open the configuration file and fill out all {REQUIRED} attributes. Also, fill out all {OPTIONAL} attributes or remove these if not needed. See configuration file Digst.OioIdws.WscCore.OioWsTrust.OioIdwsWcfConfigurationSection for details about each configuration element.

The component has two implementations of the interface Digst.OioIdws.OioWsTrustCore.IStsTokenService which can be initialized programmatically:

- StsTokenService: Retrieves a token from STS on each call
- StsTokenServiceCache: Retrieves a token from STS and caches the token for the duration of the token lifetime. STS is only called again if the token is not present in the cache.

Use the implementations through the Digst.OioIdws.OioWsTrust.IStsTokenService interface:

- SecurityToken GetToken(): Use this method in the signature case scenario
- SecurityToken GetTokenWithProxyOnBehalfOf(SecurityToken proxyOboToken): Use this method in the proxy OBO token scenario.
- SecurityToken GetTokenWithOnBehalfOf(SecurityToken oboToken): Use this method in the OnBehalfOf token scenario.

In order to use OIOIDWS.Net with production certificates ... the WSC and WSP must be registered in the KOMBIT STS system and the following certificates must be in place:

- The public certificate of the STS must be acquired. This certificate must be distributed out-of-band to both WSC and WSP. WSC to trust responses from STS and WSP to trust tokens from STS.
- The WSC must acquire a FOCES certificate. This certificate does not need to be distributed out-of-band to either STS or WSP. WSP indirectly trusts the WSC through the holder-of-key mechanism and STS trusts all FOCES certificates.
- The WSC must in the OnBehalfOf and proxy OnBehalfOf cases also acquire a FOCES certificate for the service that the WSC is acting on behalf of.
- The WSP must acquire a FOCES certificate. This certificate (the public part without the private key) must be distributed out-of-band to the WSC. The WSC needs it to trust responses from the WSP. The service must also be registered in the KOMBIT STS with an endpoint ID. This ID is used in both configurations of the WSC and WSP. The WSC needs the endpoint ID to request a token for a specific WSP. The WSP needs the endpoint ID to verify that the token is issued to the right WSP.
- Information about how to order FOCES certificates from NETS DANID can be found here: http://www.nets.eu/dk-da/Produkter/Sikkerhed/Funktionssignatur/Pages/default.aspx.
- To register a WSC and WSP you must contact kdi@kombit.dk. Detailed information can be found at https://digitaliseringskataloget.dk/

## Logging

The component supports logging using the WSC's logging framework. See Digst.OioIdws.CommonCore.Logging.LoggingConfigurationSection for details of how to do this. Please notice that tokens are written out when using the Debug level. This could expose a security risk when tokens with a valid lifetime are written to disk. Hence, do not use Debug level in production.

## Replay attack

- OioWsTrustCore: Replay attack mitigation against responses from STS has support for both a built-in MemoryDistributedCache (which is just a wrapper of MemoryCache) as well as any custom IDistributedCache that you want to use. The built-in MemoryDistributedCache is good for code that runs on a single machine. For load-balanced setups, you can use a distributed cache that implements the ObjectCache abstract class.

  ```CSharp
  public static StsTokenServiceConfiguration CreateConfiguration();
  public static StsTokenServiceConfiguration CreateConfiguration(IDistributedCache customReplayAttackCache)
  ```

- OioIdWsSoapCore: Replay attack mitigation against responses from WSP is the same as OioWsTrustCore. You can turn off replay attack protection by configuring a custom ReplayAttackCache. 

## Test

Manual man-in-the-middle attacks have been made using Fiddler. The following tests have been executed:

- Tampering response so that response signature does not validate.
- Removing the signature in response to ensure that WSC does not accept the response.
- Replay attack has been tested.
- Sending a response that has expired is not accepted by WSC.

The following are issues that Digst.OioIdws.WscCore takes care of because WCF did not support them out of the box:

WSC<->STS communication

- RST:

  - Ensure that the `/s:Envelope/s:Body/trust:RequestSecurityToken/wsp:AppliesTo/wsa:EndpointReference/wsa:Address` element does not contain an ending '/'. KOMBIT STS makes string comparison instead of URI comparison.

- RSTR:
  
  - No changes are needed. RSTRs that are returned from KOMBIT STS meet all requirements.

- SOAP Faults do not follow SOAP 1.1 spec.

WSC<->WSP communication

- Request:

  - Ensure that body is signed even if ProtectionLevel has been set to None. The Body must be signed as required by [OIO-IDWS-SOAP].
  - Adds header as required by [OIO-IDWS-SOAP].
  - Ensures that SecurityTokenReference has a different attribute id than the KeyIdentifier element value as in the examples in [OIO-IDWS-SOAP]. By default, they will have the same value. If nothing is done ... it would still work from a technical point of view.

- Response:

  - Added extra check that all [OIO-IDWS-SOAP] required WS-Addressing headers are present. E.g. WCF does not require that responses contain the MessageID header.
  - Do signature validation manually because WCF Core is unable to handle the type of security token in use. After the validation succeeds, the signature is removed from the message to avoid .NET built-in validation complaining about unsupported signatures. The original message is reserved in the Properties list of the modified response object.

    ```CSharp
    manipulatedResponse.Properties.Add("originalMessage", response);
    ```

  - Support for encrypted responses from WSP because SOAP faults are encrypted by WCF.
