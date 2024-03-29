﻿using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.Common.TestUtils
{
    public static class Certificates
    {
        public static readonly X509Certificate2 STSSignatureValidationTestCertificate = new X509Certificate2(Convert.FromBase64String(@"MIIGLDCCBRSgAwIBAgIEXOhOAjANBgkqhkiG9w0BAQsFADBJMQswCQYDVQQGEwJE
SzESMBAGA1UECgwJVFJVU1QyNDA4MSYwJAYDVQQDDB1UUlVTVDI0MDggU3lzdGVt
dGVzdCBYWFhJViBDQTAeFw0yMDAyMjUyMTIwMDVaFw0yMzAyMjUyMTE3MjFaMIGW
MQswCQYDVQQGEwJESzExMC8GA1UECgwoRGlnaXRhbGlzZXJpbmdzc3R5cmVsc2Vu
IC8vIENWUjozNDA1MTE3ODFUMCAGA1UEBRMZQ1ZSOjM0MDUxMTc4LUZJRDo1Njk0
MDQxMzAwBgNVBAMMKU5lbUxvZy1pbiBBREZTIFRlc3QgKGZ1bmt0aW9uc2NlcnRp
ZmlrYXQpMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwhqXoxvnrg1R
8GSAOmWdvR0lrdRjTss9zzJc/j0zHeamjsOyMdTc787Qdy2yre74ImzbBIRGi7HI
3PIP59PebFZnVOUa+/Y1enuQ8HZgJnaZqMCxPrQ+SuU3iD99WTvWtz6x+W1BLGiI
JK5Xkz8y3Ya82OZ25LwU+VXEovYEmPFNECV7z9yWkCcLZrfF4mwKbIdr96o9uFav
tiThMSD/XkD40ahdvDap13tV+HBBIVwa0gAjYe1Cz2jYPRvCVCy5adDgoDrP7WTS
ipvga7A9Y8WIZ52nDLgUlq67rK416trp6zThXAyzpDS98e99MVPy32Ovpqvw8phd
LqT6Tq8y3wIDAQABo4ICzDCCAsgwDgYDVR0PAQH/BAQDAgO4MIGXBggrBgEFBQcB
AQSBijCBhzA8BggrBgEFBQcwAYYwaHR0cDovL29jc3Auc3lzdGVtdGVzdDM0LnRy
dXN0MjQwOC5jb20vcmVzcG9uZGVyMEcGCCsGAQUFBzAChjtodHRwOi8vZi5haWEu
c3lzdGVtdGVzdDM0LnRydXN0MjQwOC5jb20vc3lzdGVtdGVzdDM0LWNhLmNlcjCC
ASAGA1UdIASCARcwggETMIIBDwYNKwYBBAGB9FECBAYEAzCB/TAvBggrBgEFBQcC
ARYjaHR0cDovL3d3dy50cnVzdDI0MDguY29tL3JlcG9zaXRvcnkwgckGCCsGAQUF
BwICMIG8MAwWBURhbklEMAMCAQEagatEYW5JRCB0ZXN0IGNlcnRpZmlrYXRlciBm
cmEgZGVubmUgQ0EgdWRzdGVkZXMgdW5kZXIgT0lEIDEuMy42LjEuNC4xLjMxMzEz
LjIuNC42LjQuMy4gRGFuSUQgdGVzdCBjZXJ0aWZpY2F0ZXMgZnJvbSB0aGlzIENB
IGFyZSBpc3N1ZWQgdW5kZXIgT0lEIDEuMy42LjEuNC4xLjMxMzEzLjIuNC42LjQu
My4wgawGA1UdHwSBpDCBoTA8oDqgOIY2aHR0cDovL2NybC5zeXN0ZW10ZXN0MzQu
dHJ1c3QyNDA4LmNvbS9zeXN0ZW10ZXN0MzQuY3JsMGGgX6BdpFswWTELMAkGA1UE
BhMCREsxEjAQBgNVBAoMCVRSVVNUMjQwODEmMCQGA1UEAwwdVFJVU1QyNDA4IFN5
c3RlbXRlc3QgWFhYSVYgQ0ExDjAMBgNVBAMMBUNSTDE0MB8GA1UdIwQYMBaAFM1s
aJc5chmkNatk6vQRo4GH+Gk7MB0GA1UdDgQWBBRBCneiJ1Rbm3TO7S7J+lDgRq3I
FDAJBgNVHRMEAjAAMA0GCSqGSIb3DQEBCwUAA4IBAQCh4sfwYmyapqcQ/hfXye+m
qJQa9pTktjU6OVZ5XwqvnMSRAwDaix6jfkW+s1CDBRdHY/vpUEczOKW6piPd6x0O
cRrKghyK138yN8472EuNb3v+A4g5AvKgtrRIojRtzUHOOQyl7e5um49WQKEsl8FV
DxFPbWjzkLHoDsgnvyLZdOR00QjKJsKYVK6Xct1Rsh5zJZE9/LHAkKx0lBCMT2Uj
rYqIj3qR+yKQLutLJxhfaAiLj9h00dVNhUQKTb6cfgPkr/+41lg7CvaVas+wDhQD
SrsaTkF54V9u+h3q8+BC3IW6ObMOz84Ws+Rbol5d7pVkVJemvYD5uqCCA+YLG3Bj"));

        public static readonly X509Certificate2 StsCertificate = new X509Certificate2(Convert.FromBase64String(@"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"));
        public static readonly X509Certificate2 TestCertificate = new X509Certificate2("Data/Test.p12", "D0mm3dag");
        public static readonly X509Certificate2 ExpiredCertificate = new X509Certificate2("Data/expired.cert.cer");
        public static readonly X509Certificate2 RevokeCertificate = new X509Certificate2("Data/revoked.cert.cer");
    }
}
