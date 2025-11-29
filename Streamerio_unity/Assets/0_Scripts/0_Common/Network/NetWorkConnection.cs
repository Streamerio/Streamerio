using UnityEngine;

namespace Common.Network
{
    public interface INetworkConnection
    {
        bool CheckConnection();
    }

    public class NetworkConnection: INetworkConnection
    {
        public bool CheckConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }   
}
