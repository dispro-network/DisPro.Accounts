# setup certificate properties including the commonName (DNSName) property for Chrome 58+
$website = "dispro.network.local"
$password = "SuperSecretPassword"

$certificate = New-SelfSignedCertificate `
    -DnsName $website, "*.$website" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -NotBefore (Get-Date) `
    -NotAfter (Get-Date).AddYears(2) `
    -CertStoreLocation "cert:CurrentUser\My" `
    -FriendlyName "Wildcard Certificate for *.$website" `
    -HashAlgorithm SHA256 `
    -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") 
$certificatePath = 'Cert:\CurrentUser\My\' + ($certificate.ThumbPrint)  

# create temporary certificate path
$certOutputPath = "..\certificates"
If (!(test-path $certOutputPath)) {
    New-Item -ItemType Directory -Force -Path $certOutputPath
}

# set certificate password here
$pfxPassword = ConvertTo-SecureString -String $password -Force -AsPlainText
$pfxFilePath = "$certOutputPath\$website.pfx"
$cerFilePath = "$certOutputPath\$website.cer"

# create pfx certificate
Export-PfxCertificate -Cert $certificatePath -FilePath $pfxFilePath -Password $pfxPassword
Export-Certificate -Cert $certificatePath -FilePath $cerFilePath

# import the pfx certificate
Import-PfxCertificate -FilePath $pfxFilePath Cert:\LocalMachine\My -Password $pfxPassword -Exportable


# create pem file for using with create-react-app
$keyPath = "$certOutputPath\$website-key.pem"
$certPath = "$certOutputPath\$website-cert.pem"
$outPath = "$certOutputPath\$website.pem"

openssl pkcs12 -in $pfxFilePath -nocerts -out $keyPath -nodes -passin pass:$password
openssl pkcs12 -in $pfxFilePath -nokeys -out $certPath -nodes -passin pass:$password

$key = Get-Content $keyPath
$cert = Get-Content $certPath
$key + $cert | Out-File $outPath -Encoding ASCII


# trust the certificate by importing the pfx certificate into your trusted root
Import-Certificate -FilePath $cerFilePath -CertStoreLocation Cert:\CurrentUser\Root