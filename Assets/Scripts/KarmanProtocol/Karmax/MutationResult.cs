using System;

namespace KarmanProtocol.Karmax {
    public class MutationResult {
        private readonly Fragment fragment;
        private readonly string failureReason;

        private MutationResult(Fragment fragment, string failureReason) {
            this.fragment = fragment;
            this.failureReason = failureReason;
        }

        public static MutationResult Ok(Fragment fragment) {
            if (fragment == null) {
                throw new ArgumentNullException("fragment");
            }
            return new MutationResult(fragment, null);
        }

        public static MutationResult Failure(string reason) {
            if (reason == null) {
                throw new ArgumentNullException("reason");
            }
            return new MutationResult(null, reason);
        }

        public bool IsFailure() {
            return failureReason != null;
        }

        public Fragment GetFragment() {
            return fragment;
        }

        public string GetFailureReason() {
            return failureReason;
        }
    }
}