using System;
using System.Runtime.CompilerServices;

namespace Silky.Core.MethodExecutor
{
    public readonly struct ObjectMethodExecutorAwaitable
    {
        private readonly object _customAwaitable;
        private readonly Func<object, object> _getAwaiterMethod;
        private readonly Func<object, bool> _isCompletedMethod;
        private readonly Func<object, object> _getResultMethod;
        private readonly Action<object, Action> _onCompletedMethod;
        private readonly Action<object, Action> _unsafeOnCompletedMethod;


        public ObjectMethodExecutorAwaitable(
            object customAwaitable,
            Func<object, object> getAwaiterMethod,
            Func<object, bool> isCompletedMethod,
            Func<object, object> getResultMethod,
            Action<object, Action> onCompletedMethod,
            Action<object, Action> unsafeOnCompletedMethod)
        {
            _customAwaitable = customAwaitable;
            _getAwaiterMethod = getAwaiterMethod;
            _isCompletedMethod = isCompletedMethod;
            _getResultMethod = getResultMethod;
            _onCompletedMethod = onCompletedMethod;
            _unsafeOnCompletedMethod = unsafeOnCompletedMethod;
        }

        public Awaiter GetAwaiter()
        {
            var customAwaiter = _getAwaiterMethod(_customAwaitable);
            return new Awaiter(customAwaiter, _isCompletedMethod, _getResultMethod, _onCompletedMethod,
                _unsafeOnCompletedMethod);
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly object _customAwaiter;
            private readonly Func<object, bool> _isCompletedMethod;
            private readonly Func<object, object> _getResultMethod;
            private readonly Action<object, Action> _onCompletedMethod;
            private readonly Action<object, Action> _unsafeOnCompletedMethod;

            public Awaiter(
                object customAwaiter,
                Func<object, bool> isCompletedMethod,
                Func<object, object> getResultMethod,
                Action<object, Action> onCompletedMethod,
                Action<object, Action> unsafeOnCompletedMethod)
            {
                _customAwaiter = customAwaiter;
                _isCompletedMethod = isCompletedMethod;
                _getResultMethod = getResultMethod;
                _onCompletedMethod = onCompletedMethod;
                _unsafeOnCompletedMethod = unsafeOnCompletedMethod;
            }

            public bool IsCompleted => _isCompletedMethod(_customAwaiter);

            public object GetResult() => _getResultMethod(_customAwaiter);

            public void OnCompleted(Action continuation)
            {
                _onCompletedMethod(_customAwaiter, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                var underlyingMethodToUse = _unsafeOnCompletedMethod ?? _onCompletedMethod;
                underlyingMethodToUse(_customAwaiter, continuation);
            }
        }
    }
}