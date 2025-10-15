@echo off

set KEYTOOL="%JAVA_HOME%\bin\keytool.exe"

set PWD00="Test1234"
set CERT_STS=".\kombitsts.cer"
set KEYSTORE_STS=".\trust.jks"

:: STS
%KEYTOOL% -v -import -trustcacerts ^
  -destkeystore %KEYSTORE_STS% -deststoretype JKS ^
  -keypass %PWD00% -storepass %PWD00% ^
  -noprompt -alias remote-kombit-sts -file %CERT_STS%
:: NOTE: Ensure that alias matches the one in "trust.jks"

:: STS content
%KEYTOOL% -list ^
  -keystore %KEYSTORE_STS% ^
  -keypass %PWD00% -storepass %PWD00%


pause