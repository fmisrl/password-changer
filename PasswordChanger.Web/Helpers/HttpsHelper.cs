using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PasswordChanger.Web.Helpers
{
    public class HttpsHelper
    {
        private const string CertificatePath = "certificate.pfx";
        
        public static X509Certificate2 GetSelfSignedCertificate()
        {
            // var password = Guid.NewGuid().ToString();
            var password = "1c128af3-d676-48ce-8993-cac92043c66d";
            const string commonName = "localhost";

            var hashAlgorithm = HashAlgorithmName.SHA256;

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest($"cn={commonName}", rsa, hashAlgorithm, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                    X509KeyUsageFlags.DigitalSignature, false)
            );
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false)
            );

            var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

            var pfxData = certificate.Export(X509ContentType.Pfx, password);
            
            File.WriteAllBytes(CertificatePath, pfxData);
            var cert = X509CertificateLoader.LoadPkcs12FromFile(CertificatePath, password);
            
            // var cert = X509CertificateLoader.LoadPkcs12(pfxData, password);

            return cert;
        }

        public static X509Certificate2 GetKeyFromContainer(string containerName)
        {
            /*
            var password = "1c128af3-d676-48ce-8993-cac92043c66d";
            var parameters = new CspParameters
            {
                KeyContainerName = containerName
            };
            
            using var rsa = new RSACryptoServiceProvider(parameters); 
            File.WriteAllText("prova.txt", rsa.ToXmlString(true).ToCharArray());
            Console.WriteLine(rsa.ToXmlString(true));
            Console.WriteLine();
            
            var cert = X509CertificateLoader.LoadPkcs12(rsa.ExportCspBlob(true), password);

            return cert;
            */
            
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindBySerialNumber, "00B44A99082D840E5E", false);
            var certificate = certificates[0];
            
            store.Close();
            
            return certificate;
        }
    }
}