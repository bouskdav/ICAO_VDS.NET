# ICAO_VDS.NET
C# port mixed from projects:

https://github.com/kurzdigital/vds-jvm

https://github.com/tsenger/vdstools

## how to use:

DigitalSeal digitalSeal = DataParser.parseVdsSeal(rawBytes);
VdsType vdsType = digitalSeal.getVdsType()
	
// Depending on the returned VDS type you can access the seals content
String mrz = (String) seal.getFeature(Feature.MRZ);
String azr = (String) seal.getFeature(Feature.AZR);
   
// Get the VDS signer certificate reference
String signerCertRef = digitalSeal.getSignerCertRef();
   
// Provide for the matching X509 signer certificate
// and use this to verify the VDS signature   
Verifier verifier = new Verifier(digitalSeal, x509SignerCert);
Verifier.Result result = verifier.verify();

## TODO:

Verifier not implemented