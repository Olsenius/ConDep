using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.Application.Local;
using ConDep.Dsl.Operations.Application.Local.PreCompile;
using ConDep.Dsl.Operations.Application.Local.TransformConfig;
using ConDep.Dsl.Operations.Application.Local.WebRequest;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.SemanticModel.Sequence;

namespace ConDep.Dsl.Builders
{
    public class LocalOperationsBuilder : IOfferLocalOperations, IConfigureLocalOperations
    {
        private readonly LocalSequence _localSequence;
        private readonly IManageInfrastructureSequence _infrastructureSequence;
        private readonly IEnumerable<ServerConfig> _servers;

        public LocalOperationsBuilder(LocalSequence localSequence, IManageInfrastructureSequence infrastructureSequence, IEnumerable<ServerConfig> servers)
        {
            _localSequence = localSequence;
            _infrastructureSequence = infrastructureSequence;
            _servers = servers;
        }

        public IOfferLocalOperations TransformConfigFile(string configDirPath, string configName, string transformName)
        {
            var operation = new TransformConfigOperation(configDirPath, configName, transformName);
            AddOperation(operation);
            return this;
        }

        public IOfferLocalOperations PreCompile(string webApplicationName, string webApplicationPhysicalPath, string preCompileOutputpath)
        {
            var operation = new PreCompileOperation(webApplicationName, webApplicationPhysicalPath,
                                                                          preCompileOutputpath);
            AddOperation(operation);
            return this;
        }

        public IOfferLocalOperations HttpGet(string url)
        {
            var operation = new HttpGetOperation(url);
            AddOperation(operation);
            return this;
        }

        public IOfferRemoteOperations ToEachServer(Action<IOfferRemoteOperations> action)
        {
            var builder = new RemoteOperationsBuilder(_localSequence.NewRemoteSequence(_infrastructureSequence, _servers));
            action(builder);
            return builder;
        }

        public void AddOperation(LocalOperation operation)
        {
            _localSequence.Add(operation);
        }
    }
}