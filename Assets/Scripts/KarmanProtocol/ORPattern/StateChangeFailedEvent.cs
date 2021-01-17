using Networking;
using System;

namespace KarmanProtocol.ORPattern {
    public class StateChangeFailedEvent : SharedStatePacket<object> {
        private readonly string sharedStateIdentifier;
        private readonly string packetTypeName;
        private readonly string reason;

        public StateChangeFailedEvent(byte[] bytes) : base(bytes) {
            sharedStateIdentifier = ReadString();
            packetTypeName = ReadString();
            reason = ReadString();
        }

        public StateChangeFailedEvent(Guid requestId, string sharedStateIdentifier, string packetTypeName, string reason) : base(
            requestId,
            Bytes.Pack(Bytes.Of(sharedStateIdentifier), Bytes.Of(packetTypeName), Bytes.Of(reason))
        ) {
            this.sharedStateIdentifier = sharedStateIdentifier;
            this.packetTypeName = packetTypeName;
            this.reason = reason;
        }

        public override bool IsValid() {
            return !string.IsNullOrEmpty(sharedStateIdentifier) && !string.IsNullOrEmpty(reason);
        }

        public new string GetSharedStateIdentifier() {
            return sharedStateIdentifier;
        }

        public string GetPacketName() {
            return packetTypeName;
        }

        public string GetReason() {
            return reason;
        }
    }
}