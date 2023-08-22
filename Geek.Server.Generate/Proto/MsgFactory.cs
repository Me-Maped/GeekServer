//auto generated, do not modify it

using System;
namespace Geek.Server.Proto
{
	public class MsgFactory
	{
		private static readonly System.Collections.Generic.Dictionary<int, Type> lookup;

        static MsgFactory()
        {
            lookup = new System.Collections.Generic.Dictionary<int, Type>(26)
            {
			    { 1435193915, typeof(Geek.Server.Proto.ReqBagInfo) },
			    { -1872884227, typeof(Geek.Server.Proto.ResBagInfo) },
			    { 225320501, typeof(Geek.Server.Proto.ReqComposePet) },
			    { 750865816, typeof(Geek.Server.Proto.ResComposePet) },
			    { 1686846581, typeof(Geek.Server.Proto.ReqUseItem) },
			    { -1395845865, typeof(Geek.Server.Proto.ReqSellItem) },
			    { 901279609, typeof(Geek.Server.Proto.ResItemChange) },
			    { 386917578, typeof(Geek.Server.Proto.NetConnectMessage) },
			    { 1599184588, typeof(Geek.Server.Proto.NetDisConnectMessage) },
			    { -679570763, typeof(Geek.Server.Proto.ReqConnectGate) },
			    { 2096149334, typeof(Geek.Server.Proto.ResConnectGate) },
			    { -1857713043, typeof(Geek.Server.Proto.ReqInnerConnectGate) },
			    { 1306001561, typeof(Geek.Server.Proto.ResInnerConnectGate) },
			    { -1734875143, typeof(Geek.Server.Proto.ReqClientChannelActive) },
			    { 1769619940, typeof(Geek.Server.Proto.ReqClientChannelInactive) },
			    { -1408529503, typeof(Geek.Server.Proto.ReqDisconnectClient) },
			    { -593677237, typeof(Geek.Server.Proto.UserInfo) },
			    { 1267074761, typeof(Geek.Server.Proto.ReqLogin) },
			    { 785960738, typeof(Geek.Server.Proto.ResLogin) },
			    { 1587576546, typeof(Geek.Server.Proto.ResLevelUp) },
			    { 1575482382, typeof(Geek.Server.Proto.HearBeat) },
			    { 1179199001, typeof(Geek.Server.Proto.ResErrorCode) },
			    { 537499886, typeof(Geek.Server.Proto.ResPrompt) },
			    { 299119425, typeof(Geek.Server.Proto.TestStruct) },
			    { 1250601847, typeof(Geek.Server.Proto.A) },
			    { -899515946, typeof(Geek.Server.Proto.B) },
            };
        }

        public static Type GetType(int msgId)
		{
			if (lookup.TryGetValue(msgId, out Type res))
				return res;
			else
				throw new Exception($"can not find msg type :{msgId}");
		}

	}
}
