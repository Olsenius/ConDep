using System;
using System.Security.Cryptography.X509Certificates;

namespace ConDep.Dsl
{
    public interface IOfferRemoteCertDeployment
    {
        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        IOfferRemoteDeployment FromStore(X509FindType findType, string findValue);

        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server with provided options.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IOfferRemoteDeployment FromStore(X509FindType findType, string findValue, Action<IOfferCertificateOptions> options);

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IOfferRemoteDeployment FromFile(string path, string password);

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server with provided options.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IOfferRemoteDeployment FromFile(string path, string password, Action<IOfferCertificateOptions> options);
    }
}