using System.Globalization;
using ConDep.Dsl.Builders;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Infrastructure.IIS.AppPool
{
    public class IisAppPoolOperation : RemoteCompositeInfrastructureOperation
    {
        private readonly string _appPoolName;
        private readonly IisAppPoolOptions.IisAppPoolOptionsValues _appPoolOptions;

        public IisAppPoolOperation(string appPoolName)
        {
            _appPoolName = appPoolName;
        }

        public IisAppPoolOperation(string appPoolName, IisAppPoolOptions.IisAppPoolOptionsValues appPoolOptions) 
        {
            _appPoolName = appPoolName;
            _appPoolOptions = appPoolOptions;
        }

        public override string Name
        {
            get { return "IIS Application Pool"; }
        }

        public override bool IsValid(Notification notification)
        {
            return !string.IsNullOrWhiteSpace(_appPoolName);
        }

        public override void Configure(IOfferRemoteComposition server, IOfferInfrastructure require)
        {
            var appPoolOptions = "$appPoolOptions = $null;";

            if(_appPoolOptions != null)
            {
                appPoolOptions = string.Format("$appPoolOptions = @{{Enable32Bit=${0}; IdentityUsername='{1}'; IdentityPassword='{2}'; IdleTimeoutInMinutes={3}; LoadUserProfile=${4}; ManagedPipeline={5}; NetFrameworkVersion={6}; RecycleTimeInMinutes={7}}};"
                    , _appPoolOptions.Enable32Bit.HasValue ? _appPoolOptions.Enable32Bit.Value.ToString() : "false"
                    , _appPoolOptions.IdentityUsername
                    , _appPoolOptions.IdentityPassword
                    , _appPoolOptions.IdleTimeoutInMinutes.HasValue ? _appPoolOptions.IdleTimeoutInMinutes.Value.ToString(CultureInfo.InvariantCulture.NumberFormat) : "$null"
                    , _appPoolOptions.LoadUserProfile.HasValue ? _appPoolOptions.LoadUserProfile.Value.ToString() : "false"
                    , _appPoolOptions.ManagedPipeline.HasValue ? "'" + _appPoolOptions.ManagedPipeline.Value + "'" : "$null"
                    , _appPoolOptions.NetFrameworkVersion ?? "$null"
                    , _appPoolOptions.RecycleTimeInMinutes.HasValue ? _appPoolOptions.RecycleTimeInMinutes.Value.ToString(CultureInfo.InvariantCulture.NumberFormat) : "$null"
                    );
            }
            server.ExecuteRemote.PowerShell(string.Format(@"{0} New-ConDepAppPool '{1}' $appPoolOptions;", appPoolOptions, _appPoolName), psOptions => psOptions.WaitIntervalInSeconds(30));
        }
    }
}