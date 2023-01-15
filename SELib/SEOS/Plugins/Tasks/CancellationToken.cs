using System;

namespace IngameScript
{
    class CancellationToken
    {
#pragma warning disable 649
        public bool CancelRequest;
        public bool CompleteRequest;
        public Exception Exception;
#pragma warning restore 649

    }
}