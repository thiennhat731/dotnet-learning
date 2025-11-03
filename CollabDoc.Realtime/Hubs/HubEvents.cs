namespace CollabDoc.Realtime.Hubs
{
    public static class HubEvents
    {
        public const string InitialState = "InitialState";
        public const string NoInitialState = "NoInitialState";
        public const string RequestSync = "RequestSync";
        public const string YjsUpdate = "YjsUpdate";
        public const string AwarenessUpdate = "AwarenessUpdate";
        public const string UserJoined = "UserJoined";
        public const string UserLeft = "UserLeft";
        public const string ActiveUsers = "ActiveUsers";
    }
}