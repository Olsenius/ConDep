using System;
using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl.Operations.Application.Deployment.Certificate;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteCertDeploymentBuilder : IOfferRemoteCertDeployment
    {
        private readonly IManageRemoteSequence _remoteSequence;
        private readonly IOfferRemoteDeployment _remoteDeploymentBuilder;

        public RemoteCertDeploymentBuilder(IManageRemoteSequence remoteSequence, IOfferRemoteDeployment remoteDeploymentBuilder)
        {
            _remoteSequence = remoteSequence;
            _remoteDeploymentBuilder = remoteDeploymentBuilder;
        }

        public IOfferRemoteDeployment FromStore(X509FindType findType, string findValue)
        {
            return FromStore(findType, findValue, null);
        }

        public IOfferRemoteDeployment FromStore(X509FindType findType, string findValue, Action<IOfferCertificateOptions> options)
        {
            var certOptions = new CertificateOptions();
            if (options != null)
            {
                options(certOptions);
            }

            var certOp = new CertificateFromStoreOperation(findType, findValue, certOptions);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _remoteDeploymentBuilder;
        }

        public IOfferRemoteDeployment FromFile(string path, string password)
        {
            return FromFile(path, password, null);
        }

        public IOfferRemoteDeployment FromFile(string path, string password, Action<IOfferCertificateOptions> options)
        {
            var certOptions = new CertificateOptions();
            if (options != null)
            {
                options(certOptions);
            }

            var certOp = new CertificateFromFileOperation(path, password, certOptions);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _remoteDeploymentBuilder;
        }
    }
}