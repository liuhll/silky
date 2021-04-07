using System;

namespace Silky.Lms.Rpc.Address.Selector
{
    public class RandomAddressSelector : AddressSelectorBase
    {
        private readonly Func<int, int, int> _generate;
        private readonly Random _random;

        public RandomAddressSelector()
        {
            _random = new Random((int)DateTime.Now.Ticks);
            _generate = (min, max) => _random.Next(min, max);
        }

        public override AddressSelectorMode AddressSelectorMode { get; } = AddressSelectorMode.Random;

        protected override IAddressModel SelectAddressByAlgorithm(AddressSelectContext context)
        {
             var index = _generate(0, context.AddressModels.Length);
             return context.AddressModels[index];
        }
    }
}