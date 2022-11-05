using System;

namespace Silky.Rpc.Endpoint.Selector
{
    public class RandomRpcEndpointSelector : RpcEndpointSelectorBase
    {
        private readonly Func<int, int, int> _generate;
        private readonly Random _random;

        public RandomRpcEndpointSelector()
        {
            _random = new Random((int)DateTime.Now.Ticks);
            _generate = (min, max) => _random.Next(min, max);
        }

        public override ShuntStrategy ShuntStrategy { get; } = ShuntStrategy.Random;

        protected override ISilkyEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context)
        {
            var index = _generate(0, context.AddressModels.Length);
            return context.AddressModels[index];
        }
    }
}