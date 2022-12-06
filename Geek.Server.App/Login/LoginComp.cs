﻿using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Storage;
using System.Collections.Concurrent;

namespace Geek.Server.App.Login
{

    public class PlayerInfo : InnerState
    {
        //player相对特殊，id不是long，所以不继承DBState，自定义mongoDB的id
        public string playerId;
        public int SdkType;
        public string UserName;

        //这里设定每个账号在1服只有能创建1个角色 
        public Dictionary<int, long> RoleMap = new Dictionary<int, long>();

        [BsonIgnore]
        public bool IsChanged;
    }

    public class LoginState : CacheState
    {
        public ConcurrentDictionary<string, PlayerInfo> PlayerMap = new();
    }


    [Comp(ActorType.Login)]
    public class LoginComp : StateComp<LoginState>
    {

    }

}
