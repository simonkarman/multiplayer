﻿namespace KarmanNet.Networking {
    public class FactoryBytesException : NetworkingException {
        public FactoryBytesException(string reason):
            base(reason) {
        }
    }
}