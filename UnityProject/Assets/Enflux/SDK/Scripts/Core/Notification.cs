// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
namespace Enflux.SDK.Core
{
    public class Notification<T>
    {
        public Notification(T value, string message = "")
        {
            Value = value;
            Message = message;
        }

        public T Value { get; private set; }
        public string Message { get; private set; }
    }
}
